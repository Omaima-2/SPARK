using UnityEngine;
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
        public Transform[] flowers;
        public GameObject[] nectarObjects;
        public GameObject handGesture; // Hand gesture icon
        private Dictionary<Transform, List<GameObject>> nectarGroups = new Dictionary<Transform, List<GameObject>>();
        
        public float flySpeed = 3f;
        public float rotationSpeed = 5f;
        public float flutterSpeed = 2f;
        public float wingSpeed = 3f;
        public float landingOffset = 0.2f; // Adjusted landing offset for Y-axis only

        private bool isFlyingToFlower = false;
        private bool firstTapOccurred = false;

        private void Start()
        {
            Debug.Log("🔄 ButterflyActivity script started.");

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

            foreach (Transform flower in flowers)
            {
                AddClickListener(flower);
            }

            if (handGesture != null)
            {
                handGesture.SetActive(true); // Show hand gesture initially
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
                    handGesture.SetActive(false); // Hide hand gesture on first tap
                    firstTapOccurred = true;
                }
                HandleClickOrTap();
            }
        }

        private void HandleClickOrTap()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (nectarGroups.ContainsKey(hit.transform))
                {
                    StartCoroutine(FlyToFlower(hit.transform));
                }
            }
        }

        private IEnumerator FlyToFlower(Transform flower)
        {
            isFlyingToFlower = true;
            Debug.Log("🦋 Flying to: " + flower.name);

            Vector3 landingPosition = flower.position;
            landingPosition.y += landingOffset;

            while (Vector3.Distance(body.transform.position, landingPosition) > 0.02f) // Smaller threshold
            {
                body.transform.position = Vector3.MoveTowards(body.transform.position, landingPosition, flySpeed * Time.deltaTime);
                body.transform.rotation = Quaternion.Slerp(body.transform.rotation, Quaternion.LookRotation(flower.position - body.transform.position), rotationSpeed * Time.deltaTime);
                yield return null;
            }

            body.transform.position = landingPosition;
            CollectNectar(flower);
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

        private void FlutterEffect()
        {
            float wingFlapAngle = Mathf.Sin(Time.time * wingSpeed) * 30f;
            leftWing.localRotation = Quaternion.Euler(0f, 0f, wingFlapAngle);
            rightWing.localRotation = Quaternion.Euler(0f, 0f, -wingFlapAngle);
        }

        private void AddClickListener(Transform flower)
        {
            Collider flowerCollider = flower.GetComponent<Collider>();
            if (flowerCollider == null)
            {
                flowerCollider = flower.gameObject.AddComponent<BoxCollider>();
                Debug.Log("🟢 Added BoxCollider to: " + flower.name);
            }
        }
    }
}
