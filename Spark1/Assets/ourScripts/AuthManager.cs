using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;

public class AuthManager : MonoBehaviour
{
    // UI References
    public InputField signupInputField; // For username input during sign up
    public Transform listViewContent;  // Content Transform for the List View (Scroll View)
    public GameObject listItemPrefab;  // Prefab for list items (buttons with usernames)
    public Text statusText;            // For displaying status messages

    private DatabaseReference dbReference; // Firebase Database Reference

    void Start()
    {
        // Initialize Firebase Database
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        // Load existing user names into the List View
        LoadUserNames();
    }

    /// <summary>
    /// Sign up a new user by adding their name to the Firebase Realtime Database.
    /// </summary>
    public void Signup()
    {
        string username = signupInputField.text.Trim();

        if (string.IsNullOrEmpty(username))
        {
            statusText.text = "Username cannot be empty.";
            return;
        }

        // Add the new user to Firebase
        dbReference.Child("users").Child(username).SetValueAsync(true).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Signed up successfully as: " + username);
                statusText.text = "Sign up successful!";
                // Add the new user to the List View
                AddUserToListView(username);
            }
            else
            {
                Debug.LogError("Sign up failed: " + task.Exception);
                statusText.text = "Sign up failed. Try again.";
            }
        });
    }

    /// <summary>
    /// Load existing user names from Firebase and populate the List View.
    /// </summary>
    public void LoadUserNames()
    {
        dbReference.Child("users").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to load user names: " + task.Exception);
                return;
            }

            DataSnapshot snapshot = task.Result;

            foreach (var child in snapshot.Children)
            {
                string userName = child.Key;

                // Add each user to the List View
                AddUserToListView(userName);
            }
        });
    }

    /// <summary>
    /// Add a user name to the List View as a selectable button.
    /// </summary>
    private void AddUserToListView(string userName)
    {
        GameObject listItem = Instantiate(listItemPrefab, listViewContent);
        Text buttonText = listItem.GetComponentInChildren<Text>();
        buttonText.text = userName;

        // Add a click event to the button
        listItem.GetComponent<Button>().onClick.AddListener(() => Login(userName));
    }

    /// <summary>
    /// Login with the selected username.
    /// </summary>
    public void Login(string userName)
    {
        if (string.IsNullOrEmpty(userName))
        {
            statusText.text = "Please select a user!";
            return;
        }

        dbReference.Child("users").Child(userName).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                Debug.Log("Logged in as: " + userName);
                statusText.text = "Welcome, " + userName + "!";
            }
            else
            {
                Debug.Log("Login failed: User does not exist.");
                statusText.text = "User not found.";
            }
        });
    }
}

