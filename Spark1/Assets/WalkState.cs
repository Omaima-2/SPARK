using UnityEngine;

public class WalkState : StateMachineBehaviour
{
    public Transform[] points; // Waypoints array to check the final point
    private int lastPointIndex;
    private Transform characterTransform;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        characterTransform = animator.transform;
        if (points != null && points.Length > 0)
        {
            lastPointIndex = points.Length - 1;
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (points != null && points.Length > 0)
        {
            Transform lastPoint = points[lastPointIndex];
            // Check if the character is close enough to the last point
            if (Vector3.Distance(characterTransform.position, lastPoint.position) < 0.1f)
            {
                animator.SetTrigger("Idle");
            }
        }
    }
}
