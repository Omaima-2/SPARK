using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class sfg : MonoBehaviour
{
    private FirebaseFirestore db;
    public Text textUI; // النص المرتبط بالحوار
    public FrameTrigger frame2Trigger; // التريجر لتفعيل frame2
    public Button muteButton; // زر تشغيل الصوت / الكتم
    private AudioSource audioSource; // مصدر الصوت
    private AudioClip loadedClip; // الملف الصوتي بعد تحميله
    private bool isMuted = true; // افتراضيًا الصوت مكتوم حتى يتم الضغط على الزر

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        audioSource = gameObject.AddComponent<AudioSource>(); // إضافة AudioSource إلى العنصر

        if (muteButton != null)
        {
            muteButton.onClick.AddListener(ToggleAudio);
        }
        else
        {
            Debug.LogError("زر Mute لم يتم ربطه في الـ Inspector!");
        }

        StartCoroutine(FetchFramesFromStory("story1")); // تحميل القصة
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

                if (i == 1) // إذا كان Frame2 هو المطلوب
                {
                    Debug.Log("Waiting for trigger to activate frame2...");
                    yield return new WaitUntil(() => frame2Trigger.isTriggered); // الانتظار حتى يتم التفعيل
                }

                yield return FetchDialoguesFromFrame(frameRef); // تحميل الحوارات لهذا الإطار
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
                yield return FetchDialogue(dialogueRef); // تحميل كل حوار في الإطار
                yield return new WaitForSeconds(7); // الانتظار بين الحوارات
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
                // تحميل النص
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

                // تحميل الصوت ولكن لا يتم تشغيله فورًا
                string audioUrl = "";
                if (dialogueSnapshot.TryGetValue("Audio", out audioUrl))
                {
                    Debug.Log($"Audio field value ({dialogueRef.Path}): {audioUrl}");
                    StartCoroutine(LoadAudio(audioUrl)); // تحميل الصوت فقط
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
            Debug.LogWarning("لا يوجد صوت محمل بعد!");
            return;
        }

        isMuted = !isMuted;  // عكس حالة الصوت

        if (!isMuted)
        {
            audioSource.Play();
            Debug.Log("تم تشغيل الصوت.");
        }
        else
        {
            audioSource.Pause();
            Debug.Log("تم إيقاف الصوت.");
        }
    }
}
