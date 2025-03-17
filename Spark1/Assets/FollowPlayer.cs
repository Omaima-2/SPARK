using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform target; // Assign the girl's Transform in Inspector
    public Vector3 offset = new Vector3(0, 3, -5); // Adjust as needed
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.LookAt(target); // Optional: Makes the camera always look at the girl
        }
    }
}
