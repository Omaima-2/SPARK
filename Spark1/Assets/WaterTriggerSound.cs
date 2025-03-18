using UnityEngine;

public class WaterTriggerSound : MonoBehaviour
{
    public AudioSource audioSource; // Assign in Inspector

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>(); // Auto-assign if not set
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"🚶 Trigger detected by: {other.name}");

        if (other.CompareTag("Player")) // Ensure the girl has the correct tag
        {
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.Play();
                Debug.Log("🎵 Water sound started!");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (audioSource != null)
            {
                audioSource.Stop();
                Debug.Log("⏹️ Water sound stopped!");
            }
        }
    }
}
