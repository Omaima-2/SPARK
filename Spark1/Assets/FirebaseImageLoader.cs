
using Firebase;
using Firebase.Extensions;  // 🔹 Required for ContinueWithOnMainThread
using Firebase.Storage;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks; // 🔹 Required for Task<T>
using UnityEngine.Networking;
using Firebase.Firestore;  // ✅ Required for Firestore
using TMPro;  // ✅ Required for TMP text



public class FirebaseImageLoader : MonoBehaviour
{
    private FirebaseStorage storage;
    private StorageReference storageReference;
 private FirebaseFirestore db;
    public TMP_Text displayText; // UI Text to show retrieved data

    public string imagePath = "images/ladybug_metamorphosis.png"; // Path in Firebase Storage
    public RawImage displayImage; // UI element to display image

  void Start()
{
    FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
    {
        if (task.Result == DependencyStatus.Available)
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            storage = FirebaseStorage.DefaultInstance;
            storageReference = storage.GetReferenceFromUrl("gs://spark-d3746.firebasestorage.app/");
            db = FirebaseFirestore.DefaultInstance;

            // Now call methods after Firebase is ready
            RetrieveImage();
            ListDocuments();
            RetrieveText();
        }
        else
        {
            Debug.LogError("❌ Firebase Dependencies Not Available: " + task.Result);
        }
    });
}


    public void RetrieveImage()
    {
        StorageReference imageRef = storageReference.Child(imagePath);

        // Get Download URL
        imageRef.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                string url = task.Result.ToString();
                Debug.Log("Download URL: " + url);
                StartCoroutine(LoadImage(url));
            }
            else
            {
                Debug.LogError("Failed to get image URL: " + task.Exception);
            }
        });
    }

    private IEnumerator LoadImage(string imageUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            displayImage.texture = texture;
        }
        else
        {
            Debug.LogError("Error loading image: " + request.error);
        }
    }
    public void RetrieveText()
{
    DocumentReference docRef = db.Collection("wordDefinitions").Document("W1");

    docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted && !task.IsFaulted)
        {
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                Debug.Log("✅ Firestore Document Exists!");

                var allData = snapshot.ToDictionary();
                foreach (var pair in allData)
                {
                    Debug.Log($"🔹 {pair.Key}: {pair.Value}");
                }

                if (snapshot.ContainsField("meaning"))
                {
                    string meaning = snapshot.GetValue<string>("meaning");
                    Debug.Log("🎉 Retrieved Meaning: " + meaning);
                    displayText.text = meaning;  // Assign text
                }
                else
                {
                    Debug.LogWarning("⚠️ Field 'meaning' does not exist!");
                }
            }
            else
            {
                Debug.LogWarning("❌ Document does not exist!");
            }
        }
        else
        {
            Debug.LogError("🔥 Error retrieving document: " + task.Exception);
        }
    });
    


}
public void ListDocuments()
{
    db.Collection("wordDefinitions").GetSnapshotAsync().ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted && !task.IsFaulted)
        {
            QuerySnapshot snapshot = task.Result;
            Debug.Log($"📂 Found {snapshot.Count} documents in 'wordDefinitions'.");

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                Debug.Log($"📜 Document ID: {doc.Id}");
            }
        }
        else
        {
            Debug.LogError("🔥 Error retrieving documents: " + task.Exception);
        }
    });
}

}
