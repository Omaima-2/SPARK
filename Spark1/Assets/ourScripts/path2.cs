using UnityEngine;
using System.Collections;

public class path2 : MonoBehaviour
{
    [SerializeField] private Transform[] Points; // Array of waypoints
    [SerializeField] private float moveSpeed = 1f; // Movement speed
    [SerializeField] private float rotationSpeed = 2f; // Speed of rotation during movement
    [SerializeField] private Animator animator; // Character Animator

    private int pointsIndex;
    private bool isWalking = false; // To check if walking animation is triggered

    public void Start()
    {
        if (Points != null && Points.Length > 0)
        {
            transform.position = Points[pointsIndex].position; // Set initial position
            StartCoroutine(StartWalkingAfterDelay(5f)); // Start walking after 5 seconds
        }
    }

    public void Update()
    {
        if (isWalking && pointsIndex < Points.Length)
        {
            MoveToNextPoint();
        }

        // If character enters Idle state, set fixed rotation
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            SetFixedIdleRotation();
        }
    }

    public void MoveToNextPoint()
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

    private void SetFixedIdleRotation()
    {
        // Set the character's rotation to (0, 57, 0) when Idle
        transform.rotation = Quaternion.Euler(0, 57, 0);
    }

    private IEnumerator StartWalkingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetTrigger("Walk"); // Trigger Walk animation
        isWalking = true; // Start movement after animation trigger
    }
}
