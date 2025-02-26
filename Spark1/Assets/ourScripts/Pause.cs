using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    private bool isPaused = false;
    public AudioSource birdsAudioSource; // Assign this manually in the Inspector
    public GameObject pauseMenuUI; // Assign a UI panel for the pause menu (optional)

    private void Start()
    {
        // Check if the AudioSource is assigned
        if (birdsAudioSource == null)
        {
            Debug.LogError("⚠️ ERROR: BirdsAudioSource is NOT assigned in the Inspector!");
        }
        else
        {
            Debug.Log("✅ BirdsAudioSource successfully assigned: " + birdsAudioSource.gameObject.name);
        }

        // Hide the pause menu if assigned
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }

    public void TogglePause()
    {
        Debug.Log("🎯 Pause Button Clicked!"); // Check if the button is working

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
        Debug.Log("🔍 Before Pause: Time.timeScale = " + Time.timeScale);
        Time.timeScale = 0f; // Pause the game
        Debug.Log("🔍 After Pause: Time.timeScale = " + Time.timeScale);

        if (birdsAudioSource != null && birdsAudioSource.isPlaying)
        {
            birdsAudioSource.Pause(); // Pause instead of Stop
            Debug.Log("✅ Bird Sound Paused!");
        }
        else
        {
            Debug.LogWarning("⚠️ WARNING: BirdsAudioSource is NULL or not playing!");
        }

        // Show Pause UI
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }

        isPaused = true;
        Debug.Log("✅ Game Paused!");
    }

    private void ResumeStory()
    {
        Debug.Log("🔍 Before Resume: Time.timeScale = " + Time.timeScale);
        Time.timeScale = 1f; // Resume the game
        Debug.Log("🔍 After Resume: Time.timeScale = " + Time.timeScale);

        if (birdsAudioSource != null)
        {
            birdsAudioSource.UnPause(); // Resume from where it left off
            Debug.Log("✅ Bird Sound Resumed!");
        }

        // Hide Pause UI
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        isPaused = false;
        Debug.Log("✅ Game Resumed!");
    }
}
