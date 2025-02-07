using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Ddbmanager1 : MonoBehaviour
{
    private FirebaseFirestore db;
    public Text textUI; // Reference to the Text UI component

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        StartCoroutine(FetchDialoguesSequentially());
    }

    IEnumerator FetchDialoguesSequentially()
    {
        yield return FetchDialogue("D1"); // Fetch first dialogue
        yield return new WaitForSeconds(120); // Wait 7 seconds
        yield return FetchDialogue("D2"); // Fetch second dialogue
    }

    IEnumerator FetchDialogue(string dialogueId)
    {
        DocumentReference docRef = db.Collection("Dialogues").Document(dialogueId);
        var task = docRef.GetSnapshotAsync();

        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted)
        {
            Debug.LogError($"Failed to fetch document {dialogueId}: {task.Exception}");
        }
        else
        {
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists && snapshot.TryGetValue("text", out string text))
            {
                Debug.Log($"Text field value ({dialogueId}): {text}");
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
                Debug.Log($"Document '{dialogueId}' does not exist or missing 'text' field!");
            }
        }
    }
}


