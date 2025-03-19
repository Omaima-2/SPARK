using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Required for Button component

public class PanelVisibilityController : MonoBehaviour
{
    public GameObject panel1; // Assign in Unity Inspector
    public GameObject panel2; // Assign in Unity Inspector
    public FrameTrigger frame2Trigger; // Reference to Frame 2 trigger
    public FrameTrigger frame3Trigger; // Reference to Frame 3 trigger
    public FrameTrigger frame4Trigger; // Reference to Frame 4 trigger

    void Start()
    {
        // Hide the panels initially
        if (panel1 != null) panel1.SetActive(false);
        if (panel2 != null) panel2.SetActive(false);

        // Start coroutine to wait for Frame 2 trigger
        StartCoroutine(CheckFrameTriggers());
    }

    IEnumerator CheckFrameTriggers()
    {
        Debug.Log("⏳ Waiting for Frame 2 trigger...");

        // Wait until frame2Trigger is triggered
        yield return new WaitUntil(() => frame2Trigger != null && frame2Trigger.isTriggered);

        Debug.Log("✅ Frame 2 triggered! Showing panels...");

        // Show panels when Frame 2 is triggered
        if (panel1 != null) panel1.SetActive(true);
        if (panel2 != null) panel2.SetActive(true);

        // Wait until frame3Trigger or frame4Trigger is triggered
        yield return new WaitUntil(() =>
            (frame3Trigger != null && frame3Trigger.isTriggered) ||
            (frame4Trigger != null && frame4Trigger.isTriggered)
        );

        Debug.Log("❌ Frame 3 or Frame 4 triggered! Hiding all panels...");
        HidePanels();
    }

    public void HidePanels()
    {
        if (panel1 != null) panel1.SetActive(false);
        if (panel2 != null) panel2.SetActive(false);
    }
}

