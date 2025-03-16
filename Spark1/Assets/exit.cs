using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class exit : MonoBehaviour
{
    public GameObject dialogPanel; // Assign your Panel in the inspector

    void Start()
    {
        // Ensure the dialog is hidden at the start
        dialogPanel.SetActive(false);
    }

    // Function to open the dialog
    public void ShowDialog()
    {
        dialogPanel.SetActive(true);
    }

    // Function to close the dialog
    public void HideDialog()
    {
        dialogPanel.SetActive(false);
    }

    // Function to handle exiting the game
    public void ExitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit(); // Quits the game (works in build, not in editor)
    }
}

