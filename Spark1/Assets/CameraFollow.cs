using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The character to follow
    public Vector3 offset = new Vector3(0, 3, -6); // Fixed offset behind the character
    public float smoothSpeed = 5f; // Speed of camera movement

    private Quaternion fixedRotation; // Store the fixed rotation of the camera

    void Start()
    {
        // Store the camera's initial rotation to keep it fixed
        fixedRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Keep the camera at a fixed world-space offset from the target
            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // Lock the camera's rotation so it does NOT rotate with the character
            transform.rotation = fixedRotation;
        }
    }
}
