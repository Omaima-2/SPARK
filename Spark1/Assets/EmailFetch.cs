using UnityEngine;
using Firebase;
using Firebase.Auth;
using UnityEngine.UI;

public class NameFetch : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseUser user;

    public Text nameText; // Assign a UI Text component in Unity

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        FetchUserName();
    }

    public void FetchUserName()
    {
        user = auth.CurrentUser;
        if (user != null)
        {
            string userName = user.DisplayName; // Fetch user name
            Debug.Log("User Name: " + userName);

            if (!string.IsNullOrEmpty(userName))
            {
                if (nameText != null)
                {
                    nameText.text = userName;
                }
            }
            else
            {
                Debug.Log("User name is not set.");
                if (nameText != null)
                {
                    nameText.text = "No name available.";
                }
            }
        }
        else
        {
            Debug.Log("No user is currently signed in.");
            if (nameText != null)
            {
                nameText.text = "No user signed in.";
            }
        }
    }
}
