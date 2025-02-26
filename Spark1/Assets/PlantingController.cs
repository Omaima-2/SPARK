using UnityEngine;
using System.Collections.Generic;

public class PlantingController : MonoBehaviour
{
    public List<GameObject> seeds; // Assign seed GameObjects in the Inspector
    public GameObject handGuideUI; // Assign the UI hand guide (to hide it after the first tap)
    public Camera plantingCamera; // Assign the Camera in the Inspector
    public AudioClip plantingSound; // Assign the sound effect in the Inspector

    private int currentSeedIndex = 0; // Keeps track of the next seed to activate
    private bool handGestureHidden = false; // Tracks if the hand gesture has been hidden
    private AudioSource audioSource; // Audio source for playing the sound

    void Start()
    {
        // Debug check for missing references
        if (seeds == null || seeds.Count == 0) Debug.LogError("❌ ERROR: Seeds list is empty!");
        if (handGuideUI == null) Debug.LogWarning("⚠️ WARNING: Hand Guide UI is not assigned.");
        if (plantingCamera == null) Debug.LogWarning("⚠️ WARNING: Planting Camera is not assigned.");
        if (plantingSound == null) Debug.LogWarning("⚠️ WARNING: Planting sound is not assigned!");

        // Get or Add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>(); // Adds an AudioSource if missing
        }

        // Ensure all seeds are inactive at the start
        foreach (GameObject seed in seeds)
        {
            if (seed != null)
                seed.SetActive(false);
            else
                Debug.LogError("❌ ERROR: One of the seeds in the list is NULL!");
        }

        // Attempt to find the Camera if it's not assigned
        if (plantingCamera == null)
        {
            plantingCamera = Camera.main; // Use main camera as fallback
            if (plantingCamera == null)
            {
                Debug.LogError("❌ ERROR: No camera assigned and no Main Camera found!");
            }
            else
            {
                Debug.Log("✅ Found main camera as fallback.");
            }
        }
    }

    void Update()
    {
        if (plantingCamera == null)
        {
            Debug.LogError("❌ ERROR: Planting Camera is NULL! Cannot perform raycast.");
            return;
        }

        // Check for touch input (mobile) or mouse click (PC testing)
        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
        {
            Ray ray;
            if (Input.touchCount > 0) // Mobile Touch
            {
                ray = plantingCamera.ScreenPointToRay(Input.GetTouch(0).position);
            }
            else // Mouse Click
            {
                ray = plantingCamera.ScreenPointToRay(Input.mousePosition);
            }

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null)
                {
                    Debug.Log("✅ Clicked on: " + hit.collider.gameObject.name);
                    if (hit.collider.gameObject == gameObject) // Ensure it's the soil
                    {
                        ActivateNextSeed();
                    }
                }
                else
                {
                    Debug.LogWarning("⚠️ Raycast hit nothing!");
                }
            }
        }
    }

    void ActivateNextSeed()
    {
        if (!handGestureHidden) // Hide the hand gesture on first tap
        {
            HideHandGesture();
            handGestureHidden = true;
        }

        if (currentSeedIndex < seeds.Count) // Check if there are seeds left to activate
        {
            if (seeds[currentSeedIndex] != null)
            {
                seeds[currentSeedIndex].SetActive(true); // Activate the next seed
                Debug.Log("✅ Activated Seed: " + seeds[currentSeedIndex].name);

                // Play planting sound
                PlayPlantingSound();

                currentSeedIndex++; // Move to the next seed
            }
            else
            {
                Debug.LogError("❌ ERROR: Seed at index " + currentSeedIndex + " is NULL!");
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
            if (handAnimator != null)
            {
                Debug.Log("✅ Disabling Hand Animator...");
                handAnimator.enabled = false; // Stop animation
            }
            else
            {
                Debug.LogError("❌ ERROR: Animator not found on handGuideUI!");
            }

            Debug.Log("🔄 Hiding hand gesture UI...");
            handGuideUI.SetActive(false); // Hide the UI
        }
        else
        {
            Debug.LogWarning("⚠️ Hand Guide UI was already null before hiding.");
        }
    }

    void PlayPlantingSound()
    {
        if (audioSource != null && plantingSound != null)
        {
            audioSource.PlayOneShot(plantingSound);
            Debug.Log("🔊 Played planting sound!");
        }
        else
        {
            Debug.LogWarning("⚠️ Planting sound or AudioSource is missing!");
        }
    }
}
