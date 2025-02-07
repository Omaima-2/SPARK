using UnityEngine;
using System.Collections;



public class path0 : MonoBehaviour
{
    [SerializeField] private Transform[] Points; // Array of points
    [SerializeField] private float moveSpeed = 1f; // Movement speed
    [SerializeField] private float rotationSpeed = 1f; // Speed of rotation
    [SerializeField] private Animator animator; // Reference to the Animator
    private int pointsIndex;
    private bool isWalking = false; // To check if walking animation is triggered

    void Start()
    {
        // Ensure the array has elements to avoid out-of-bounds errors
        if (Points != null && Points.Length > 0)
        {
            Quaternion targetRotation = Quaternion.Euler(0, 25.5f, 0); // Target rotation



            transform.position = Points[pointsIndex].position; // Set initial position
            StartCoroutine(StartWalkingAfterDelay(5f)); // Start walking after 5 seconds
        }
    }

    void Update()
    {
        if (isWalking && pointsIndex < Points.Length)
        {
            // Get the current target position
            Vector3 targetPosition = Points[pointsIndex].position;

            // Move the character toward the target position
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            // Calculate the direction to the target
            Vector3 direction = (targetPosition - transform.position).normalized;

            // Rotate the character toward the target direction
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            // Check if the object has reached the current point
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                pointsIndex++; // Move to the next point

                // If reached the last point, trigger "Idle" animation
                if (pointsIndex >= Points.Length)
                {
                    animator.SetTrigger("Idle");
            

                }
            }
        }
    }

    private IEnumerator StartWalkingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetTrigger("Walk");
        isWalking = true; // Start movement after animation trigger
    }
}
