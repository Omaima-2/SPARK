using UnityEngine;

public class FrameTrigger : MonoBehaviour
{
    public bool isTriggered = false;
    public GameObject nextFrame; // Assign the next frame in Unity Inspector

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger detected by: {other.name}");
        if (other.CompareTag("Player")) // Check if it's the Boy character
        {
            isTriggered = true;
            Debug.Log("Player reached the trigger point for frame2.");
        }
    }

    public void TriggerNextFrame()
    {
        if (nextFrame != null)
        {
            nextFrame.SetActive(true); // Activate the next frame
            gameObject.SetActive(false); // Deactivate the current trigger if needed
            Debug.Log("Next frame activated via FrameTrigger.");
        }
        else
        {
            Debug.LogError("Next frame is not assigned in FrameTrigger!");
        }
    }
}
