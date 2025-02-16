using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ddbmanager1 : MonoBehaviour
{
    private FirebaseFirestore db;
    public Text textUI; // Reference to the Text UI component
    public FrameTrigger frame2Trigger; // Reference to the FrameTrigger script for frame2

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        StartCoroutine(FetchFramesFromStory("story1")); // Pass the story ID
    }

    IEnumerator FetchFramesFromStory(string storyId)
    {
        DocumentReference storyRef = db.Collection("stories").Document(storyId);
        var storyTask = storyRef.GetSnapshotAsync();

        yield return new WaitUntil(() => storyTask.IsCompleted);

        if (storyTask.IsFaulted)
        {
            Debug.LogError($"Failed to fetch story {storyId}: {storyTask.Exception}");
            yield break;
        }

        DocumentSnapshot storySnapshot = storyTask.Result;
        if (storySnapshot.Exists && storySnapshot.TryGetValue("dialogeList", out List<DocumentReference> frameList))
        {
            Debug.Log($"Fetched frames list for story {storyId}.");

            for (int i = 0; i < frameList.Count; i++)
            {
                DocumentReference frameRef = frameList[i];

                if (i == 1) // Assuming frame2 is at index 1
                {
                    Debug.Log("Waiting for trigger to activate frame2...");
                    yield return new WaitUntil(() => frame2Trigger.isTriggered); // Wait for the trigger
                }

                yield return FetchDialoguesFromFrame(frameRef); // Fetch dialogues for the frame
            }
        }
        else
        {
            Debug.LogError($"Story document '{storyId}' does not exist or missing 'dialogueList' field!");
        }
    }

    IEnumerator FetchDialoguesFromFrame(DocumentReference frameRef)
    {
        var frameTask = frameRef.GetSnapshotAsync();

        yield return new WaitUntil(() => frameTask.IsCompleted);

        if (frameTask.IsFaulted)
        {
            Debug.LogError($"Failed to fetch frame {frameRef.Path}: {frameTask.Exception}");
            yield break;
        }

        DocumentSnapshot frameSnapshot = frameTask.Result;
        if (frameSnapshot.Exists && frameSnapshot.TryGetValue("listofDialoges", out List<DocumentReference> dialogueList))
        {
            Debug.Log($"Fetched dialogue list for frame {frameRef.Path}.");

            foreach (DocumentReference dialogueRef in dialogueList)
            {
                yield return FetchDialogue(dialogueRef); // Fetch each dialogue in the frame
                yield return new WaitForSeconds(7); // Wait 7 seconds before fetching the next dialogue
            }
        }
        else
        {
            Debug.LogError($"Frame document '{frameRef.Path}' does not exist or missing 'listofDialogues' field!");
        }
    }

    IEnumerator FetchDialogue(DocumentReference dialogueRef)
    {
        var dialogueTask = dialogueRef.GetSnapshotAsync();

        yield return new WaitUntil(() => dialogueTask.IsCompleted);
        if (dialogueTask.IsFaulted)
        {
            Debug.LogError($"Failed to fetch dialogue from {dialogueRef.Path}: {dialogueTask.Exception}");
        }
        else
        {
            DocumentSnapshot dialogueSnapshot = dialogueTask.Result;
            if (dialogueSnapshot.Exists)
            {
                // Fetch text field
                string text = "";
                if (dialogueSnapshot.TryGetValue("text", out text))
                {
                    Debug.Log($"Text field value ({dialogueRef.Path}): {text}");
                    if (textUI != null)
                    {
                        textUI.text = text;
                    }
                    else
                    {
                        Debug.LogWarning("Text UI component is not assigned.");
                    }
                }
                else
                {
                    Debug.LogError($"Dialogue document '{dialogueRef.Path}' does not contain 'text' field!");
                }

                // Fetch audio field
                string audioUrl = "";
                if (dialogueSnapshot.TryGetValue("Audio", out audioUrl))
                {
                    Debug.Log($"Audio field value ({dialogueRef.Path}): {audioUrl}");
                    StartCoroutine(PlayAudio(audioUrl)); // Play the audio
                }
                else
                {
                    Debug.LogError($"Dialogue document '{dialogueRef.Path}' does not contain 'Audio' field!");
                }
            }
            else
            {
                Debug.LogError($"Dialogue document '{dialogueRef.Path}' does not exist!");
            }
        }
    }
    IEnumerator PlayAudio(string url)
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
                AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }


}