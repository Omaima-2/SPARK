using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    private bool isPaused = false;
    public AudioSource birdsAudioSource; // Assign manually in the Inspector
    public GameObject pauseMenuUI; // Assign a UI panel for the pause menu (optional)
    private Ddbmanager audioManager; // Reference to Firebase Audio Manager

    private void Start()
    {
        // Find Ddbmanager script in the scene
        audioManager = FindObjectOfType<Ddbmanager>();

        if (audioManager == null)
        {
            Debug.LogError("⚠️ ERROR: Ddbmanager script not found in the scene!");
        }

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
        Debug.Log("🎯 Pause Button Clicked!");

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

        // Pause local audio
        if (birdsAudioSource != null && birdsAudioSource.isPlaying)
        {
            birdsAudioSource.Pause();
            Debug.Log("✅ Bird Sound Paused!");
        }
        else
        {
            Debug.LogWarning("⚠️ WARNING: BirdsAudioSource is NULL or not playing!");
        }

        // Mute Firebase Audio
        if (audioManager != null)
        {
            audioManager.MuteAudio();
            Debug.Log("✅ Firebase Audio Muted!");
        }
        else
        {
            Debug.LogWarning("⚠️ WARNING: AudioManager not found!");
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

        // Resume local audio
        if (birdsAudioSource != null)
        {
            birdsAudioSource.UnPause();
            Debug.Log("✅ Bird Sound Resumed!");
        }

        // Unmute Firebase Audio
        if (audioManager != null)
        {
            audioManager.UnmuteAudio();
            Debug.Log("✅ Firebase Audio Unmuted!");
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