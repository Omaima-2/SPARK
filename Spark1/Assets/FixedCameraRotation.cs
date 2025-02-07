using UnityEngine;

public class FixedCameraRotation : MonoBehaviour
{
    [SerializeField] private Transform boyRoot; // Reference to the Boy's root transform
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -10); // Offset from the Boy's position

    private Vector3 initialCameraRotation; // Camera's initial rotation in local space

    void Start()
    {
        // Store the initial local rotation of the camera
        initialCameraRotation = transform.localEulerAngles;
    }

    void LateUpdate()
    {
        if (boyRoot != null)
        {
            // Follow the Boy's position with an offset
            transform.position = boyRoot.position + offset;

            // Keep the camera's local rotation fixed
            transform.localEulerAngles = initialCameraRotation;
        }
    }
}
