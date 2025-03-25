using UnityEngine;

public class FrameTrigger : MonoBehaviour
{
    public bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"🚶 Trigger detected by: {other.name}");

        if (other.CompareTag("Player")) // Ensure the player has the correct tag
        {
            isTriggered = true;
            Debug.Log("✅ Frame Trigger ACTIVATED! isTriggered = " + isTriggered);
        }
    }

}
