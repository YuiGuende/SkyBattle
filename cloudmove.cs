using UnityEngine;

public class cloudmove : MonoBehaviour
{
    public float ms = 5;
    public float deadzone = -46;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position +(Vector3.left * ms) *Time.deltaTime;
        if (transform.position.x < deadzone)
        {
            Destroy(gameObject);
        }
    }
}
