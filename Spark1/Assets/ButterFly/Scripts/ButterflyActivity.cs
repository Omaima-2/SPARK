using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace ButterFly
{
    public class ButterflyActivity : MonoBehaviour
    {
        public GameObject body;
        public Transform leftWing;
        public Transform rightWing;
        public Transform[] flowers; // The three flowers
        public GameObject[] nectarObjects; // The 9 nectar objects
        public GameObject handGesture;
        private Dictionary<Transform, List<GameObject>> nectarGroups = new Dictionary<Transform, List<GameObject>>();

        public float flySpeed = 3f;
        public float rotationSpeed = 5f;
        public float flutterSpeed = 2f;
        public float wingSpeed = 3f;
        public float landingOffset = 0.5f;

        private bool isFlyingToFlower = false;
        private bool firstTapOccurred = false;
        private bool isSwitchingScene = false;

        public Animator environmentAnimator;
        public AudioSource completionAudio;

        private AudioSource audioSource; // ✅ Internal source for playback

        private void Start()
        {
            Debug.Log("🔄 ButterflyActivity script started.");

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            // Validate flowers and nectar objects
            if (flowers == null || flowers.Length != 3)
            {
                Debug.LogError("❌ ERROR: You must assign exactly 3 flowers in the Inspector!");
                return;
            }

            if (nectarObjects == null || nectarObjects.Length != 9)
            {
                Debug.LogError("❌ ERROR: You must assign exactly 9 nectar objects in the Inspector!");
                return;
            }

            for (int i = 0; i < flowers.Length; i++)
            {
                nectarGroups[flowers[i]] = new List<GameObject>();
                for (int j = 0; j < 3; j++)
                {
                    int nectarIndex = (i * 3) + j;
                    nectarGroups[flowers[i]].Add(nectarObjects[nectarIndex]);
                }
            }

            if (handGesture != null)
            {
                handGesture.SetActive(true);
            }
        }

        private void Update()
        {
            if (!isFlyingToFlower)
            {
                FlutterEffect();
            }

            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                if (!firstTapOccurred && handGesture != null)
                {
                    handGesture.SetActive(false);
                    firstTapOccurred = true;
                }
                HandleClickOrTap();
            }
        }

        private void HandleClickOrTap()
        {
            Camera path1Cam = GameObject.FindGameObjectWithTag("Path1cam")?.GetComponent<Camera>();
            if (path1Cam == null)
            {
                Debug.LogError("❌ No camera found with tag 'Path1Cam'!");
                return;
            }

            Ray ray = path1Cam.ScreenPointToRay(Input.mousePosition);
            int layerMask = LayerMask.GetMask("Flowers");
            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);

            if (hits.Length > 0)
            {
                foreach (RaycastHit hit in hits)
                {
                    Transform hitTransform = hit.transform;
                    Debug.Log($"🎯 Hit flower: {hitTransform.name}");

                    foreach (Transform flower in flowers)
                    {
                        if (hitTransform == flower && nectarGroups.ContainsKey(flower))
                        {
                            StartCoroutine(FlyToFlower(flower));
                            return;
                        }
                    }
                }
            }
            else
            {
                Debug.Log("🌐 Click missed the flowers.");
            }
        }

        private IEnumerator FlyToFlower(Transform flower)
        {
            isFlyingToFlower = true;
            Debug.Log("🦋 Flying to: " + flower.name);

            Vector3 landingPosition = flower.position;
            landingPosition.y += landingOffset;

            while (Vector3.Distance(body.transform.position, landingPosition) > 0.02f)
            {
                body.transform.position = Vector3.MoveTowards(body.transform.position, landingPosition, flySpeed * Time.deltaTime);
                body.transform.rotation = Quaternion.Slerp(body.transform.rotation, Quaternion.LookRotation(flower.position - body.transform.position), rotationSpeed * Time.deltaTime);
                yield return null;
            }

            body.transform.position = landingPosition;
            CollectNectar(flower);
            CheckAllNectarCollected();
            isFlyingToFlower = false;
        }

        private void CollectNectar(Transform flower)
        {
            if (nectarGroups[flower].Count > 0)
            {
                GameObject nectar = nectarGroups[flower][0];
                nectar.SetActive(false);
                nectarGroups[flower].RemoveAt(0);
                Debug.Log($"🍯 Nectar collected from {flower.name}, remaining: {nectarGroups[flower].Count}");
            }
        }

        private void CheckAllNectarCollected()
        {
            bool allCollected = true;
            foreach (var group in nectarGroups)
            {
                if (group.Value.Count > 0)
                {
                    allCollected = false;
                    break;
                }
            }

            if (allCollected && !isSwitchingScene)
            {
                Debug.Log("🏆 All nectar collected! Preparing to trigger success...");
                isSwitchingScene = true;
                StartCoroutine(PlaySoundAndTrigger());
            }
        }

        private void FlutterEffect()
        {
            float wingFlapAngle = Mathf.Sin(Time.time * wingSpeed) * 30f;
            leftWing.localRotation = Quaternion.Euler(0f, 0f, wingFlapAngle);
            rightWing.localRotation = Quaternion.Euler(0f, 0f, -wingFlapAngle);
        }

        private IEnumerator PlaySoundAndTrigger()
        {
            Debug.Log("▶️ PlaySoundAndTrigger called...");

            if (completionAudio != null && completionAudio.clip != null)
            {
                audioSource.PlayOneShot(completionAudio.clip);
                Debug.Log($"🏆 Played completion audio (Length: {completionAudio.clip.length}s)");
                yield return new WaitForSeconds(completionAudio.clip.length);
            }
            else
            {
                Debug.LogWarning("⚠️ Completion audio not assigned or has no clip!");
                yield return new WaitForSeconds(3f); // fallback wait
            }

            if (environmentAnimator != null)
            {
                environmentAnimator.SetTrigger("activityDone");
                Debug.Log("🎬 Triggered 'activityDone' in Animator.");
            }
            else
            {
                Debug.LogWarning("⚠️ Animator reference not set!");
            }
        }
    }
}
