using UnityEngine;

public class StateController : MonoBehaviour
{
    public Transform[] points; // Assign these in the Inspector
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Pass the points to the WalkState
        foreach (var behaviour in animator.GetBehaviours<WalkState>())
        {
            behaviour.points = points; // Assign the points array
        }
    }
}
