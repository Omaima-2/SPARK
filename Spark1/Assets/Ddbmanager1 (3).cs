
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
    public FrameTrigger frame4Trigger; public Animator animator;
    public Button soundToggleButton;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;
    public AudioSource audioSource;
    public bool isMuted = true;
    private Dictionary<string, string> wordImages = new Dictionary<string, string>();
    private Dictionary<string, string> wordDefinitions = new Dictionary<string, string>();
    public GameObject definitionPanel;

    public GameObject definitionActionButton;  // Assign in the Inspector (e.g., Got It button)

    public GameObject ExitPanel; // Assign your Panel in the inspector
    public Button previousButton;
    public Button nextButton;
    public RawImage photoUI;         // Drag your RawImage here in the Inspector
    public GameObject photoPanel;    // Optional: panel wrapping the RawImage

    private List<DocumentReference> currentDialogues = new List<DocumentReference>();
    private int currentDialogueIndex = 0;
    private bool isPlaying = false;

    private Coroutine autoAdvanceCoroutine;

    private Coroutine playingCoroutine; // ✅ Added coroutine to handle playback interruptions
    public TMP_Text definitionText;

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
        ExitPanel.SetActive(false);
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
                            frameRef = frameList[3]; // Frame 4
                            yield return FetchDialoguesFromFrame(frameRef);

                            // ✅ ننتظر دخول حالة afterActivity
                            Debug.Log("⏳ Waiting for Animator to enter 'afterActivity' state...");
                            yield return new WaitUntil(() =>
                            {
                                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // 0 = base layer
                                return stateInfo.IsName("afterActivity");
                            });

                            // ✅ بعد دخول الحالة نعرض Frame 5
                            Debug.Log("✅ Animator is in 'afterActivity' state. Fetching Frame 5 dialogues...");
                            yield return FetchDialoguesFromFrame(frameList[4]);

                            yield break; // 🔥 نوقف بعد Frame 5
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
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine); // ✅ Stop previous coroutine to allow immediate dialogue switch
        }
        playingCoroutine = StartCoroutine(PlayDialogueCoroutine(dialogueId));
        yield return playingCoroutine;
    }

    IEnumerator PlayDialogueCoroutine(string dialogueId)
    {
        Debug.Log($"🎯 FetchAndPlayDialogue() called for ID: {dialogueId}");
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
                string photoUrl = dialogueSnapshot.ContainsField("visual") ? dialogueSnapshot.GetValue<string>("visual") : "";

                if (!string.IsNullOrEmpty(photoUrl))
                    StartCoroutine(LoadPhoto(photoUrl));
                else if (photoPanel != null) photoPanel.SetActive(false);

                if (textUI != null)
                    textUI.text = HighlightWord(dialogueText, highlightedWord);

                if (!string.IsNullOrEmpty(highlightedWord))
                {
                    if (!wordDefinitions.ContainsKey(highlightedWord))
                        wordDefinitions[highlightedWord] = wordMeaning;
                    if (!wordImages.ContainsKey(highlightedWord))
                        wordImages[highlightedWord] = imageUrl;
                }

                float clipLength = 7f; // ✅ Default fallback time if no audio is found

                if (!string.IsNullOrEmpty(audioUrl))
                {
                    yield return StartCoroutine(LoadAndPlayAudio(audioUrl));
                    if (audioSource.clip != null)
                        clipLength = audioSource.clip.length;
                }

                if (autoAdvanceCoroutine != null)
                    StopCoroutine(autoAdvanceCoroutine);
                autoAdvanceCoroutine = StartCoroutine(AutoAdvanceDialogue(clipLength)); // ✅ Start dynamic timer

                UpdateButtons();

            }
        }

    }


    // ✅ Updated HighlightWord() function to support clickable words
    string HighlightWord(string dialogue, string word)
    {
        if (!string.IsNullOrEmpty(word) && dialogue.Contains(word))
        {
            return dialogue.Replace(word, $"<link=\"{word}\"><b><color=#CCCC00>{word}</color></b></link>");
        }
        return dialogue;
    }

    // ✅ Detects if the user clicks on a highlighted word and opens the definition popup
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Detect mouse click
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(textUI, Input.mousePosition, null);
            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = textUI.textInfo.linkInfo[linkIndex];
                string clickedWord = linkInfo.GetLinkID();
                Debug.Log("✅ Clicked Word: " + clickedWord);

                if (wordDefinitions.ContainsKey(clickedWord)) // ✅ Check if the word exists
                {
                    ShowDefinitionPopup(clickedWord);
                }
                else
                {
                    Debug.LogWarning("⚠️ Word definition not found: " + clickedWord);
                    definitionPanel.SetActive(false); // ✅ Hide if no definition found
                }
            }
            else

            {
                definitionPanel.SetActive(false);
                definitionActionButton.SetActive(false); // 👈 Hide button if clicked outside
            }


        }
    }

    // ✅ Function to Show Word Definition
    void ShowDefinitionPopup(string word)
    {
        definitionPanel.SetActive(true);
        definitionActionButton.SetActive(true);


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
            StartCoroutine(LoadImage(wordImages[word]));
        }
        else
        {
            definitionImage.gameObject.SetActive(false);
        }
    }

    public void CloseDefinitionPopup()
    {
        Debug.Log("🧹 CloseDefinitionPopup called");

        definitionPanel.SetActive(false);


    }





    private bool isPaused = false;

    public void PauseStory()
    {
        if (isPaused) return;

        Debug.Log("⏸️ PauseStory() called");
        Time.timeScale = 0f;

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log("✅ Local audio paused");
        }

        MuteAudio(); // your existing function
        isPaused = true;
    }
    public void ResumeStory()
    {
        Debug.Log("🚀 ResumeStory() CALLED");

        if (!isPaused)
        {
            Debug.Log("🟡 Resume skipped — isPaused is false");
            return;
        }

        Time.timeScale = 1f;
        Debug.Log("✅ Time resumed: Time.timeScale = " + Time.timeScale);

        if (audioSource != null)
        {
            audioSource.UnPause();
            Debug.Log("✅ Local audio resumed");
        }

        UnmuteAudio();
        isPaused = false;
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
    /*void RestartAutoAdvanceCoroutine()
    {
        if (autoAdvanceCoroutine != null)
        {
            Debug.Log("⏹️ Stopping previous auto-advance...");
            StopCoroutine(autoAdvanceCoroutine);
        }

        Debug.Log("▶️ Starting new auto-advance...");
        autoAdvanceCoroutine = StartCoroutine(AutoAdvanceDialogue());
    }*/
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
    IEnumerator AutoAdvanceDialogue(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        if (currentDialogueIndex < currentDialogues.Count - 1)
        {
            currentDialogueIndex++;
            StartCoroutine(FetchAndPlayDialogue(currentDialogues[currentDialogueIndex].Id));
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
            StartCoroutine(FetchAndPlayDialogue(currentDialogues[currentDialogueIndex].Id)); // ✅ Immediate switch
        }
        UpdateButtons();
    }

    void NextDialogue()
    {
        if (currentDialogueIndex < currentDialogues.Count - 1)
        {
            currentDialogueIndex++;
            StartCoroutine(FetchAndPlayDialogue(currentDialogues[currentDialogueIndex].Id)); // ✅ Immediate switch
        }
        UpdateButtons();
    }

    void UpdateButtons()
    {
        previousButton.gameObject.SetActive(currentDialogueIndex > 0);
        nextButton.gameObject.SetActive(currentDialogueIndex < currentDialogues.Count - 1);
    }






}