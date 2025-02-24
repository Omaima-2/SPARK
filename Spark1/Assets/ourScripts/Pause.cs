using UnityEngine; 


public class Pause : MonoBehaviour
{
    private bool isPaused = false;

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeStory();
        }
        else
        {
            PauseStory();
        }
    }

    private void PauseStory()
    {
        Time.timeScale = 0f;  // Pause everything
        isPaused = true;
        Debug.Log("Story Paused");
    }

    private void ResumeStory()
    {
        Time.timeScale = 1f;  // Resume everything
        isPaused = false;
        Debug.Log("Story Resumed");
    }
}