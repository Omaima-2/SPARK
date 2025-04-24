using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections;

public class StoryPhotoLoader : MonoBehaviour
{
    private FirebaseFirestore db;
    private bool firebaseReady = false;

    [Header("Firestore Settings")]
    public string storyId = "story1";

    [Header("UI Settings")]
    public RawImage photoUI;

    void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("✅ Firebase initialized");
                db = FirebaseFirestore.DefaultInstance;
                firebaseReady = true;
            }
            else
            {
                Debug.LogError("❌ Firebase initialization failed: " + task.Result);
            }
        });
    }

    void OnEnable()
    {
        if (firebaseReady)
        {
            Debug.Log("📦 Panel activated – starting image fetch...");
            StartCoroutine(LoadStoryPhoto(storyId));
        }
        else
        {
            Debug.LogWarning("🕐 Firebase not ready yet – will fetch once ready.");
            StartCoroutine(WaitForFirebaseThenFetch());
        }
    }

    IEnumerator WaitForFirebaseThenFetch()
    {
        yield return new WaitUntil(() => firebaseReady);
        StartCoroutine(LoadStoryPhoto(storyId));
    }

    IEnumerator LoadStoryPhoto(string storyId)
    {
        DocumentReference storyRef = db.Collection("stories").Document(storyId);
        var task = storyRef.GetSnapshotAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError("❌ Firestore fetch failed: " + task.Exception);
            yield break;
        }

        var storySnap = task.Result;
        if (!storySnap.Exists || !storySnap.ContainsField("photo"))
        {
            Debug.LogWarning("⚠️ Photo field missing or document not found.");
            yield break;
        }

        string photoUrl = storySnap.GetValue<string>("photo");
        Debug.Log("🌐 Photo URL: " + photoUrl);

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(photoUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Failed to load image: " + request.error);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            photoUI.texture = texture;
            Debug.Log("✅ Image loaded successfully.");
        }
    }
}
