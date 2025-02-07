using UnityEngine;

public class WaveState : StateMachineBehaviour
{

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // This function is called when the Wave state starts
        Debug.Log("Wave state entered");
    }

   /* override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Trigger the Walk state after waving
        animator.SetTrigger("Walk");
    }*/
}

