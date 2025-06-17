using UnityEngine;

public class GridCell : MonoBehaviour
{
    private int gridX;
    private int gridY;
    private GridManager gridManager;

    public GameObject missilePrefab;

    public void Initialize(int x, int y, GridManager manager)
    {
        gridX = x;
        gridY = y;
        gridManager = manager;
    }



    void OnMouseDown()
    {
        if (gridManager != null)
        {
            HitType result = gridManager.ShootAt(gridX, gridY);
            LaunchMissile(); // << Call missile launch here
            // Có thể thêm sound effects hoặc animations ở đây
            switch (result)
            {
                case HitType.HeadHit:
                    Debug.Log("🎯 HEAD SHOT!");
                    break;
                case HitType.BodyHit:
                    Debug.Log("💥 BODY HIT!");
                    break;
                case HitType.Miss:
                    Debug.Log("💨 MISS!");
                    break;
            }
        }
    }


    void LaunchMissile()
{
    if (missilePrefab == null) return;

    // Launch from screen bottom center (adjust if needed)
    Vector3 launchPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0, 10));
    launchPos.z = 0;

    GameObject missile = Instantiate(missilePrefab, launchPos, Quaternion.identity);
    missile.GetComponent<missile_animation>().targetPosition = transform.position;
}

}