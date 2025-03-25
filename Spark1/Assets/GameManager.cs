using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Camera environmentCam;
    public Camera path1Cam;

    // UI Elements from the Canvas
    public GameObject dialog, mute, stopStory, homeButton, next, previous, HandTaping;

    // ✅ Fade Panel Reference
    public CanvasGroup fadeGroup;  // Assign the CanvasGroup of your black panel here

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ActivateEnvironmentCam();  // Start on Environment
    }

    // ✅ CAMERA SWITCH ENTRY POINTS (Call these instead of direct switch)
    public void ActivateEnvironmentCam()
    {
        StartCoroutine(FadeAndSwitchCamera(false));  // false = to Environment
    }

    public void ActivatePath1Cam()
    {
        StartCoroutine(FadeAndSwitchCamera(true));   // true = to Path1
    }

    // ✅ STEP 3 - Fade and Camera Switch Coroutine
    private IEnumerator FadeAndSwitchCamera(bool toPath1)
    {
        // Fade to black
        yield return StartCoroutine(Fade(1));

        // Switch cameras
        if (toPath1)
            SwitchToPath1();
        else
            SwitchToEnvironment();

        // Fade back to clear
        yield return StartCoroutine(Fade(0));
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float duration = 1f; // 1 second fade duration
        float startAlpha = fadeGroup.alpha;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }
        fadeGroup.alpha = targetAlpha;
    }

    // ✅ Separate logic for camera and UI switching
    private void SwitchToEnvironment()
    {
        environmentCam.gameObject.SetActive(true);
        path1Cam.gameObject.SetActive(false);

        dialog.SetActive(true);
        mute.SetActive(true);
        stopStory.SetActive(true);
        homeButton.SetActive(true);
        next.SetActive(true);
        previous.SetActive(true);
        HandTaping.SetActive(false);
    }

    private void SwitchToPath1()
    {
        environmentCam.gameObject.SetActive(false);
        path1Cam.gameObject.SetActive(true);

        dialog.SetActive(false);
        mute.SetActive(false);
        stopStory.SetActive(false);
        homeButton.SetActive(false);
        next.SetActive(false);
        previous.SetActive(false);
        HandTaping.SetActive(true);
    }
}
