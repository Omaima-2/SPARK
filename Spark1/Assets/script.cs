using UnityEngine;
using UnityEngine.SceneManagement;

public class script : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
