using UnityEngine;

public class Pause : MonoBehaviour
{
    private bool isPaused = false;
    public AudioSource birdsAudioSource; // Assign this manually in the Inspector

    private void Start()
    {
        if (birdsAudioSource == null)
        {
            Debug.LogError("⚠️ ERROR: BirdsAudioSource is NOT assigned in the Inspector!");
        }
        else
        {
            Debug.Log("✅ BirdsAudioSource successfully assigned: " + birdsAudioSource.gameObject.name);
        }
    }

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
        Time.timeScale = 0f; // Pause the game

        if (birdsAudioSource != null && birdsAudioSource.isPlaying)
        {
            birdsAudioSource.Stop(); // Completely stops the sound
            Debug.Log("✅ Bird Sound Stopped!");
        }
        else
        {
            Debug.LogWarning("⚠️ WARNING: BirdsAudioSource is NULL or not playing!");
        }

        isPaused = true;
        Debug.Log("✅ Game Paused!");
    }

    private void ResumeStory()
    {
        Time.timeScale = 1f; // Resume the game

        if (birdsAudioSource != null)
        {
            birdsAudioSource.Play(); // Starts from the beginning
            Debug.Log("✅ Bird Sound Resumed!");
        }

        isPaused = false;
        Debug.Log("✅ Game Resumed!");
    }
}
