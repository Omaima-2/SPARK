
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks; // ✅ Needed for async

public class LoadEnvironmentScene : MonoBehaviour
{
    public GameObject startSceneRoot; // Assign in Inspector
    public static GameObject cachedStartRoot;

   public async void LoadStoryScene()
{
    if (startSceneRoot != null)
    {
        startSceneRoot.SetActive(false);
        cachedStartRoot = startSceneRoot;
    }
    

    SceneManager.LoadScene("Environment_Free 1", LoadSceneMode.Additive);
    SceneManager.sceneLoaded += OnEnvironmentSceneLoaded;
}


    private void OnEnvironmentSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Environment_Free 1")
        {
            SceneManager.SetActiveScene(scene);
            Debug.Log("✅ Environment scene is now active.");
            SceneManager.sceneLoaded -= OnEnvironmentSceneLoaded;
        }
    }
}
