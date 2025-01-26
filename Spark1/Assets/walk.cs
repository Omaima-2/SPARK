using UnityEngine;

public class Walk : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();  
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.angularVelocity = Vector3.zero;
        rb.MovePosition(transform.position + speed * Time.deltaTime * transform.forward);
    }
}
