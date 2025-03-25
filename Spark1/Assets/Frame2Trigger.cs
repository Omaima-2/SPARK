using UnityEngine;

public class Frame2Trigger : MonoBehaviour
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

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"🚪 Exit detected by: {other.name}");

        if (other.CompareTag("Player")) // Ensure only the player can deactivate it
        {
            isTriggered = false;
            Debug.Log("❌ Frame Trigger DEACTIVATED! isTriggered = " + isTriggered);
        }
    }
}
