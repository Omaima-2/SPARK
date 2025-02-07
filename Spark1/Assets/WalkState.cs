using UnityEngine;

public class WalkState : StateMachineBehaviour
{
    public Transform[] points; // Array of points to move along
    public float moveSpeed = 1f; // Movement speed
    private int pointsIndex = 0;
    private Transform characterTransform;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Reference the character's transform
        characterTransform = animator.transform;

        // Make sure we start at the first point
        if (points != null && points.Length > 0)
        {
            characterTransform.position = points[0].position;
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (points != null && pointsIndex < points.Length)
        {
            // Move the character toward the current target point
            Vector3 targetPosition = points[pointsIndex].position;
            characterTransform.position = Vector3.MoveTowards(
                characterTransform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            // Check if the target point is reached
            if (Vector3.Distance(characterTransform.position, targetPosition) < 0.1f)
            {
                pointsIndex++; // Move to the next point
            }
        }
        else
        {
            // If all points are reached, trigger the transition to Idle
            animator.SetTrigger("Idle");
        }
    }
}
