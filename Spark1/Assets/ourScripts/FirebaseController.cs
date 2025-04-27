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

    [Header("UI Panels")]
    public GameObject loginPanel, signupPanel, homePanel, accountInfoPanel, welcmePanel;
    
    [Header("Login/Signup UI elements")]
    public InputField loginEmail, loginPassword, signupEmail, signupPassword, signupCPassword, signupName;
    public TextMeshProUGUI errorTextSignUp , errorTextLogin;
    
    [Header("Parent page UI elements ")]
    public Button logoutButton;
    public Button accountNameButton;
    public TextMeshProUGUI usernameText;
    
    [Header("Account Panel UI")]
    public TextMeshProUGUI displayUsernameText;
    public TextMeshProUGUI emailText;
    public GameObject editNamePanel; 
    public TMP_InputField editNameInputField;
    public TextMeshProUGUI editNameErrorText;
    
    [Header("Delete Account")]
    public Button deleteAccountButton;
    public InputField deleteConfirmPassword;
    public TextMeshProUGUI accountErrorText;
    
    // User change delegate and event
    public delegate void UserChangedEventHandler(FirebaseUser oldUser, FirebaseUser newUser);
    public event UserChangedEventHandler OnUserChanged;

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

        // 🔥 Validate Email Format
        if (!IsValidEmail(signupEmail.text)) 
        {
            DisplayError("Hmm..That doesn't look like a valid email. try again!✨");
            return;
        }

        if (signupPassword.text != signupCPassword.text)
        {
            DisplayError("Oops! Your passwords don't match. Try again!");
            return;
        }

        // Check if name is valid 
        if (!IsValidName(signupName.text))
        {
            DisplayError("Name can only contain letters, spaces, and common punctuation. Maximum 30 characters allowed!");
            return;
        }

        CreateUser(signupEmail.text, signupPassword.text, signupName.text);
    }

    private bool IsValidName(string name)
    {
        if (string.IsNullOrEmpty(name) || name.Length > 20 )
            return false;

        return true;
    }

    async void CreateUser(string email, string password, string displayName)
    {
        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);

            // ✅ Always re-fetch current user after signup to avoid stale data issues
            user = auth.CurrentUser;

            if (user != null)
            {
                Debug.LogFormat("✅ User created and logged in: {0} ({1})", user.Email, user.UserId);

                // Update Firebase Authentication Profile with the display name
                UserProfile profile = new UserProfile { DisplayName = displayName };
                await user.UpdateUserProfileAsync(profile);
                Debug.Log("✅ Display name updated.");

                // Store user data in Firestore
                DocumentReference userRef = db.Collection("users").Document(user.UserId);
                await userRef.SetAsync(new Dictionary<string, object>
            {
                { "displayName", displayName },
                { "email", email },
                { "createdAt", FieldValue.ServerTimestamp }
            });
                Debug.Log("✅ User info stored in Firestore.");

                // ✅ Load full user data if needed (just like login)
                await LoadUserData(user.UserId);

                // Move to home page
                ShowHomePanel();

                // Fire user change event manually
                OnUserChanged?.Invoke(null, user);
                Debug.Log("🆕 Fired OnUserChanged after signup.");

                // Clear and reload children
                if (ChildAccountManager.Instance != null)
                {
                    ChildAccountManager.Instance.ClearChildData();
                    ChildAccountManager.Instance.LoadChildAccounts();
                }
            }
        }
        catch (FirebaseException firebaseEx)
        {
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            switch (errorCode)
            {
                case AuthError.EmailAlreadyInUse:
                    DisplayError("Oops! This email is already taken. Try another one.");
                    break;
                case AuthError.InvalidEmail:
                    DisplayError("Hmm... That doesn't look like a valid email. Try again!");
                    break;
                case AuthError.WeakPassword:
                    DisplayError("Oops! Your password is too short. Make it at least 6 characters long!");
                    break;
                default:
                    DisplayError("Something went wrong, but don't worry! Try again soon.");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("🔥 Unexpected Error: " + e.Message);
            DisplayError("Uh-oh! Something went wrong. Try again!");
        }
    }


    async void SignInUser(string email, string password)
    {
        if (!IsFirebaseInitialized)
        {
            DisplayError("Please wait a moment, Firebase is still initializing...", true);
            Debug.LogWarning("⚠ Tried to login before Firebase initialized!");
            return;
        }
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

                // Fire user change event
                if (OnUserChanged != null)
                {
                    OnUserChanged(null, user);
                    Debug.Log("🔄 Fired OnUserChanged manually after login");
                }

                // Force reload children just in case
                if (ChildAccountManager.Instance != null)
                {
                    ChildAccountManager.Instance.ClearChildData();
                    ChildAccountManager.Instance.LoadChildAccounts();
                }
            }
        }
        catch (FirebaseException firebaseEx)
        {
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
                
                string displayName = "";
                if (userData.TryGetValue("displayName", out object displayNameObj))
                {
                    displayName = displayNameObj.ToString();
                }
                else if (user.DisplayName != null)
                {
                    // Fallback to Auth DisplayName if Firestore doesn't have display name
                    displayName = user.DisplayName;
                }
                
                // Update all UI elements with the display name
                if (!string.IsNullOrEmpty(displayName))
                {
                    if (usernameText != null)
                    {
                        usernameText.text = displayName;
                    }
                    
                    if (displayUsernameText != null)
                    {
                        displayUsernameText.text = displayName;
                    }
                    
                    Debug.Log($"✅ Loaded user data with display name: {displayName}");
                }
                
                // Update email display
                if (emailText != null && user != null)
                {
                    emailText.text = user.Email ?? "";
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

    // Logout functionality
    public void LogoutUser()
    {
        if (auth != null)
        {
            try
            {
                // Store old user before logout
                FirebaseUser oldUser = user;
                
                // Sign out from Firebase
                auth.SignOut();
                
                // Update reference
                user = null;
                
                // Clear child data before triggering user change
                if (ChildAccountManager.Instance != null)
                {
                    ChildAccountManager.Instance.ClearChildData();
                    Debug.Log("✅ Cleared child data manually before logout.");
                }

                // Manually trigger OnUserChanged event
                if (OnUserChanged != null)
                {
                    OnUserChanged(oldUser, null);
                }
                
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
 public async void LogoutAndDeleteUser()
{
    if (auth != null && user != null)
    {
        try
        {
            // Store old user before deletion
            FirebaseUser oldUser = user;

            // Clear sensitive fields early
            if (loginPassword != null)
                loginPassword.text = "";
            if (deleteConfirmPassword != null)
                deleteConfirmPassword.text = "";

            // Delete the user from Firebase
            await user.DeleteAsync();
            Debug.Log("✅ User account deleted successfully.");

            // Sign out from Firebase
            auth.SignOut();
            user = null;

            // Clear child data before triggering user change
            if (ChildAccountManager.Instance != null)
            {
                ChildAccountManager.Instance.ClearChildData();
                Debug.Log("✅ Cleared child data manually after delete.");
            }

            // Manually trigger OnUserChanged event
            if (OnUserChanged != null)
            {
                OnUserChanged(oldUser, null);
            }

            // Handle UI panels
            if (accountInfoPanel != null)
                accountInfoPanel.SetActive(false);

            loginPanel.SetActive(false);
            homePanel.SetActive(false);
            welcmePanel.SetActive(true);
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Error during delete/logout: " + e.Message);
            DisplayAccountError("Something went wrong while deleting your account. Please try again.");
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

    // Updated AuthStateChanged method to use the OnUserChanged event
    void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth != null)
        {
            FirebaseUser oldUser = user;
            FirebaseUser newUser = auth.CurrentUser;

            // Check if the user has actually changed
            if (oldUser != newUser)
            {
                if (newUser == null)
                {
                    Debug.Log("🔴 User signed out");
                    // Clear all user-related UI
                    ClearUserUI();
                }
                else if (oldUser == null)
                {
                    Debug.Log($"🟢 User signed in: {newUser.UserId}");
                    // Load user data and update UI
                    if (newUser.UserId != null)
                    {
                        _ = LoadUserData(newUser.UserId);
                    }
                }
                else
                {
                    Debug.Log($"🔄 User changed from {oldUser.UserId} to {newUser.UserId}");
                    // Clear previous user data and load new user data
                    ClearUserUI();
                    if (newUser.UserId != null)
                    {
                        _ = LoadUserData(newUser.UserId);
                    }
                }

                // Update our user reference
                user = newUser;

                // Trigger the event
                if (OnUserChanged != null)
                {
                    OnUserChanged(oldUser, newUser);
                }
            }
        }
    }

    private void ClearUserUI()
{
    // Clear all UI fields that contain user information
    if (usernameText != null)
    {
        usernameText.text = "";
    }
    
    if (displayUsernameText != null)
    {
        displayUsernameText.text = "";
    }
    
    if (emailText != null)
    {
        emailText.text = "";
    }
    
    // Clear the edit name input field too
    if (editNameInputField != null)
    {
        editNameInputField.text = "";
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

    // Add this method to update the account info panel when it's opened
    public void OpenAccountInfoPanel()
    {
        if (user != null)
        {
            // Update the account info panel with current user information
            if (displayUsernameText != null)
            {
                displayUsernameText.text = user.DisplayName ?? "User";
            }
            
            if (emailText != null)
            {
                emailText.text = user.Email ?? "";
            }
            
            accountInfoPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("⚠ Cannot open account info panel: User is not logged in.");
        }
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

    #region Account Name Editing

    public void SaveEditedName()
{
    string newName = editNameInputField.text.Trim();

    // Display errors in the edit name panel specifically
    if (string.IsNullOrEmpty(newName))
    {
        DisplayEditNameError("Name cannot be empty.");
        return;
    }
    
    if (newName.Length < 3)
    {
        DisplayEditNameError("Name must be at least 3 characters long.");
        return;
    }
    
    if (newName.Length > 20)
    {
        DisplayEditNameError("Name cannot exceed 20 characters.");
        return;
    }
    
    if (!System.Text.RegularExpressions.Regex.IsMatch(newName, @"^[a-zA-Z0-9\s\.\-_']+$"))
    {
        DisplayEditNameError("Name can only contain letters, numbers, spaces, and common punctuation (. - _ ').");
        return;
    }

    UpdateParentDisplayName(newName);
}

private void DisplayEditNameError(string message)
{
    if (editNameErrorText != null)
    {
        editNameErrorText.text = message;
        editNameErrorText.gameObject.SetActive(true);
    }
    else
    {
        // Fallback to the account error text if the specific one isn't assigned
        DisplayAccountError(message);
    }
    Debug.LogWarning("Edit Name Error: " + message);
}

    // Actually update it in Firebase Auth and Firestore
    private async void UpdateParentDisplayName(string newName)
    {
        try
        {
            // Update Firebase Authentication profile
            UserProfile profile = new UserProfile { DisplayName = newName };
            await user.UpdateUserProfileAsync(profile);
            Debug.Log("✅ Updated display name in Firebase Authentication");

            // Update Firestore database
            DocumentReference userRef = db.Collection("users").Document(user.UserId);
            await userRef.UpdateAsync("displayName", newName);
            Debug.Log("✅ Updated display name in Firestore");

            // Update ALL UI elements that display the username
            if (displayUsernameText != null)
            {
                displayUsernameText.text = newName;
            }
            
            // Update the username in the parent page
            if (usernameText != null)
            {
                usernameText.text = newName;
            }

            // Close the edit panel
            editNamePanel.SetActive(false);
            Debug.Log("🎉 Name updated and edit panel closed");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Error updating name: {e.Message}");
            DisplayAccountError("Failed to update name. Try again later.");
        }
    }

// Add this method to open and initialize the edit name panel
public void OpenEditNamePanel()
{
    if (user != null)
    {
        // Set the input field to current user's name
        if (editNameInputField != null)
        {
            editNameInputField.text = user.DisplayName ?? "";
        }
        
        // Clear any previous error messages
        if (editNameErrorText != null)
        {
            editNameErrorText.text = "";
            editNameErrorText.gameObject.SetActive(false);
        }
        
        editNamePanel.SetActive(true);
    }
    else
    {
        Debug.LogWarning("Cannot open edit name panel: User is not logged in");
    }
}

// Add this method to your FirebaseController class
public void InitEditNamePanel()
{
    if (user != null)
    {
        // Set the input field to current user's name
        if (editNameInputField != null)
        {
            editNameInputField.text = user.DisplayName ?? "";
        }
        
        // Clear any previous error messages
        if (editNameErrorText != null)
        {
            editNameErrorText.text = "";
            editNameErrorText.gameObject.SetActive(false);
        }
    }
}
    #endregion
}