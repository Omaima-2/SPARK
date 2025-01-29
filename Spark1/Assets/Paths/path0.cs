using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class path0 : MonoBehaviour
{
    [SerializeField] Transform[] Points; // Array of points
    [SerializeField] private float moveSpeed; // Movement speed
    private int pointsIndex;

    void Start()
    {
        // Ensure the array has elements to avoid out-of-bounds errors
        if (Points != null && Points.Length > 0)
        {
            transform.position = Points[pointsIndex].position; // Set initial position
        }
    }

    void Update()
    {
        if (pointsIndex <= Points.Length - 1)
        {
            // Move towards the current point using Vector3.MoveTowards
            transform.position = Vector3.MoveTowards(
                transform.position,
                Points[pointsIndex].position,
                moveSpeed * Time.deltaTime
            );

            // Check if the object has reached the current point
            if (Vector3.Distance(transform.position, Points[pointsIndex].position) < 0.1f)
            {
                pointsIndex += 1; // Move to the next point
            }
        }
    }
}
