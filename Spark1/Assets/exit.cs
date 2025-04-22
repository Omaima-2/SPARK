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

    // NEW METHOD to switch scene and target a specific page
    public void LeaveToSceneWithPage(int pageIndex)
    {
        NavigationData.targetPanelIndex = 4; // Set this to whatever index matches child_Home
        SceneManager.LoadScene("Start"); // Load the UI scene
       
    }

}
