using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ddbmanager : MonoBehaviour
{
    private FirebaseFirestore db;
    public Text textUI; // UI text element for displaying dialogue
    public FrameTrigger frame2Trigger; // Trigger for activating Frame2
    public Button soundToggleButton; // Button that toggles sound (previously mute button)
    public Sprite soundOnSprite; // Image for sound ON
    public Sprite soundOffSprite; // Image for sound OFF
    private AudioSource audioSource; // Audio source component
    private AudioClip loadedClip; // Currently loaded audio clip
    private bool isPlaying = false; // Prevents overlapping dialogues
    private bool isMuted = true; // Starts as muted

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false; // Do not loop the audio
        audioSource.volume = 0f; // Start with audio muted (default)

        if (soundToggleButton != null)
        {
            soundToggleButton.onClick.AddListener(ToggleSound);
            UpdateButtonSprite(); // Ensure the button has the correct image on start
        }
        else
        {
            Debug.LogError("Sound Toggle Button is not assigned!");
        }

        StartCoroutine(FetchFramesFromStory("story1"));
    }

    void ToggleSound()
    {
        isMuted = !isMuted; // Toggle sound state
        audioSource.volume = isMuted ? 0f : 1f; // Apply mute/unmute
        Debug.Log($"Sound {(isMuted ? "Muted" : "Unmuted")}");
        UpdateButtonSprite();
    }

    void UpdateButtonSprite()
    {
        if (soundToggleButton != null && soundToggleButton.image != null)
        {
            soundToggleButton.image.sprite = isMuted ? soundOffSprite : soundOnSprite;
        }
        else
        {
            Debug.LogWarning("Sound toggle button or image component is missing!");
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
            Debug.Log($"Fetched frames for story {storyId}.");

            for (int i = 0; i < frameList.Count; i++)
            {
                DocumentReference frameRef = frameList[i];

                if (i == 1)
                {
                    Debug.Log("Waiting for Frame2 trigger...");
                    yield return new WaitUntil(() => frame2Trigger.isTriggered);
                }

                yield return FetchDialoguesFromFrame(frameRef);
            }
        }
        else
        {
            Debug.LogError($"Document '{storyId}' does not contain 'dialogeList'!");
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
            Debug.Log($"Fetched dialogues for frame {frameRef.Path}.");

            foreach (DocumentReference dialogueRef in dialogueList)
            {
                yield return FetchAndPlayDialogue(dialogueRef);
            }
        }
        else
        {
            Debug.LogError($"Document '{frameRef.Path}' does not contain 'listofDialogues'!");
        }
    }

    IEnumerator FetchAndPlayDialogue(DocumentReference dialogueRef)
    {
        while (isPlaying) yield return null;
        isPlaying = true;

        var dialogueTask = dialogueRef.GetSnapshotAsync();
        yield return new WaitUntil(() => dialogueTask.IsCompleted);

        if (dialogueTask.IsFaulted)
        {
            Debug.LogError($"Failed to load dialogue {dialogueRef.Path}: {dialogueTask.Exception}");
        }
        else
        {
            DocumentSnapshot dialogueSnapshot = dialogueTask.Result;
            if (dialogueSnapshot.Exists)
            {
                string dialogueText = "";
                string audioUrl = "";

                if (dialogueSnapshot.TryGetValue("text", out dialogueText))
                {
                    Debug.Log($"Fetched text ({dialogueRef.Path}): {dialogueText}");
                    if (textUI != null)
                    {
                        textUI.text = dialogueText;
                    }
                    else
                    {
                        Debug.LogWarning("Text UI element is not assigned!");
                    }
                }
                else
                {
                    Debug.LogError($"Dialogue '{dialogueRef.Path}' does not contain 'text'!");
                }

                if (dialogueSnapshot.TryGetValue("Audio", out audioUrl))
                {
                    Debug.Log($"Fetched audio URL ({dialogueRef.Path}): {audioUrl}");
                    yield return StartCoroutine(LoadAndPlayAudio(audioUrl));
                }
                else
                {
                    Debug.LogError($"Dialogue '{dialogueRef.Path}' does not contain 'Audio'!");
                }

                yield return new WaitForSeconds(7);

                audioSource.Stop();
                textUI.text = "";
            }
            else
            {
                Debug.LogError($"Dialogue '{dialogueRef.Path}' does not exist!");
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
                audioSource.volume = isMuted ? 0f : 1f; // Apply mute state
                audioSource.Play();
                Debug.Log($"Audio loaded and playing with volume {(isMuted ? "muted" : "unmuted")}.");
            }
        }
    }
}
