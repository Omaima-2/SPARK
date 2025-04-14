using UnityEngine;

public class idleState : StateMachineBehaviour
{
    public Vector3 cameraShift = new Vector3(-2f, 0, 0); // Shift left by 2 units
    private Camera mainCam;
    private Vector3 originalCamPosition;

    // Called when the Animator enters the state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // ✅ Rotate the character (your original line)
        animator.gameObject.transform.rotation = Quaternion.Euler(0, 300, 0);

        // ✅ Only shift the camera if the state is Idle 2
        if (stateInfo.IsName("Idle 2"))
        {
            mainCam = Camera.main;
            if (mainCam != null)
            {
                // Save original camera position to restore later if needed
                originalCamPosition = mainCam.transform.position;

                // Shift the camera to the left
                mainCam.transform.position += cameraShift;
         
           }
        }
        if (stateInfo.IsName("afterActivity"))
           {
             GameManager.Instance.ActivateEnvironmentCam();
           }

         if (stateInfo.IsName("EnterPath1"))
        {
        GameManager.Instance.ActivatePath1Cam();

        }
    }

    // Optional: Reset camera position when exiting the state
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.IsName("Idle 2") && mainCam != null)
        {
            mainCam.transform.position = originalCamPosition;
        }
       
    }
}