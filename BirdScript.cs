using UnityEngine;


public class BirdScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public new Rigidbody2D rigidbody2D;
    public float flapStrength;
    void Start()
    {

    }

    // Update is called once per frame
    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {


    //        rigidbody2D.linearVelocity = Vector2.up * flapStrength;
    //    }
    //}
    private Vector3 offset;
    private bool isDragging = false;

    void OnMouseDown()
    {
        // Tính độ lệch giữa object và vị trí chuột
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mousePos.x, mousePos.y, transform.position.z);
        isDragging = true;
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePos.x, mousePos.y, transform.position.z) + offset;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
    }
}
