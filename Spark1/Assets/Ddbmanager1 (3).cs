
using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;


public class Ddbmanager : MonoBehaviour
{
    private FirebaseFirestore db;
    public TMP_Text textUI;
    public Frame2Trigger frame2Trigger;
    public FrameTrigger frame3Trigger;
    public FrameTrigger frame4Trigger;

    public Button soundToggleButton;
    public Sprite soundOnSprite; 
    public Animator animator;
    public Sprite soundOffSprite;
    private AudioSource audioSource;
    private bool isMuted = true;
    private Dictionary<string, string> wordImages = new Dictionary<string, string>();
    private Dictionary<string, string> wordDefinitions = new Dictionary<string, string>();

    public Button previousButton;
    public Button nextButton;
public RawImage photoUI;         // Drag your RawImage here in the Inspector
public GameObject photoPanel;    // Optional: panel wrapping the RawImage

    private List<DocumentReference> currentDialogues = new List<DocumentReference>();
    private int currentDialogueIndex = 0;
    private bool isPlaying = false;

    private Coroutine autoAdvanceCoroutine;

    public TMP_Text definitionText;
    public GameObject definitionPanel;
    public RawImage definitionImage;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        definitionPanel.SetActive(false);
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.volume = isMuted ? 0f : 1f;

        if (soundToggleButton != null)
        {
            soundToggleButton.onClick.AddListener(ToggleSound);
            UpdateButtonSprite();
        }

