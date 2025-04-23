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
    // Singleton instance
    public static FirebaseController Instance { get; private set; }
    public bool IsFirebaseInitialized { get; private set; }
    
    // Firebase variables
    private FirebaseAuth auth;
    private FirebaseUser user;
    private FirebaseFirestore db;

    // UI Panels
    public GameObject loginPanel, signupPanel, homePanel, accountInfoPanel, welcmePanel;
    
    // Login/Signup UI elements
    public InputField loginEmail, loginPassword, signupEmail, signupPassword, signupCPassword, signupName;
    public TextMeshProUGUI errorTextSignUp, errorTextLogin;
    public TextMeshProUGUI usernameText;

    // Account management UI elements
    public Button logoutButton;
    public Button accountNameButton;
    public TextMeshProUGUI displayUsernameText;
    public TextMeshProUGUI displayEmailText;
    public Button deleteAccountButton;
    public InputField deleteConfirmPassword;
    public TextMeshProUGUI accountErrorText;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    async void Start()
    {
        Debug.Log("FirebaseController: Starting initialization");
        await InitializeFirebase();
    }

    async Task InitializeFirebase()
    {
        Debug.Log("FirebaseController: Beginning Firebase initialization");
        
        try
        {
            var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
            await dependencyTask; // Wait until Firebase dependencies are checked

            if (dependencyTask.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance;
                auth.StateChanged += AuthStateChanged;
                AuthStateChanged(this, null);
                IsFirebaseInitialized = true;
                Debug.Log("✅ Firebase initialized successfully.");
            }
            else
            {
                Debug.LogError("❌ Firebase initialization failed: " + dependencyTask.Result);
                DisplayError("Firebase failed to initialize.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Exception during Firebase initialization: " + e.Message);
        }
    }

    #region Login and Signup

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

        // Check if name is valid (no excessive length, etc.)
        if (!IsValidName(signupName.text))
        {
            DisplayError("Name can only contain letters, spaces, and common punctuation. Maximum 50 characters allowed!");
            return;
        }

        // Create user directly without checking username availability
        CreateUser(signupEmail.text, signupPassword.text, signupName.text);
    }

    private bool IsValidName(string name)
    {
        // Name validation - much more permissive than username validation
        // Just check for reasonable length and basic sanity
        if (string.IsNullOrEmpty(name) || name.Length > 50)
            return false;

        // You could add more validation here if needed
        return true;
    }

    async void CreateUser(string email, string password, string displayName)
    {
        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            user = result.User;

            if (user != null)
            {
                Debug.LogFormat("✅ User created successfully: {0} ({1})", user.Email, user.UserId);

                // Update Firebase Authentication Profile with the display name
                UserProfile profile = new UserProfile { DisplayName = displayName };
                await user.UpdateUserProfileAsync(profile);
                Debug.Log("✅ Display name updated in Firebase Authentication.");

                // Store user data in Firestore
                DocumentReference userRef = db.Collection("users").Document(user.UserId);
                await userRef.SetAsync(new Dictionary<string, object>
                {
                    { "displayName", displayName },
                    { "email", email },
                    { "createdAt", FieldValue.ServerTimestamp }
                });

                Debug.Log("✅ User information stored in Firestore.");
                ShowHomePanel();
            }
        }
        catch (FirebaseException firebaseEx)
        {
//            Debug.LogError("🔥 Firebase Auth Error: " + firebaseEx.Message);

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
                // Fetch user data from Firestore
                await LoadUserData(user.UserId);
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
        DisplayError("Oh no! We couldn't find that account. Try signing up first! 📩", true);
        break;
    case AuthError.InvalidEmail:
        DisplayError("That doesn't look like a valid email. Try again! ✨", true);
        break;
    case AuthError.UserDisabled:
        DisplayError("This account has been disabled. Please contact support.", true);
        break;
    case (AuthError)1: // 🔥 Firebase internal error code
        DisplayError("Oops! Incorrect Email or password. Try again. 🔑", true);
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

     void DisplayError(string message, bool isLogin = false)
    {
        //Debug.LogError("Displaying error: " + message);

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

    #endregion

    #region User Data Management

    async Task LoadUserData(string userId)
    {
        try
        {
            DocumentReference userRef = db.Collection("users").Document(userId);
            DocumentSnapshot snapshot = await userRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                Dictionary<string, object> userData = snapshot.ToDictionary();
                
                if (userData.TryGetValue("displayName", out object displayNameObj) && usernameText != null)
                {
                    string displayName = displayNameObj.ToString();
                    usernameText.text = displayName;
                    Debug.Log($"✅ Loaded user data with display name: {displayName}");
                }
                else if (user.DisplayName != null && usernameText != null)
                {
                    // Fallback to Auth DisplayName if Firestore doesn't have display name
                    usernameText.text = user.DisplayName;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Error loading user data: {e.Message}");
        }
    }

    #endregion

    #region Account Management

    // Toggle the account info panel when clicking on the account name
    public void ToggleAccountInfoPanel()
    {
        if (accountInfoPanel != null)
        {
            bool isActive = !accountInfoPanel.activeSelf;
            accountInfoPanel.SetActive(isActive);
            
            if (isActive)
            {
                // Update displayed user information
                UpdateAccountInfoDisplay();
            }
        }
    }
    
    // Update the account info display with current user data
    private void UpdateAccountInfoDisplay()
    {
        if (user != null)
        {
            if (displayUsernameText != null)
                displayUsernameText.text = user.DisplayName ?? "No name";
                
            if (displayEmailText != null)
                displayEmailText.text = user.Email ?? "No email";
                
            // Clear any previous error messages
            if (accountErrorText != null)
            {
                accountErrorText.text = "";
                accountErrorText.gameObject.SetActive(false);
            }
        }
    }

    // Logout functionality
    public void LogoutUser()
    {
        if (auth != null)
        {
            try
            {
                auth.SignOut();
                user = null;
                Debug.Log("✅ User signed out successfully");
                
                // Hide account info panel if it's open
                if (accountInfoPanel != null)
                    accountInfoPanel.SetActive(false);
                
                loginPanel.SetActive(false);
                homePanel.SetActive(false);
                welcmePanel.SetActive(true);

                // Clear sensitive fields
                if (loginPassword != null)
                    loginPassword.text = "";
                if (deleteConfirmPassword != null)
                    deleteConfirmPassword.text = "";
                
            }
            catch (Exception e)
            {
                Debug.LogError("❌ Error signing out: " + e.Message);
                DisplayAccountError("Something went wrong while signing out. Please try again.");
            }
        }
    }

    // Delete user account
    public async void DeleteUserAccount()
    {
        if (user == null || auth == null)
        {
            DisplayAccountError("You need to be logged in to delete your account.");
            return;
        }

        // Check if password confirmation was provided
        if (deleteConfirmPassword == null || string.IsNullOrEmpty(deleteConfirmPassword.text))
        {
            DisplayAccountError("Please enter your password to confirm account deletion.");
            return;
        }

        try
        {
            // Re-authenticate user before deletion (required by Firebase)
            var credential = EmailAuthProvider.GetCredential(user.Email, deleteConfirmPassword.text);
            await user.ReauthenticateAsync(credential);
            
            // Get user ID to delete Firestore data after auth account deletion
            string userId = user.UserId;
            
            // Delete from Firebase Authentication
            await user.DeleteAsync();
            
            // Delete user data from Firestore
            await DeleteUserData(userId);
            
            Debug.Log("✅ User account deleted successfully");
            
            // Return to login screen
            loginPanel.SetActive(true);
            homePanel.SetActive(false);
            if (accountInfoPanel != null)
                accountInfoPanel.SetActive(false);
                
            // Clear sensitive fields
            if (loginPassword != null)
                loginPassword.text = "";
            if (deleteConfirmPassword != null)
                deleteConfirmPassword.text = "";
        }
        catch (FirebaseException ex)
        {
            // Handle specific Firebase errors
            AuthError errorCode = (AuthError)ex.ErrorCode;
            
            if (errorCode == AuthError.WrongPassword)
            {
                DisplayAccountError("Incorrect password. Please try again.");
            }
            else
            {
                Debug.LogError("❌ Firebase error deleting account: " + ex.Message);
                DisplayAccountError("Error deleting account: " + ex.Message);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Error deleting account: " + e.Message);
            DisplayAccountError("Something went wrong while deleting your account. Please try again.");
        }
    }
    
    // Delete user data from Firestore
    private async Task DeleteUserData(string userId)
    {
        try
        {
            // Delete user document
            await db.Collection("users").Document(userId).DeleteAsync();
            
            Debug.Log("✅ User data deleted from Firestore");
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Error deleting user data from Firestore: " + e.Message);
            // We don't throw here because the authentication account is already deleted
        }
    }
    
    // Display error message in the account info panel
    void DisplayAccountError(string message)
    {
        if (accountErrorText != null)
        {
            accountErrorText.text = message;
            accountErrorText.gameObject.SetActive(true);
        }
        Debug.LogError("Account Error: " + message);
    }

    #endregion

    #region Firebase Auth State

    void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth != null && auth.CurrentUser != user)
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

    #endregion

    #region UI Navigation

    void ShowHomePanel()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        homePanel.SetActive(true);
    }

    #endregion
    
    // Public methods for accessing Firebase services from other scripts
    public FirebaseAuth GetAuth()
    {
        return auth;
    }
    
    public FirebaseFirestore GetFirestore()
    {
        return db;
    }
    
    public FirebaseUser GetCurrentUser()
    {
        return user;
    }
}