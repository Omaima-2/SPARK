using UnityEngine;

public class Walking1State : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        path1 pathScript = animator.GetComponent<path1>();
        if (pathScript != null)
        {
            pathScript.enabled = true;  // Enable path1 script
            pathScript.StartPath();     // Call a function to start movement
        }
        else
        {
            Debug.LogError("path1 script not found on the GameObject!");
        }
    }
}