        if (previousButton != null)
        {
            previousButton.onClick.AddListener(PreviousDialogue);
            previousButton.gameObject.SetActive(false);
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextDialogue);
            nextButton.gameObject.SetActive(false);
        }

        StartCoroutine(FetchFramesFromStory("story1"));
    }

    public void ToggleSound()
    {
        isMuted = !isMuted;
        audioSource.volume = isMuted ? 0f : 1f;
        UpdateButtonSprite();
    }

    public void MuteAudio()
    {
        isMuted = true;
        audioSource.volume = 0f;
        UpdateButtonSprite();
    }

    public void UnmuteAudio()
    {
        isMuted = false;
        audioSource.volume = 1f;
        UpdateButtonSprite();
    }

    void UpdateButtonSprite()
    {
        if (soundToggleButton != null && soundToggleButton.image != null)
        {
            soundToggleButton.image.sprite = isMuted ? soundOffSprite : soundOnSprite;
        }
    }

    IEnumerator FetchFramesFromStory(string storyId)
    {
        DocumentReference storyRef = db.Collection("stories").Document(storyId);
        var storyTask = storyRef.GetSnapshotAsync();
        yield return new WaitUntil(() => storyTask.IsCompleted);

        if (storyTask.IsFaulted)
        {
            Debug.LogError($"Failed to load story {storyId}: {storyTask.Exception}");
            yield break;
        }

        DocumentSnapshot storySnapshot = storyTask.Result;
        if (storySnapshot.Exists && storySnapshot.TryGetValue("dialogeList", out List<DocumentReference> frameList))
        {
            for (int i = 0; i < frameList.Count; i++)
            {
                DocumentReference frameRef = frameList[i];

                if (i == 1) // Frame 2
                {
                    Debug.Log("⏳ Waiting for Frame 2 trigger...");
                    yield return new WaitUntil(() => frame2Trigger.isTriggered);
                    Debug.Log("✅ Frame 2 triggered! Fetching dialogues...");
                }

                if (i == 2) // Frame 3 OR Frame 4 Selection
                {
                    Debug.Log("⏳ Waiting for Frame 3 OR Frame 4 trigger...");

                    // Wait until either Frame 3 or Frame 4 is triggered
                    yield return new WaitUntil(() => frame3Trigger.isTriggered || frame4Trigger.isTriggered);

                    if (frame3Trigger.isTriggered)
                    {
                        Debug.Log("✅ Frame 3 triggered! Fetching dialogues...");
                        frameRef = frameList[2]; // Set frame to Frame 3
                        yield return FetchDialoguesFromFrame(frameRef);
                        yield break; // 🔥 Exit the loop to prevent Frame 4 from running
                    }
                    else if (frame4Trigger.isTriggered)
                    {
                        Debug.Log("✅ Frame 4 triggered! Fetching dialogues...");
                        frameRef = frameList[3]; // Set frame to Frame 4
                        yield return FetchDialoguesFromFrame(frameRef);
                        if (i == 5) // Frame 6 trigger via Animator
                        {
                            Debug.Log("⏳ Waiting for Animator to enter 'afterActivity' state...");
                            yield return new WaitUntil(() =>
                            {
                                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // 0 = base layer
                                return stateInfo.IsName("afterActivity");
                            });

                            Debug.Log("✅ Animator is in 'afterActivity' state. Fetching Frame 6 dialogues...");

                            frameRef = frameList[5]; // ✅ Set the frameRef to Frame 6
                            yield return FetchDialoguesFromFrame(frameRef); // ✅ Fetch and play Frame 6
                            yield break; // ✅ Prevent further frames from executing
                        }
                        yield break; // 🔥 Exit the loop to prevent Frame 3 from running
                    }
                }
                





                yield return FetchDialoguesFromFrame(frameRef);
            }
        }
    }



    IEnumerator FetchDialoguesFromFrame(DocumentReference frameRef)
    {
        var frameTask = frameRef.GetSnapshotAsync();
        yield return new WaitUntil(() => frameTask.IsCompleted);

        if (frameTask.IsFaulted)
        {
            Debug.LogError($"❌ Failed to load frame {frameRef.Path}: {frameTask.Exception}");
            yield break;
        }

        DocumentSnapshot frameSnapshot = frameTask.Result;
        if (frameSnapshot.Exists && frameSnapshot.TryGetValue("listofDialoges", out List<DocumentReference> dialogueList))
        {
            currentDialogues = dialogueList;
            currentDialogueIndex = 0;

            if (currentDialogues.Count > 0)
            {
                yield return FetchAndPlayDialogue(currentDialogues[currentDialogueIndex].Id);
            }

            UpdateButtons();
        }
    }

    IEnumerator FetchAndPlayDialogue(string dialogueId)
    {
        while (isPlaying) yield return null;
        isPlaying = true;

        Debug.Log($"🎯 FetchAndPlayDialogue() called for ID: {dialogueId}"); // Debug log

        DocumentReference dialogueRef = db.Collection("Dialogues").Document(dialogueId);
        var dialogueTask = dialogueRef.GetSnapshotAsync();
        yield return new WaitUntil(() => dialogueTask.IsCompleted);

        if (!dialogueTask.IsFaulted)
        {
            DocumentSnapshot dialogueSnapshot = dialogueTask.Result;
            if (dialogueSnapshot.Exists)
            {
                string dialogueText = dialogueSnapshot.ContainsField("text") ? dialogueSnapshot.GetValue<string>("text") : "";
                string highlightedWord = dialogueSnapshot.ContainsField("word") ? dialogueSnapshot.GetValue<string>("word") : "";
                string wordMeaning = dialogueSnapshot.ContainsField("meaning") ? dialogueSnapshot.GetValue<string>("meaning") : "";
                string imageUrl = dialogueSnapshot.ContainsField("image") ? dialogueSnapshot.GetValue<string>("image") : "";
                string audioUrl = dialogueSnapshot.ContainsField("Audio") ? dialogueSnapshot.GetValue<string>("Audio") : "";
                // 🔽 Get photo field if it exists
               string photoUrl = dialogueSnapshot.ContainsField("visual") ? dialogueSnapshot.GetValue<string>("visual") : "";

if (!string.IsNullOrEmpty(photoUrl))
{
    Debug.Log("🖼️ Visual image URL found in document.");
    StartCoroutine(LoadPhoto(photoUrl));
}
else
{
    Debug.Log("ℹ️ No visual image URL found.");
    if (photoPanel != null) photoPanel.SetActive(false);
}

                Debug.Log($"📜 Dialogue Text: {dialogueText}"); // Debug log
                Debug.Log($"🔊 Audio URL Retrieved: {audioUrl}"); // Debug log

                if (textUI != null)
                {
                    textUI.text = HighlightWord(dialogueText, highlightedWord);
                }

                // ✅ Store word, meaning, and image
                if (!string.IsNullOrEmpty(highlightedWord))
                {
                    if (!wordDefinitions.ContainsKey(highlightedWord))
                    {
                        wordDefinitions[highlightedWord] = wordMeaning;
                    }

                    if (!wordImages.ContainsKey(highlightedWord))
                    {
                        wordImages[highlightedWord] = imageUrl;
                    }
                }

// ✅ Play audio if available
                if (!string.IsNullOrEmpty(audioUrl))
                {
                    Debug.Log($"🎵 Playing Audio from URL: {audioUrl}");
                    StartCoroutine(LoadAndPlayAudio(audioUrl));
                }
                else
                {
                    Debug.LogWarning("⚠️ No audio found for this dialogue.");
                }

                Debug.Log("🔄 Dialogue loaded successfully!");

                // ✅ Restart auto-advance after displaying dialogue
                RestartAutoAdvanceCoroutine();
            }
            else
            {
                Debug.LogWarning("⚠️ Dialogue document does not exist!");
            }
        }
        else
        {
            Debug.LogError($"❌ Failed to fetch dialogue {dialogueId}: {dialogueTask.Exception}");
        }

        isPlaying = false;
    }


    // ✅ Updated HighlightWord() function to support clickable words
    string HighlightWord(string dialogue, string word)
    {
        if (!string.IsNullOrEmpty(word) && dialogue.Contains(word))
        {
            return dialogue.Replace(word, $"<link=\"{word}\"><b><color=#1E90FF>{word}</color></b></link>");
        }
        return dialogue;
    }

    // ✅ Detects if the user clicks on a highlighted word and opens the definition popup
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Detect mouse click
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(textUI, Input.mousePosition, Camera.main);
            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = textUI.textInfo.linkInfo[linkIndex];
                string clickedWord = linkInfo.GetLinkID();
                Debug.Log("✅ Clicked Word: " + clickedWord);

                if (wordDefinitions.ContainsKey(clickedWord)) // ✅ Check if the word exists
                {
                    ShowDefinitionPopup(clickedWord); // ✅ Show word definition & image
                }
                else
                {
                    Debug.LogWarning("⚠️ Word definition not found: " + clickedWord);
                    definitionPanel.SetActive(false); // ✅ Hide if no definition found
                }
            }
            else
            {
                definitionPanel.SetActive(false); // ✅ Hide panel if user clicks outside
            }
        }
    }

    // ✅ Function to Show Word Definition
    void ShowDefinitionPopup(string word)
    {
        definitionPanel.SetActive(true);

        if (wordDefinitions.ContainsKey(word))
        {
definitionText.text = $"<b><color=#228B22>{word}</color></b>\n{wordDefinitions[word]}";
        }
        else
        {
            definitionText.text = $"<b>{word}</b>\nNo definition available.";
        }

        if (wordImages.ContainsKey(word) && !string.IsNullOrEmpty(wordImages[word]))
        {
            StartCoroutine(LoadImage(wordImages[word])); // ✅ Load and display the image
        }
        else
        {
            definitionImage.gameObject.SetActive(false); // ✅ Hide if no image available
        }
    }

    // ✅ Function to Close Definition Popup
    public void CloseDefinitionPopup()
    {
        definitionPanel.SetActive(false);
    }

    // ✅ Function to Load Image from URL
    IEnumerator LoadImage(string imageUrl)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return request.SendWebRequest();

