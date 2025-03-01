using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ddbmanager : MonoBehaviour
{
    private FirebaseFirestore db;
    public Text textUI;
    public FrameTrigger frame2Trigger;
    public Button soundToggleButton;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;
    private AudioSource audioSource;
    private AudioClip loadedClip;
    private bool isMuted = true;

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
                yield return FetchAndPlayDialogue(currentDialogues[currentDialogueIndex]);
            }

            UpdateButtons();
        }
    }

    IEnumerator FetchAndPlayDialogue(DocumentReference dialogueRef)
    {
        while (isPlaying) yield return null;
        isPlaying = true;

        var dialogueTask = dialogueRef.GetSnapshotAsync();
        yield return new WaitUntil(() => dialogueTask.IsCompleted);

        if (!dialogueTask.IsFaulted)
        {
            DocumentSnapshot dialogueSnapshot = dialogueTask.Result;
            if (dialogueSnapshot.Exists)
            {
                string dialogueText = dialogueSnapshot.ContainsField("text") ? dialogueSnapshot.GetValue<string>("text") : "";
                string audioUrl = dialogueSnapshot.ContainsField("Audio") ? dialogueSnapshot.GetValue<string>("Audio") : "";

                if (textUI != null)
                {
                    textUI.text = dialogueText;
                }

                if (!string.IsNullOrEmpty(audioUrl))
                {
                    yield return StartCoroutine(LoadAndPlayAudio(audioUrl));
                }

                // Start the auto-advance coroutine (7-second delay)
                RestartAutoAdvanceCoroutine();
            }
        }

        isPlaying = false;
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
<<<<<<< HEAD
    public void MuteAudio()
{
    if (audioSource != null)
    {
        audioSource.volume = 0f; // Mute Firebase audio
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
        audioSource.volume = 1f; // Unmute Firebase audio
        Debug.Log("✅ Firebase Audio Unmuted!");
    }
    else
    {
        Debug.LogWarning("⚠️ AudioSource is NULL, cannot unmute audio!");
    }
}

=======

    void PreviousDialogue()
    {
        if (currentDialogueIndex > 0)
        {
            currentDialogueIndex--;
            StartCoroutine(FetchAndPlayDialogue(currentDialogues[currentDialogueIndex]));
            RestartAutoAdvanceCoroutine(); // Restart auto-advance timer
        }

        UpdateButtons();
    }

    void NextDialogue()
    {
        if (currentDialogueIndex < currentDialogues.Count - 1)
        {
            currentDialogueIndex++;
            StartCoroutine(FetchAndPlayDialogue(currentDialogues[currentDialogueIndex]));
            RestartAutoAdvanceCoroutine(); // Restart auto-advance timer
        }

        UpdateButtons();
    }

    void RestartAutoAdvanceCoroutine()
    {
        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
        }

        autoAdvanceCoroutine = StartCoroutine(AutoAdvanceDialogue());
    }

    IEnumerator AutoAdvanceDialogue()
    {
        yield return new WaitForSeconds(7); // Wait 7 seconds before auto-advancing

        if (currentDialogueIndex < currentDialogues.Count - 1)
        {
            NextDialogue(); // Move to the next dialogue automatically
        }
        else
        {
            Debug.Log("Last dialogue reached, transitioning to next frame in 10 seconds.");
            yield return new WaitForSeconds(10);

            if (frame2Trigger != null)
            {
                frame2Trigger.TriggerNextFrame();
            }
            else
            {
                Debug.LogError("frame2Trigger is not assigned in Ddbmanager!");
            }
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
>>>>>>> b5808492b89754724ac52940de1cd4c1da3de71a
}
