using UnityEngine;

public class floatInSpace : MonoBehaviour
{

    public bool inSpace;
    bool prev;
    Rigidbody rb;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {   
        inSpace = transform.position.x < -22f;

        // if (inSpace && rb.useGravity)
            // rb.AddForce(Vector3.left * 3f);

        if (inSpace && rb.useGravity)
            rb.useGravity = false;

        if (!inSpace && prev)
            rb.useGravity = true;
        
        prev = inSpace;
    }
}
