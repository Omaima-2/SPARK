using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadEnvironmentScene : MonoBehaviour
{
    public GameObject startSceneRoot; // Assign this in the Inspector
    public static GameObject cachedStartRoot; // Used by exit.cs to restore Start

    public void LoadStoryScene()
    {
        if (startSceneRoot != null)
        {
            startSceneRoot.SetActive(false);        // ✅ Hide Start scene
            cachedStartRoot = startSceneRoot;       // ✅ Save reference for later
        }

        SceneManager.LoadScene("Environment_Free 1", LoadSceneMode.Additive); // ✅ Additive load
        SceneManager.sceneLoaded += OnEnvironmentSceneLoaded;
    }

    private void OnEnvironmentSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Environment_Free 1")
        {
            SceneManager.SetActiveScene(scene); // ✅ Focus the story scene
            Debug.Log("✅ Environment scene is now active.");
            SceneManager.sceneLoaded -= OnEnvironmentSceneLoaded; // Clean up listener
        }
    }
}
