using System;
using UnityEngine;
using TMPro; // For TextMeshPro
using Firebase;
using Firebase.Database;
public class Dialog : MonoBehaviour
{
    public TextMeshProUGUI textBox; // Reference to the white box text

    private DatabaseReference dbReference;

    void Start()
    {
        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            if (task.Result == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {task.Result}");
            }
        });
    }

    void InitializeFirebase()
    {
        // Get a reference to the database
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        // Fetch the text data
        FetchTextData();
    }

    void FetchTextData()
    {
        dbReference.Child("textKey").GetValueAsync().ContinueWith(task => {
            if (task.IsCompleted && task.Result != null)
            {
                DataSnapshot snapshot = task.Result;
                string fetchedText = snapshot.Value.ToString();
                UpdateTextBox(fetchedText);
            }
            else
            {
                Debug.LogError("Failed to fetch data from Firebase");
            }
        });
    }

    void UpdateTextBox(string newText)
    {
        if (textBox != null)
        {
            textBox.text = newText;
        }
    }
}