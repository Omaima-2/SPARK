using UnityEngine;

public class AreaAudioManager : MonoBehaviour
{
    public AudioSource audioSource; // Reference to the AudioSource

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            if (!audioSource.isPlaying) // Play audio if not already playing
            {
                audioSource.Play();
                Debug.Log("Audio started playing for area: " + gameObject.name);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the player is exiting the trigger
        if (other.CompareTag("Player"))
        {
            if (audioSource.isPlaying) // Stop audio when player leaves the area
            {
                audioSource.Stop();
                Debug.Log("Audio stopped for area: " + gameObject.name);
            }
        }
    }
}
