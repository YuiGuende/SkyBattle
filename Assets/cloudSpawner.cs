using UnityEngine;

public class cloudSpawner : MonoBehaviour
{
    public GameObject cloud;
    public float spawnRate = 4;
    private float spawnTime = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawn();
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnTime < spawnRate)
        {
            spawnTime = spawnTime + Time.deltaTime;
        }
        else
        {
            spawn();
            spawnTime = 0;
        }
    }

    public void spawn()
    {
        Instantiate(cloud, transform.position, transform.rotation);
    }
}
