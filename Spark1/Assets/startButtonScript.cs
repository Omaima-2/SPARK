using UnityEngine;
using UnityEngine.SceneManagement;
public class startButtonScript : MonoBehaviour
{
    public void LoadScene()
    {
        SceneManager.LoadScene("Demo"); // Make sure the scene is added to Build Settings
    }
}
