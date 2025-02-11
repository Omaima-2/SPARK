using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;

public class FirebaseController : MonoBehaviour
{
    private FirebaseAuth auth;  
    private FirebaseUser user;  

    public GameObject loginPanel, signupPanel; 
    public InputField loginEmail, loginPassword, signupEmail, signupPassword, signupCPassword;  

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

        SignInUser(loginEmail.text, loginPassword.text);
    }

    public void SignupUser()
    {
        if (string.IsNullOrEmpty(signupEmail.text) || 
            string.IsNullOrEmpty(signupPassword.text) || 
            string.IsNullOrEmpty(signupCPassword.text))
        {
            Debug.Log("Signup fields cannot be empty.");
            return;
        }

        if (signupPassword.text != signupCPassword.text)
        {
            Debug.Log("Passwords do not match.");
            return;
        }

        CreateUser(signupEmail.text, signupPassword.text);
    }

    async void CreateUser(string email, string password)
    {
        if (auth == null)
        {
            Debug.LogError("FirebaseAuth instance is null. Initialize Firebase first.");
            return;
        }

        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            user = result.User;

            if (user != null)
            {
                Debug.LogFormat("User created successfully: {0} ({1})", user.Email, user.UserId);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error creating user: " + e.Message);
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
                Debug.LogFormat("User signed in successfully: {0} ({1})", user.Email, user.UserId);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error signing in: " + e.Message);
        }
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
                Debug.Log("Firebase initialized successfully.");
            }
            else
            {
                Debug.LogError("Could not resolve Firebase dependencies: " + task.Result);
            }
        });
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out: " + user.UserId);
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Signed in: " + user.UserId);
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
}
