using UnityEngine;
using Firebase;
using Firebase.Auth;
using UnityEngine.UI;

public class EmailFetch : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseUser user;

    public Text emailText; // Assign a UI Text component in Unity

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        FetchUserEmail();
    }

    public void FetchUserEmail()
    {
        user = auth.CurrentUser;
        if (user != null)
        {
            string userEmail = user.Email;
            Debug.Log("User Email: " + userEmail);
            if (emailText != null)
            {
                emailText.text = "Email: " + userEmail;
            }
        }
        else
        {
            Debug.Log("No user is currently signed in.");
            if (emailText != null)
            {
                emailText.text = "No user signed in.";
            }
        }
    }
}
