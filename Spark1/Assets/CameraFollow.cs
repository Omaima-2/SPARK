using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The character to follow
    public Vector3 offset = new Vector3(0, 3, -6); // Fixed position behind the character
    public float smoothSpeed = 5f; // Camera follow speed
    private Quaternion fixedRotation; // Store initial rotation of the camera

    void Start()
    {
        // Store the camera's initial rotation at the start
        fixedRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Update position but keep the original rotation
            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // Lock the camera rotation so it doesn’t change when the character rotates
            transform.rotation = fixedRotation;
        }
    }
}
