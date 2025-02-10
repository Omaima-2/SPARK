using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;  // Fix the incorrect 'using FirebaseAuth'
using System;
using System.Threading.Tasks;

public class FirebaseController : MonoBehaviour
{
    private FirebaseAuth auth;  // Declare FirebaseAuth here
    private FirebaseUser user;  // Declare FirebaseUser here

    public GameObject loginPanel, signupPanel; 
    public InputField loginEmail, loginPassword, signupEmail, signupPassword, signupCPassword, signupUsername; 

void Start()
{
    InitializeFirebase();
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
            Debug.Log("Login fields cannot be empty.");
            return;
        }
        
        signInUser(loginEmail.text, loginPassword.text);
    }

    public void SignupUser()
    {
        if (string.IsNullOrEmpty(signupEmail.text) || string.IsNullOrEmpty(signupPassword.text) || string.IsNullOrEmpty(signupCPassword.text))
        {
            Debug.Log("Signup fields cannot be empty.");
            return;
        }

        if (signupPassword.text != signupCPassword.text)
        {
            Debug.Log("Passwords do not match.");
            return;
        }

        createUser(signupEmail.text, signupPassword.text, signupUsername.text);
    }

    void createUser(string email, string password, string userName)
    {
      if (auth == null)
    {
        Debug.LogError("FirebaseAuth instance is null. Initialize Firebase first.");
        return;
    }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            user = auth.CurrentUser;
        if (user != null)
        {
            Debug.LogFormat("Firebase user created successfully: {0} ({1})", user.DisplayName, user.UserId);
            updateProfile(userName);
        }

        });
    }

    public void signInUser(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            FirebaseUser newUser = auth.CurrentUser;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);

        });
    }

    void InitializeFirebase()
{
    FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
    {
        if (task.Result == DependencyStatus.Available)
        {
            auth = FirebaseAuth.DefaultInstance;
            auth.StateChanged += AuthStateChanged;
            AuthStateChanged(this, null);
            Debug.Log("Firebase is initialized successfully.");
        }
        else
        {
            Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
        }
    });
}

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null
                && auth.CurrentUser.IsValid();
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
            }
        }
    }

    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }

    void updateProfile(string userName)
    {
        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            UserProfile profile = new UserProfile {
                DisplayName = userName,
                PhotoUrl = new Uri("https://picsum.photos/200"),
            };
            user.UpdateUserProfileAsync(profile).ContinueWith(task => {
                if (task.IsCanceled) {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User profile updated successfully.");
            });
        }
    }
}

