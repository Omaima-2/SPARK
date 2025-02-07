using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ddbmanager1 : MonoBehaviour
{
    private FirebaseFirestore db;

    // Reference to the Text UI component
    public Text textUI;

    void Start()
    {
        // Initialize Firestore
        db = FirebaseFirestore.DefaultInstance;

        // Reference the document "d1" in the "Dialogues" collection
        DocumentReference docRef = db.Collection("d").Document("d2");

        // Fetch the document
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    // Access the "text" field in the document
                    if (snapshot.TryGetValue<string>("text", out string text))
                    {
                        Debug.Log("Text field value: " + text);

                        // Update the Text UI component
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
                        Debug.Log("The 'text' field is missing in the document.");
                    }
                }
                else
                {
                    Debug.Log("Document 'd1' does not exist!");
                }
            }
            else
            {
                Debug.LogError("Failed to fetch document: " + task.Exception);
            }
        });
    }
}

