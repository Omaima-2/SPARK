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
    public  TextMeshProUGUI errorTextSignUp ,errorTextLogin; // UI TextMesh Pro element for displaying error messages

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
            DisplayError("Oops! Fill in both fields.", true);
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
            DisplayError("Oops! All fields are required.😊");
            return;
        }
        if (!IsValidEmail(signupEmail.text)) // 🔥 Validate Email Format
        {
            DisplayError("Hmm..That doesn't look like a valid email.try again!✨");
            return;
        }

        if (signupPassword.text != signupCPassword.text)
        {
            DisplayError("Oops! Your passwords don't match. Try again!");
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
catch (FirebaseException firebaseEx)
{
    Debug.LogError("🔥 Firebase Auth Error: " + firebaseEx.Message);

    AuthError errorCode = (AuthError)firebaseEx.ErrorCode; // Convert to Firebase AuthError

    switch (errorCode)
    {
        case AuthError.EmailAlreadyInUse:
            DisplayError("Oops! This email is already taken. Try another one");
            break;
        case AuthError.InvalidEmail:
            DisplayError("Hmm... That doesn't look like a valid email.try again!✨");
            break;
        case AuthError.WeakPassword:
            DisplayError("Your password needs a little more strength!");
            break;
        default:
            DisplayError("Something went wrong, but don't worry! Try again in a moment. 🌟");
            break;
    }
}
catch (Exception e)
{
    Debug.LogError("🔥 Unexpected Error: " + e.Message);
    DisplayError("Uh-oh! Something went wrong. Give it another shot! 🚀");
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
            Debug.LogFormat("✅ User signed in successfully: {0} ({1})", user.Email, user.UserId);
            HideError(true);
            ShowHomePanel();
        }
    }
    catch (FirebaseException firebaseEx)
    {
        Debug.LogError($"🔥 Firebase Auth Error ({firebaseEx.ErrorCode}): {firebaseEx.Message}");

        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

        switch (errorCode)
        {
            case AuthError.WrongPassword:
                DisplayError("Oops! Incorrect password. Try again. 🔑", true);
                break;
            case AuthError.UserNotFound:
                Debug.LogError("🔥 ERROR: User not found! This should display in UI.");
                DisplayError("Oh no! We couldn't find that account. Try signing up first! 📩", true);
                break;
            case AuthError.InvalidEmail:
                DisplayError("That doesn’t look like a valid email. Try again! ✨", true);
                break;
            case AuthError.UserDisabled:
                DisplayError("This account has been disabled. Please contact support.", true);
                break;
            default:
                Debug.LogError($"🔥 Unknown Firebase Error: {errorCode} - {firebaseEx.Message}");
                DisplayError("Something went wrong, try again later. 🌟", true);
                break;
        }
    }
    catch (Exception e)
    {
        Debug.LogError($"🔥 Unexpected error: {e.Message}");
        DisplayError("An unexpected error occurred. Please try again. 🚀", true);
    }
}

void HideError(bool isLogin)
{
    if (isLogin && errorTextLogin != null)
    {
        errorTextLogin.text = ""; // Clear the error message
        errorTextLogin.gameObject.SetActive(false); // Hide the text
    }
    else if (!isLogin && errorTextSignUp != null)
    {
        errorTextSignUp.text = "";
        errorTextSignUp.gameObject.SetActive(false);
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

    void DisplayError(string message, bool isLogin = false)
{
    Debug.LogError("Displaying error: " + message);

    if (isLogin)
    {
        if (errorTextLogin != null)
        {
            errorTextLogin.gameObject.SetActive(false); // Force refresh
            errorTextLogin.text = message;
            errorTextLogin.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("⚠ errorTextLogin is NULL! Assign it in the Inspector.");
        }
    }
    else
    {
        if (errorTextSignUp != null)
        {
            errorTextSignUp.gameObject.SetActive(false);
            errorTextSignUp.text = message;
            errorTextSignUp.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("⚠ errorTextSignUp is NULL! Assign it in the Inspector.");
        }
    }
}


   
    void ShowHomePanel()
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