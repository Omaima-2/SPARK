using System.Collections;
using UnityEngine;

public class PanelVisibilityController : MonoBehaviour
{
    public GameObject panel1; // Assign in Unity Inspector
    public GameObject panel2; // Assign in Unity Inspector
    public FrameTrigger frame2Trigger; // Reference to Frame 2 trigger

    void Start()
    {
        // Hide the panels initially
        if (panel1 != null) panel1.SetActive(false);
        if (panel2 != null) panel2.SetActive(false);

        // Start coroutine to wait for Frame 2 trigger
        StartCoroutine(CheckFrame2Trigger());
    }

    IEnumerator CheckFrame2Trigger()
    {
        Debug.Log("⏳ Waiting for Frame 2 trigger...");

        // Wait until frame2Trigger is triggered
        yield return new WaitUntil(() => frame2Trigger != null && frame2Trigger.isTriggered);

        Debug.Log("✅ Frame 2 triggered! Showing panels...");

        // Show panels when Frame 2 is triggered
        if (panel1 != null) panel1.SetActive(true);
        if (panel2 != null) panel2.SetActive(true);
    }
}
