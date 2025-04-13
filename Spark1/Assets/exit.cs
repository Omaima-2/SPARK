using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class exit : MonoBehaviour
{
    public GameObject dialogPanel; // Assign your Panel in the inspector
    public Button Homebutton;
   void Start()
{
    if (Homebutton == null)
    {
        Debug.LogError("❌ Homebutton is NOT assigned in the Inspector!");
    }
    else
    {
        dialogPanel.SetActive(false);
        Homebutton.interactable = true;
    }
}


    // Function to open the dialog
    public void ShowDialog()
    {
        dialogPanel.SetActive(true);
        Homebutton.interactable = false; // 🔒 Disable the button
    }

    // Function to close the dialog
    public void HideDialog()
    {
        dialogPanel.SetActive(false);
        Homebutton.interactable = true; // ✅ Re-enable the button

    }

    // Function to handle exiting the game
    public void ExitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit(); // Quits the game (works in build, not in editor)
    }
}

