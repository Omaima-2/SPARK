using UnityEngine;

public class FrameTrigger : MonoBehaviour
{
    public bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger detected by: {other.name}");

        if (other.CompareTag("Player")) // Ensure the player has the correct tag
        {
            isTriggered = true;
            Debug.Log("✅ Player triggered the frame transition. isTriggered = " + isTriggered);
        }
    }

    private void Update()
    {
        if (isTriggered)
        {
            Debug.Log("🔵 isTriggered is TRUE in Update()");
        }
    }
}
