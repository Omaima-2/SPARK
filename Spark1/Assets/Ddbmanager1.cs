using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Make sure to include this for TextMeshPro support
using UnityEngine.Networking; // ✅ Required for UnityWebRequest


public class Ddbmanager : MonoBehaviour
{
    private FirebaseFirestore db;
    public TMP_Text textUI;
    public FrameTrigger frame2Trigger;
    public Button soundToggleButton;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;
    private AudioSource audioSource;
    private AudioClip loadedClip;
    private bool isMuted = true;
private Dictionary<string, string> wordImages = new Dictionary<string, string>(); // ✅ Store word-to-image mappings
private Dictionary<string, string> wordDefinitions = new Dictionary<string, string>(); // ✅ Store word-to-definition mappings

    public Button previousButton;
    public Button nextButton;

    private List<DocumentReference> currentDialogues = new List<DocumentReference>();
    private int currentDialogueIndex = 0;
    private bool isPlaying = false;

    private Coroutine autoAdvanceCoroutine; // Coroutine for auto-advancing dialogues

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.volume = 0f;

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

                if (i == 1)
                {
                    yield return new WaitUntil(() => frame2Trigger.isTriggered);
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
            Debug.LogError($"Failed to load frame {frameRef.Path}: {frameTask.Exception}");
            yield break;
        }

        DocumentSnapshot frameSnapshot = frameTask.Result;
        if (frameSnapshot.Exists && frameSnapshot.TryGetValue("listofDialoges", out List<DocumentReference> dialogueList))
        {
            currentDialogues = dialogueList;
            currentDialogueIndex = 0;

            if (currentDialogues.Count > 0)
            {
yield return FetchAndPlayDialogue(currentDialogues[currentDialogueIndex].Id); // ✅ Pass document ID as string
            }

            UpdateButtons();
        }
    }

  IEnumerator FetchAndPlayDialogue(string dialogueId)
{
    while (isPlaying) yield return null; // ✅ Prevents multiple fetch calls
    isPlaying = true;

    Debug.Log($"📜 Fetching dialogue: {dialogueId}");

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

            if (textUI != null)
            {
                textUI.text = HighlightWord(dialogueText, highlightedWord);
            }

            // ✅ Store word, meaning, and image
            if (!string.IsNullOrEmpty(highlightedWord))
            {
                wordDefinitions[highlightedWord] = wordMeaning;
                wordImages[highlightedWord] = imageUrl;
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


string HighlightWord(string dialogue, string word)
{
    if (!string.IsNullOrEmpty(word) && dialogue.Contains(word))
    {
return dialogue.Replace(word, $"<link=\"{word}\"><b><color=#90EE90>{word}</color></b></link>");

    }
    return dialogue;
}

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
            }
        }
    }
}


public TMP_Text definitionText; // ✅ Displays the word's definition
public GameObject definitionPanel; // ✅ The panel to show the definition
public RawImage definitionImage; // ✅ Displays the image for the word

void ShowDefinitionPopup(string word)
{
    definitionPanel.SetActive(true);

    if (wordDefinitions.ContainsKey(word))
    {
        definitionText.text = $"<b>{word}</b>\n{wordDefinitions[word]}"; // ✅ Show definition
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
public void CloseDefinitionPopup()
{
    definitionPanel.SetActive(false);
}


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


    IEnumerator LoadAndPlayAudio(string url)
    {
        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.ConnectionError ||
                www.result == UnityEngine.Networking.UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error loading audio: " + www.error);
            }
            else
            {
                loadedClip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = loadedClip;
                audioSource.volume = isMuted ? 0f : 1f;
                audioSource.Play();
            }
        }
    }

    void PreviousDialogue()
    {
        if (currentDialogueIndex > 0)
        {
            currentDialogueIndex--;
            StartCoroutine(FetchAndPlayDialogue(currentDialogues[currentDialogueIndex].Id));
            RestartAutoAdvanceCoroutine(); // Restart auto-advance timer
        }

        UpdateButtons();
    }

void NextDialogue()
{
    if (currentDialogueIndex < currentDialogues.Count - 1) // ✅ Ensure it is not the last dialogue
    {
        currentDialogueIndex++; // ✅ Move to next dialogue
        Debug.Log($"➡️ Moving to dialogue {currentDialogueIndex}...");

        StopAllCoroutines(); // ✅ Stop any ongoing dialogue fetch
        StartCoroutine(FetchAndPlayDialogue(currentDialogues[currentDialogueIndex].Id)); // ✅ Fetch next dialogue
    }
    else
    {
        Debug.Log("✅ Last dialogue reached!");
    }

    UpdateButtons();
}






IEnumerator AutoAdvanceDialogue()
{
    Debug.Log("⏳ Waiting to auto-advance...");

    yield return new WaitForSeconds(7); // ✅ Wait 7 seconds

    if (currentDialogueIndex < currentDialogues.Count - 1)
    {
        Debug.Log("➡️ Auto-advancing to next dialogue...");
        NextDialogue(); // ✅ Move to next dialogue automatically
    }
    else
    {
        Debug.Log("✅ Last dialogue reached, stopping auto-advance.");
    }
}


    void UpdateButtons()
{
    if (previousButton != null)
    {
        previousButton.gameObject.SetActive(currentDialogueIndex > 0);
    }

    if (nextButton != null)
    {
        nextButton.gameObject.SetActive(currentDialogueIndex < currentDialogues.Count - 1);
    }
}

    public void MuteAudio()
{
    if (audioSource != null)
    {
        isMuted = true; // Set muted state
        audioSource.volume = 0f; // Mute the audio
        UpdateButtonSprite();
        Debug.Log("✅ Firebase Audio Muted!");
    }
    else
    {
        Debug.LogWarning("⚠️ AudioSource is NULL, cannot mute audio!");
    }
}

public void UnmuteAudio()
{
    if (audioSource != null)
    {
        isMuted = false; // Set unmuted state
        audioSource.volume = 1f; // Unmute the audio
        UpdateButtonSprite();
        Debug.Log("✅ Firebase Audio Unmuted!");
    }
    else
    {
        Debug.LogWarning("⚠️ AudioSource is NULL, cannot unmute audio!");
    }
}


}