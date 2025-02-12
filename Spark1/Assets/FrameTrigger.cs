using UnityEngine;

public class FrameTrigger : MonoBehaviour
{
    public bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger detected by: {other.name}");
        if (other.CompareTag("Player")) // Check if it's the Boy character
        {
            isTriggered = true;
            Debug.Log("Player reached the trigger point for frame2.");
        }
    }

}
