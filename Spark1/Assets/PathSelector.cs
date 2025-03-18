using UnityEngine;

public class PathSelector : MonoBehaviour
{
    public Animator animator;

    // Called when the player chooses Path1
    public void SelectPath1()
    {
        animator.SetTrigger("path1");
    }

    // Called when the player chooses Path2
    public void SelectPath2()
    {
        animator.SetTrigger("path2");
    }
}
