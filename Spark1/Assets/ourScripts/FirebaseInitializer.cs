using UnityEngine;
using Firebase;

public class FirebaseInitializer : MonoBehaviour
{
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Firebase initialized successfully!");
            }
            else
            {
                Debug.LogError("Firebase initialization failed: " + task.Exception);
            }
        });
    }
}
