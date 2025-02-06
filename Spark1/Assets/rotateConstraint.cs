using UnityEngine;

public class RotateConstraint : MonoBehaviour
{
    [SerializeField] private Animator animator; // Reference to the Animator
    private bool doneWalking = false; // Current state of the walking trigger

    private void Start()
    {
        // Set default Y-rotation to 200
      //  transform.rotation = Quaternion.Euler(0, 30, 0);
    }

    void Update()
    {
        // Get the current state of the "walk" trigger from Animator
        doneWalking = animator.GetBool("Idle"); 

        // If walking, set Y-rotation to 200; otherwise, set it to 25
        float targetRotationY = doneWalking ? 35f : 200;

        // Only update rotation if it has changed
        if (transform.rotation.eulerAngles.y != targetRotationY)
        {
            transform.rotation = Quaternion.Euler(0, targetRotationY, 0);
        }
    }
}
