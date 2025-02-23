using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class  AreaAydioManger: MonoBehaviour
{
    private FirebaseFirestore db;
    public Text textUI; // ???? ??????? ???????
    public FrameTrigger frame2Trigger; // ??????? ?????? frame2
    public Button muteButton; // ?? ????? ????? / ?????
    private AudioSource audioSource; // ???? ?????
    private AudioClip loadedClip; // ????? ?????? ??? ??????
    private bool isMuted = true; // ????????? ????? ????? ??? ??? ????? ??? ????

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        audioSource = gameObject.AddComponent<AudioSource>(); // ????? AudioSource ??? ??????

        if (muteButton != null)
        {
            muteButton.onClick.AddListener(ToggleAudio);
        }
        else
        {
            Debug.LogError("?? Mute ?? ??? ???? ?? ??? Inspector!");
        }

        StartCoroutine(FetchFramesFromStory("story1")); // ????? ?????
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

                if (i == 1) // ??? ??? Frame2 ?? ???????
                {
                    Debug.Log("Waiting for trigger to activate frame2...");
                    yield return new WaitUntil(() => frame2Trigger.isTriggered); // ???????? ??? ??? ???????
                }

                yield return FetchDialoguesFromFrame(frameRef); // ????? ???????? ???? ??????
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
                yield return FetchDialogue(dialogueRef); // ????? ?? ???? ?? ??????
                yield return new WaitForSeconds(7); // ???????? ??? ????????
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
                // ????? ????
                string text = "";
                if (dialogueSnapshot.
                    TryGetValue("text", out text))
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

                // ????? ????? ???? ?? ??? ?????? ?????
                string audioUrl = "";
                if (dialogueSnapshot.TryGetValue("Audio", out audioUrl))
                {
                    Debug.Log($"Audio field value ({dialogueRef.Path}): {audioUrl}");
                    StartCoroutine(LoadAudio(audioUrl)); // ????? ????? ???
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

    IEnumerator LoadAudio(string url)
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
                Debug.Log("Audio loaded but not playing yet. Press the button to play.");
            }
        }
    }

    void ToggleAudio()
    {
        if (loadedClip == null)
        {
            Debug.LogWarning("?? ???? ??? ???? ???!");
            return;
        }

        isMuted = !isMuted;  // ??? ???? ?????

        if (!isMuted)
        {
            audioSource.Play();
            Debug.Log("?? ????? ?????.");
        }
        else
        {
            audioSource.Pause();
            Debug.Log("?? ????? ?????.");
        }
    }
}
