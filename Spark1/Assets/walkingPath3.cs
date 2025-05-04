using UnityEngine;

public class walkingPath3 : StateMachineBehaviour
{ override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        path3 pathScript = animator.GetComponent<path3>();
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
