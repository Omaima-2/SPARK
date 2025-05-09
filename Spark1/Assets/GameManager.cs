using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Camera environmentCam;
    public Camera path1Cam;
    public Camera path2Cam;

    // UI Elements from the Canvas
    public GameObject dialog, mute, stopStory, homeButton, next, previous;
    public GameObject handGuide1;
    public GameObject handGuide2;

    // ✅ Fade Panel Reference
    public CanvasGroup fadeGroup;

    // ✅ GameObjects for path-specific elements
    public GameObject Flower, Flower1, Flower2, Flower3;
    public GameObject Soil, Soil0;

    private void Awake()
    {
        SwitchToEnvironment();
 
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {

       string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
         if (sceneName == "Start")  // Adjust name to match your Start scene
            {
                Destroy(gameObject);
                return;
            }

      

        // Deactivate all special objects at startup
        SetFlowersActive(false);
        SetSoilActive(false);
       


        ActivateEnvironmentCam(); // Start on Environment
    }

    public void ActivateEnvironmentCam()
    {
        StartCoroutine(FadeAndSwitchCamera("Environment"));
    }

    public void ActivatePath1Cam()
    {
        StartCoroutine(FadeAndSwitchCamera("Path1"));
    }

    public void ActivatePath2Cam()
    {
        StartCoroutine(FadeAndSwitchCamera("Path2"));
    }

    private IEnumerator FadeAndSwitchCamera(string camTarget)
    {
        yield return StartCoroutine(Fade(1)); // Fade to black

        switch (camTarget)
        {
            case "Path1":
                SwitchToPath1();
                break;
            case "Path2":
                SwitchToPath2();
                break;
            case "Environment":
            default:
                SwitchToEnvironment();
                break;
        }

        yield return StartCoroutine(Fade(0)); // Fade to clear
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float duration = 2f;
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

    private void SwitchToEnvironment()
    {
        environmentCam.gameObject.SetActive(true);
        path1Cam.gameObject.SetActive(false);
        path2Cam.gameObject.SetActive(false);

        dialog.SetActive(true);
        mute.SetActive(true);
        stopStory.SetActive(true);
        homeButton.SetActive(true);
        next.SetActive(true);
        previous.SetActive(true);
        //HandTaping1.SetActive(false);
        //HandTaping2.SetActive(false);
        handGuide2.SetActive(false);
        handGuide1.SetActive(false);

        SetFlowersActive(false);
        SetSoilActive(false);
    }

    private void SwitchToPath1()
    {
        environmentCam.gameObject.SetActive(false);
        path1Cam.gameObject.SetActive(true);
        path2Cam.gameObject.SetActive(false);

        dialog.SetActive(false);
        mute.SetActive(false);
        stopStory.SetActive(false);
        homeButton.SetActive(false);
        next.SetActive(false);
        previous.SetActive(false);
        //HandTaping1.SetActive(true);
        //HandTaping2.SetActive(false);
        handGuide1.SetActive(true);

        SetFlowersActive(true);
        SetSoilActive(false);
    }

    private void SwitchToPath2()
    {
        environmentCam.gameObject.SetActive(false);
        path1Cam.gameObject.SetActive(false);
        path2Cam.gameObject.SetActive(true);

        dialog.SetActive(false);
        mute.SetActive(false);
        stopStory.SetActive(false);
        homeButton.SetActive(false);
        next.SetActive(false);
        previous.SetActive(false);
        //HandTaping1.SetActive(false);
        //HandTaping2.SetActive(true);
        handGuide2.SetActive(true);

        //SetFlowersActive(false);
        SetSoilActive(true);
    }

    // ✅ Helper functions for object activation
    private void SetFlowersActive(bool state)
    {
        if (Flower != null) Flower.SetActive(state);
        if (Flower1 != null) Flower1.SetActive(state);
        if (Flower2 != null) Flower2.SetActive(state);
        if (Flower3 != null) Flower3.SetActive(state);
    }

    private void SetSoilActive(bool state)
    {
        if (Soil != null) Soil.SetActive(state);

        if(state){
            if (Soil0 != null) Soil0.SetActive(false);
        }else{
            if (Soil0 != null) Soil0.SetActive(true);
        }
    }
    


}
