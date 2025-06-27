using UnityEngine;
using Fusion;
public class missile_animation : MonoBehaviour
{
    public Vector3 targetPosition;
    public float speed = 10f;
    //public GameObject explosionPrefab;

    private Vector3 direction;

    void Start()
    {
        Vector3 fixedZ = transform.position;
        fixedZ.z = -1f;
        transform.position = fixedZ;

        direction = (targetPosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle-90);
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            //if (explosionPrefab != null)
            //targetPosition.z = -1f;
            //    Instantiate(explosionPrefab, targetPosition, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}

