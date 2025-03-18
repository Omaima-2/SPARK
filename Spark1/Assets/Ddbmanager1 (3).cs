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
    public FrameTrigger frame2Trigger;
    public Button soundToggleButton;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;
    private AudioSource audioSource;
    private bool isMuted = true;
    private Dictionary<string, string> wordImages = new Dictionary<string, string>();
    private Dictionary<string, string> wordDefinitions = new Dictionary<string, string>();

    public Button previousButton;
    public Button nextButton;

    private List<DocumentReference> currentDialogues = new List<DocumentReference>();
    private int currentDialogueIndex = 0;
    private bool isPlaying = false;

    private Coroutine autoAdvanceCoroutine;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
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

                if (!string.IsNullOrEmpty(highlightedWord))
                {
                    wordDefinitions[highlightedWord] = wordMeaning;
                    wordImages[highlightedWord] = imageUrl;
                }

                float waitTime = Mathf.Clamp(dialogueText.Length * 0.1f, 3f, 10f); // Adjust wait time dynamically
                yield return new WaitForSeconds(waitTime);

                NextDialogue();
            }
        }
        isPlaying = false;
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
        if (previousButton != null)
        {
            previousButton.gameObject.SetActive(currentDialogueIndex > 0);
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(currentDialogueIndex < currentDialogues.Count - 1);
        }
    }

    string HighlightWord(string dialogue, string word)
    {
        if (!string.IsNullOrEmpty(word) && dialogue.Contains(word))
        {
            return dialogue.Replace(word, $"<link=\"{word}\"><b><color=#90EE90>{word}</color></b></link>");
        }
        return dialogue;
    }

    public TMP_Text definitionText;
    public GameObject definitionPanel;
    public RawImage definitionImage;

    void ShowDefinitionPopup(string word)
    {
        definitionPanel.SetActive(true);
        if (wordDefinitions.ContainsKey(word))
        {
            definitionText.text = $"<b>{word}</b>\n{wordDefinitions[word]}";
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
                definitionImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("❌ Failed to load image: " + request.error);
                definitionImage.gameObject.SetActive(false);
            }
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
