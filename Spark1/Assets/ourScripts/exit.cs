using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class exit : MonoBehaviour
{
    public GameObject dialogPanel;
    public Button Homebutton;
    private GameObject startSceneRoot;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f);

        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        if (Homebutton != null)
            Homebutton.interactable = true;
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
        Application.Quit();
    }

    public void LeaveToStartScene()
    {
        Debug.Log("↩️ Exiting story and restoring Start scene...");

        // ✅ Use cached reference from LoadEnvironmentScene.cs
        if (startSceneRoot == null)
        {
            startSceneRoot = LoadEnvironmentScene.cachedStartRoot;
        }

        if (startSceneRoot != null)
        {
            startSceneRoot.SetActive(true);

            // Refresh all Canvas components to fix any rendering glitches
            Canvas[] canvases = startSceneRoot.GetComponentsInChildren<Canvas>(true);
            foreach (Canvas c in canvases)
            {
                c.enabled = false;
                c.enabled = true;
            }

            Debug.Log("✅ Start scene reactivated.");
        }
        else
        {
            Debug.LogWarning("⚠️ StartSceneRoot is missing. Cannot restore Start scene.");
        }

        // ✅ Unload the story scene
        if (SceneManager.GetSceneByName("Environment_Free 1").isLoaded)
        {
            SceneManager.UnloadSceneAsync("Environment_Free 1");
        }

        // ✅ Make Start the active scene again
        Scene startScene = SceneManager.GetSceneByName("Start");
        if (startScene.IsValid())
        {
            SceneManager.SetActiveScene(startScene);
        }
    }
}
