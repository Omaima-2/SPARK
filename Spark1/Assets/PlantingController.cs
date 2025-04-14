using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlantingController : MonoBehaviour
{
    public List<GameObject> seeds;
    public GameObject handGuideUI;
    public Camera plantingCamera;
    public AudioClip plantingSound;
    public AudioSource completionAudio; // Assign your audio clip in the Inspector


    public Animator environmentAnimator;
    public string enterPathStateName = "EnterPath2";

    private int currentSeedIndex = 0;
    private bool handGestureHidden = false;
    private AudioSource audioSource;
    private bool finalActionTriggered = false;
    private bool cameraSwitched = false;

    void Start()
    {
        if (seeds == null || seeds.Count == 0) Debug.LogError("❌ ERROR: Seeds list is empty!");
        if (handGuideUI == null) Debug.LogWarning("⚠️ WARNING: Hand Guide UI is not assigned.");
        if (plantingCamera == null) Debug.LogWarning("⚠️ WARNING: Planting Camera is not assigned.");
        if (plantingSound == null) Debug.LogWarning("⚠️ WARNING: Planting sound is not assigned!");

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        foreach (GameObject seed in seeds)
        {
            if (seed != null)
                seed.SetActive(false);
            else
                Debug.LogError("❌ ERROR: One of the seeds is NULL!");
        }

        if (plantingCamera == null)
        {
            plantingCamera = Camera.main;
            if (plantingCamera == null)
                Debug.LogError("❌ ERROR: No camera found!");
            else
                Debug.Log("✅ Found main camera as fallback.");
        }
    }

    void Update()
    {
        MonitorAnimationState();

        if (plantingCamera == null)
        {
            Debug.LogError("❌ ERROR: Planting Camera is NULL!");
            return;
        }

        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
        {
            Ray ray = Input.touchCount > 0 ?
                plantingCamera.ScreenPointToRay(Input.GetTouch(0).position) :
                plantingCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    ActivateNextSeed();
                }
            }
        }
    }

    void MonitorAnimationState()
    {
        if (!cameraSwitched && environmentAnimator != null)
        {
            AnimatorStateInfo stateInfo = environmentAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName(enterPathStateName))
            {
                cameraSwitched = true;
                if (plantingCamera != null)
                    plantingCamera.enabled = true;

                if (handGuideUI != null)
                    handGuideUI.SetActive(true);

                Debug.Log("🎥 Camera enabled and hand gesture shown on 'EnterPath2'");
            }
        }
    }

    void ActivateNextSeed()
    {
        if (!handGestureHidden)
        {
            HideHandGesture();
            handGestureHidden = true;
        }

        if (currentSeedIndex < seeds.Count)
        {
            if (seeds[currentSeedIndex] != null)
            {
                seeds[currentSeedIndex].SetActive(true);
                PlayPlantingSound();
                Debug.Log("✅ Activated Seed: " + seeds[currentSeedIndex].name);
                currentSeedIndex++;

                if (currentSeedIndex == seeds.Count && !finalActionTriggered)
                {
                    finalActionTriggered = true;
                    StartCoroutine(PlaySuccessSequence());
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No more seeds to activate!");
        }
    }

    void HideHandGesture()
    {
        if (handGuideUI != null)
        {
            Animator handAnimator = handGuideUI.GetComponent<Animator>();
            if (handAnimator != null) handAnimator.enabled = false;
            handGuideUI.SetActive(false);
        }
    }

    void PlayPlantingSound()
    {
        if (audioSource != null && plantingSound != null)
        {
            audioSource.PlayOneShot(plantingSound);
        }
    }

    IEnumerator PlaySuccessSequence()
    {
        Debug.Log("⏳ Waiting before playing success sound...");
       

        if (completionAudio != null  && completionAudio.clip != null)
{
   audioSource.PlayOneShot(completionAudio.clip); // safer than .Play()
        Debug.Log("🏆 Played completion audio.");
        yield return new WaitForSeconds(completionAudio.clip.length); // wait until it's done
}
else
{
    Debug.LogWarning("⚠️ Completion Audio not assigned!");
}


     //   yield return new WaitForSeconds(3f); // Delay to finish sound

        if (environmentAnimator != null)
        {
            environmentAnimator.SetTrigger("activityDone");
            Debug.Log("🎬 Triggered 'activityDone' in Animator.");
        }
    }
}