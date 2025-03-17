using UnityEngine;

namespace ButterFly
{ 
    public class ButterflyController : MonoBehaviour
    {
        public GameObject body;
        public Transform leftWing;
        public Transform rightWing;

        [Tooltip("Radius of the flying area")]
        public float flyingAreaRadius = 1.0f;

        [Tooltip("Base radius of the fluttering movements")]
        public float baseFlutteringRadius = 0.1f;

        [Tooltip("Multiplier to adjust fluttering radius")]
        public float flutteringRadiusMultiplier = 0.2f;

        [Tooltip("Total time the butterfly spends flying btween change directions ")]
        public float flyTime = 10f;

        [Tooltip("Butterfly's flying speed")]
        public float flySpeed = 0.2f;

        [Tooltip("Rotation speed of the butterfly")]
        public float rotationSpeed = 10f;

        [Tooltip("Time spent fluttering within the fluttering sphere before direction change")]
        public float flutterTime = 0.3f;

        [Tooltip("Fluttering speed of the butterfly")]
        public float flutterSpeed = 5f;

        [Tooltip("Maximum swing angle for the wings")]
        public float wingMaxSwing = 90f;

        [Tooltip("Minimum swing angle for the wings")]
        public float wingMinSwing = -30f;

        [Tooltip("Speed of wing flapping")]
        public float wingSpeed = 3f;

        private Transform flyingAreaSphere;            // Reference to the flying area sphere
        private Transform flutteringSphere;            // Reference to the fluttering sphere
        private float flyTimer;
        private Vector3 flyTargetPosition;
        private float flutterTimer;
        private Vector3 flutterTargetPosition;

        private void Start()
        {
            // Create the flying area sphere and set its position
            flyingAreaSphere = new GameObject("FlyingAreaSphere").transform;
            flyingAreaSphere.position = transform.position + new Vector3(0, flyingAreaRadius, 0);
            flyingAreaSphere.SetParent(transform);

            // Create the fluttering sphere and set its initial position using the baseFlutteringRadius
            flutteringSphere = new GameObject("FlutteringSphere").transform;
            flutteringSphere.position = GetRandomPointInSphere(baseFlutteringRadius);
            flutteringSphere.SetParent(transform);

            // Initialize random flyTime and flutterTime
            flyTime = Random.Range(flyTime - (flyTime / 10), flyTime + (flyTime / 10));
            flutterTime = Random.Range(flutterTime - (flutterTime / 10), flutterTime + (flutterTime / 10));
            wingSpeed = Random.Range(wingSpeed - (wingSpeed / 10), wingSpeed + (wingSpeed / 10));
        }


        private void Update()
        {
            // Calculate the angle for wing flapping using a sine wave
            float wingFlapAngle = Mathf.Sin(Time.time * wingSpeed) * (wingMaxSwing - wingMinSwing) / 2f + (wingMaxSwing + wingMinSwing) / 2f;

            // Apply the wing rotation
            Quaternion newLeftRotation = Quaternion.Euler(0f, 0f, wingFlapAngle);
            Quaternion newRightRotation = Quaternion.Euler(0f, 0f, -wingFlapAngle);

            leftWing.localRotation = newLeftRotation;
            rightWing.localRotation = newRightRotation;

            if (flyTimer < flyTime)
            {
                flyTimer += Time.deltaTime;
            }
            else
            {
                // Set a new random target position inside the flyingAreaSphere
                flyTargetPosition = GetRandomPointInSphere(baseFlutteringRadius);
                flyTimer = 0f; // Reset the timer
            }

            // Move the flutteringSphere towards the target position using Lerp
            flutteringSphere.position = Vector3.Lerp(flutteringSphere.position, flyTargetPosition, Time.deltaTime * flySpeed);

            // Calculate the rotation to look at the flyTargetPosition
            Vector3 flyDirection = flyTargetPosition - body.transform.position;

            if (flyDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(flyDirection);

                // Smoothly interpolate the rotation
                body.transform.rotation = Quaternion.Lerp(body.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }

            // Flutter inside the flutteringSphere
            if (flutterTimer < flutterTime)
            {
                flutterTimer += Time.deltaTime;
            }
            else
            {
                // Set a new random target position inside the flutteringSphere
                flutterTargetPosition = GetRandomPointInFlutteringSphere();
                flutterTimer = 0f; // Reset the timer
            }

            // Move the butterfly inside the flutteringSphere towards the target position using Lerp
            body.transform.position = Vector3.Lerp(body.transform.position, flutterTargetPosition, Time.deltaTime * flutterSpeed);
        }

        // Generate a random point inside the fluttering sphere
        private Vector3 GetRandomPointInFlutteringSphere()
        {
            // Generate a random point inside the fluttering sphere using spherical coordinates
            float theta = Random.Range(0f, Mathf.PI * 2f); // Random angle
            float phi = Random.Range(0f, Mathf.PI);        // Random inclination angle

            // Convert spherical coordinates to Cartesian coordinates
            float x = Mathf.Sin(phi) * Mathf.Cos(theta);
            float y = Mathf.Sin(phi) * Mathf.Sin(theta);
            float z = Mathf.Cos(phi);

            // Scale by the fluttering radius and lift it to the desired height
            Vector3 randomPoint = new Vector3(x, y, z) * (baseFlutteringRadius * flutteringRadiusMultiplier) + flutteringSphere.position;

            return randomPoint;
        }

        private Vector3 GetRandomPointInSphere(float radius)
        {
            // Generate a random point inside the sphere using spherical coordinates
            float theta = Random.Range(0f, Mathf.PI * 2f); // Random angle
            float phi = Random.Range(0f, Mathf.PI);        // Random inclination angle

            // Convert spherical coordinates to Cartesian coordinates
            float x = Mathf.Sin(phi) * Mathf.Cos(theta);
            float y = Mathf.Sin(phi) * Mathf.Sin(theta);
            float z = Mathf.Cos(phi);

            // Scale by the given radius and lift it to the desired height
            Vector3 randomPoint = new Vector3(x, y, z) * radius + flyingAreaSphere.position;

            return randomPoint;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow; // Set the color for Gizmos
            if (flyingAreaSphere != null)
            {
                Gizmos.DrawWireSphere(flyingAreaSphere.position, flyingAreaRadius); // Draw the flying area sphere using Gizmos
            }

            Gizmos.color = Color.red; // Set the color for Gizmos
            if (flutteringSphere != null)
            {
                // Draw the fluttering sphere using the adjusted radius
                Gizmos.DrawWireSphere(flutteringSphere.position, baseFlutteringRadius * flutteringRadiusMultiplier);
            }
        }
    }
}

