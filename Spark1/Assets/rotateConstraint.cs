using UnityEngine;

public class RotateConstraint : MonoBehaviour
{
    [SerializeField] private Animator animator; // Reference to the Animator
    private bool isWalking = false; // Current state of the walking trigger

    // Update is called once per frame
    void Update()
    {
        // Check the animation trigger state
        isWalking = animator.GetBool("isWalking"); // Replace "isWalking" with the exact trigger name in your Animator

        // Set Y-rotation based on isWalking state
        if (isWalking)
        {
            // Set Y-rotation to 25
            transform.rotation = Quaternion.Euler(0, 360, 0);
        }
        else
        {
            // Set Y-rotation to 200
            transform.rotation = Quaternion.Euler(0, 200, 0);
        }
    }
}
