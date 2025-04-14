using UnityEngine;

public class idleState1 : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Ensure the GameObject is rotated to Y = 35 when entering idle state
        animator.gameObject.transform.rotation = Quaternion.Euler(0, 270, 0);


          if (stateInfo.IsName("afterActivity1"))
           {
             GameManager.Instance.ActivateEnvironmentCam();
           }

         if (stateInfo.IsName("EnterPath2"))
        {
        GameManager.Instance.ActivatePath2Cam();

        }
        
    }
    

    // You can use OnStateExit if needed to reset or change the rotation when leaving the idle state
}