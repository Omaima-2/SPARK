using UnityEngine;

public class Walking2State : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        path2 pathScript = animator.GetComponent<path2>();
        if (pathScript != null)
        {
            pathScript.enabled = true;  
            pathScript.StartPath();     
        }
        else
        {
            Debug.LogError("path2 script not found on the GameObject!");
        }
    }
}
