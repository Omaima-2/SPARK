using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    private bool isPaused = false;

    public AudioSource birdsAudioSource; // Assign manually in the Inspector
    public AudioSource waterAudioSource; // Assign in Inspector

    public GameObject pauseMenuUI;       // Optional: assign a UI panel for the pause menu
    public Sprite pauseSprite;           // Icon shown when the story is playing
    public Sprite resumeSprite;          // Icon shown when the story is paused
    public Button pauseButton;           // Button with Source Image to update icon

    private Ddbmanager audioManager;     // Reference to Firebase Audio Manager

    private bool wasSecondaryAudioPlaying = false;
    private bool wasSecondaryAudioMuted = true;

    public void Start()
    {
        // Find Ddbmanager in the scene
        audioManager = FindObjectOfType<Ddbmanager>();

        if (audioManager == null)
        {
            Debug.LogError("⚠️ ERROR: Ddbmanager script not found in the scene!");
        }

        if (birdsAudioSource == null)
        {
            Debug.LogError("⚠️ ERROR: BirdsAudioSource is NOT assigned in the Inspector!");
        }
        else
        {
            Debug.Log("✅ BirdsAudioSource successfully assigned: " + birdsAudioSource.gameObject.name);
        }

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

    public void PauseStory()
    {
        Debug.Log("🔍 Before Pause: Time.timeScale = " + Time.timeScale);
        Time.timeScale = 0f;
        Debug.Log("🔍 After Pause: Time.timeScale = " + Time.timeScale);

        if (birdsAudioSource != null && birdsAudioSource.isPlaying)
        {
            birdsAudioSource.Pause();
            Debug.Log("✅ Bird Sound Paused!");
        }

        if (audioManager != null && audioManager.audioSource != null)
        {
            wasSecondaryAudioMuted = audioManager.isMuted;
            wasSecondaryAudioPlaying = audioManager.audioSource.isPlaying;
            audioManager.audioSource.Pause();
            Debug.Log("🔇 Firebase Audio Paused!");
        }

        if (audioManager != null)
        {
            audioManager.MuteAudio();
        }

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }

        if (pauseButton != null && resumeSprite != null)
        {
            pauseButton.image.sprite = resumeSprite;
            Debug.Log("🔄 Pause button sprite changed to Resume.");
        }

        isPaused = true;
        Debug.Log("✅ Game Paused!");
        if (waterAudioSource != null && waterAudioSource.isPlaying)
{
    waterAudioSource.Pause();
    Debug.Log("💧 Water Sound Paused!");
}

    }

    public void ResumeStory()
{
    Debug.Log("🔁 ResumeStory() CALLED!");
    Debug.Log("🔍 Before Resume: Time.timeScale = " + Time.timeScale);
    Time.timeScale = 1f;
    Debug.Log("🔍 After Resume: Time.timeScale = " + Time.timeScale);

    if (birdsAudioSource != null)
    {
        birdsAudioSource.UnPause();
        Debug.Log("✅ Bird Sound Resumed!");
    }

    if (audioManager != null && audioManager.audioSource != null)
    {
        if (wasSecondaryAudioPlaying)
        {
            if (!wasSecondaryAudioMuted)
            {
                audioManager.UnmuteAudio();
                audioManager.audioSource.UnPause();
                Debug.Log("🔊 Firebase Audio Resumed & Unmuted!");
            }
            else
            {
                audioManager.MuteAudio();
                Debug.Log("🔕 Firebase Audio stayed muted.");
            }
        }
    }

    if (waterAudioSource != null)
    {
        waterAudioSource.UnPause();
        Debug.Log("💧 Water Sound Resumed!");
    }

    if (pauseMenuUI != null)
    {
        pauseMenuUI.SetActive(false);
        Debug.Log("📴 Pause menu hidden.");
    }

    if (pauseButton != null && pauseSprite != null)
    {
        pauseButton.image.sprite = pauseSprite;
        Debug.Log("🔄 Pause button sprite changed to Pause.");
    }

    isPaused = false;
    Debug.Log("✅ Game Resumed! isPaused = " + isPaused);
}

}