if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                definitionImage.texture = texture;
                definitionImage.gameObject.SetActive(true); // ✅ Show the image
            }
            else
            {
                Debug.LogError("❌ Failed to load image: " + request.error);
                definitionImage.gameObject.SetActive(false); // ✅ Hide if failed
            }
        }
    }
    void RestartAutoAdvanceCoroutine()
    {
        if (autoAdvanceCoroutine != null)
        {
            Debug.Log("⏹️ Stopping previous auto-advance...");
            StopCoroutine(autoAdvanceCoroutine);
        }

        Debug.Log("▶️ Starting new auto-advance...");
        autoAdvanceCoroutine = StartCoroutine(AutoAdvanceDialogue());
    }
IEnumerator LoadPhoto(string imageUrl)
{
    Debug.Log("🖼️ Attempting to load visual photo: " + imageUrl);

    using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
    {
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            photoUI.texture = texture;
            photoUI.gameObject.SetActive(true);

            if (photoPanel != null)
            {
                photoPanel.SetActive(true);
            }

            Debug.Log("✅ Visual photo loaded and displayed.");
        }
        else
        {
            Debug.LogError("❌ Failed to load visual photo: " + request.error);
            photoUI.gameObject.SetActive(false);
            if (photoPanel != null) photoPanel.SetActive(false);
        }
    }
}

    IEnumerator AutoAdvanceDialogue()
    {
        Debug.Log("⏳ Waiting to auto-advance...");

        yield return new WaitForSeconds(7); // ✅ Wait 7 seconds before moving to the next dialogue

        if (currentDialogueIndex < currentDialogues.Count - 1)
        {
            Debug.Log("➡️ Auto-advancing to next dialogue...");
            NextDialogue();
        }
        else
        {
            Debug.Log("✅ Last dialogue reached, stopping auto-advance.");
        }
    }


    IEnumerator LoadAndPlayAudio(string url)
    {
        Debug.Log("🎧 Attempting to load audio...");

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            www.SendWebRequest();

            while (!www.isDone)
            {
                yield return null; // Wait for download to finish
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip != null)
                {
                    audioSource.clip = clip;
                    audioSource.volume = isMuted ? 0f : 1f;
                    audioSource.Play();
                    Debug.Log("✅ Audio playback started!");
                }
                else
                {
                    Debug.LogError("❌ AudioClip is NULL after download!");
                }
            }
            else
            {
                Debug.LogError($"❌ Failed to load audio: {www.error}");
            }
        }
    }




    void PreviousDialogue()
    {
        if (currentDialogueIndex > 0)
        {
            currentDialogueIndex--;
            StartCoroutine(FetchAndPlayDialogue(currentDialogues[currentDialogueIndex].Id));
        }
        UpdateButtons();
    }

    void NextDialogue()
    {
        if (currentDialogueIndex < currentDialogues.Count - 1)
        {
            currentDialogueIndex++;
            StartCoroutine(FetchAndPlayDialogue(currentDialogues[currentDialogueIndex].Id));
        }
        UpdateButtons();
    }

    void UpdateButtons()
    {
        previousButton.gameObject.SetActive(currentDialogueIndex > 0);
        nextButton.gameObject.SetActive(currentDialogueIndex < currentDialogues.Count - 1);
    }
    
    

   


}