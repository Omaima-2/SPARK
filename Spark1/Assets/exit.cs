using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections; // 👈 This gives access to IEnumerator


public class exit : MonoBehaviour
{
    public GameObject dialogPanel; // Assign in Inspector
    public Button Homebutton;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f); // Allow other scripts to run

        if (Homebutton == null)
        {
            Debug.LogError("❌ Homebutton is NOT assigned in the Inspector!");
        }
        else
        {
            dialogPanel.SetActive(false);
            Debug.Log("🧹 Forced home panel to deactivate after delay.");
            Homebutton.interactable = true;
        }

        Debug.Log("👋 Start() coroutine finished in Exit.cs");
    }

    public void ShowDialog()
    {
        dialogPanel.SetActive(true);
        Homebutton.interactable = false;
    }

    public void HideDialog()
    {
        dialogPanel.SetActive(false);
        Homebutton.interactable = true;
    }

    public void ExitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit();
    }

    // ✅ NEW METHOD: Leave story and go to Start scene safely
    public void LeaveToSceneWithPage()
    {
        // First: Check if child is logged in
        string currentChildId = PlayerPrefs.GetString("CurrentChildID", "");
        Debug.Log($"🔵 CurrentChildID when exiting story: {currentChildId}");


        if (string.IsNullOrEmpty(currentChildId))
        {
            Debug.LogWarning("⚠️ No current child ID found! Going back to default page.");
            PlayerPrefs.SetInt("CurrentPageIndex", 0); // Default: Welcome page
        }
        else
        {
            Debug.Log($"🎯 Child '{currentChildId}' exiting story. Setting target page index to 4.");
            PlayerPrefs.SetInt("CurrentPageIndex", 4); // Child Home page index
        }

        PlayerPrefs.Save();

        // Now load Start scene
        SceneManager.LoadScene("Start");
    }
}
