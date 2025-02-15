using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;

public class FirebaseController : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseUser user;
    private FirebaseFirestore db;

    public GameObject loginPanel, signupPanel, homePanel;
    public InputField loginEmail, loginPassword, signupEmail, signupPassword, signupCPassword, signupName;
    public TextMeshProUGUI errorText; // UI TextMesh Pro element for displaying error messages

    async void Start()
    {
        await InitializeFirebase();
    }

    async Task InitializeFirebase()
    {
        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
        await dependencyTask; // Wait until Firebase dependencies are checked

        if (dependencyTask.Result == DependencyStatus.Available)
        {
            auth = FirebaseAuth.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance;
            auth.StateChanged += AuthStateChanged;
            AuthStateChanged(this, null);
            Debug.Log("✅ Firebase initialized successfully.");
        }
        else
        {
            Debug.LogError("❌ Firebase initialization failed: " + dependencyTask.Result);
            DisplayError("Firebase failed to initialize.");
        }
    }

    public void OpenLogin()
    {
        loginPanel.SetActive(true);
        signupPanel.SetActive(false);
    }

    public void OpenSignUp()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(true);
    }

    public void LoginUser()
    {
        if (string.IsNullOrEmpty(loginEmail.text) || string.IsNullOrEmpty(loginPassword.text))
        {
            DisplayError("Login fields cannot be empty.");
            return;
        }

        SignInUser(loginEmail.text, loginPassword.text);
    }

    public void SignupUser()
    {
        if (auth == null)
        {
            DisplayError("Firebase is not initialized. Please wait...");
            return;
        }

        if (string.IsNullOrEmpty(signupName.text) ||
            string.IsNullOrEmpty(signupEmail.text) ||
            string.IsNullOrEmpty(signupPassword.text) ||
            string.IsNullOrEmpty(signupCPassword.text))
        {
            DisplayError("Signup fields cannot be empty.");
            return;
        }
         if (!IsValidEmail(signupEmail.text)) // 🔥 Validate Email Format
    {
        DisplayError("Invalid email format. Please enter a valid email.");
        return;
    }

        if (signupPassword.text != signupCPassword.text)
        {
            DisplayError("Passwords do not match.");
            return;
        }

        CreateUser(signupEmail.text, signupPassword.text, signupName.text);
    }

    async void CreateUser(string email, string password, string name)
    {
        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            user = result.User;

            if (user != null)
            {
                Debug.LogFormat("✅ User created successfully: {0} ({1})", user.Email, user.UserId);

                // Update Firebase Authentication Profile
                UserProfile profile = new UserProfile { DisplayName = name };
                await user.UpdateUserProfileAsync(profile);
                Debug.Log("✅ User name updated in Firebase Authentication.");

                // Store user data in Firestore
                DocumentReference userRef = db.Collection("users").Document(user.UserId);
                await userRef.SetAsync(new Dictionary<string, object>
                {
                    { "name", name },
                    { "email", email }
                });

                Debug.Log("✅ User information stored in Firestore.");
                  ShowHomePanel();
            }
        }
        catch (Exception e)
        {
            DisplayError("Error creating user: " + e.Message);
        }
    }

    async void SignInUser(string email, string password)
{
    try
    {
        var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
        user = result.User;

        if (user != null)
        {
            string userName = user.DisplayName; // Retrieve name from Firebase Authentication
            Debug.LogFormat("✅ User signed in successfully: {0} ({1}) - Name: {2}", user.Email, user.UserId, userName);
              ShowHomePanel();
        }
    }
    catch (FirebaseException firebaseEx)
    {
        Debug.LogError("🔥 Firebase Auth Error: " + firebaseEx.Message);
        DisplayError("Authentication failed: " + firebaseEx.Message);
    }
    catch (Exception e)
    {
        Debug.LogError("Error signing in: " + e.Message);
        DisplayError("An unexpected error occurred. Please try again.");
    }
}


    void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("🔴 Signed out: " + user.UserId);
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("🟢 Signed in: " + user.UserId);
            }
        }
    }

    void OnDestroy()
    {
        if (auth != null)
        {
            auth.StateChanged -= AuthStateChanged;
        }
    }

    void DisplayError(string message)
    {
        Debug.LogError(message);
        if (errorText != null)
        {
            errorText.text = message; // Display error in UI
        }
    }void ShowHomePanel()
{
    loginPanel.SetActive(false);
    signupPanel.SetActive(false);
    homePanel.SetActive(true);

   
}
private bool IsValidEmail(string email)
{
    try
    {
        var addr = new System.Net.Mail.MailAddress(email);
        return addr.Address == email;
    }
    catch
    {
        return false;
    }
}


}
