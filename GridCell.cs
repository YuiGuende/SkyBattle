using UnityEngine;

public class GridCell : MonoBehaviour
{
    private int gridX;
    private int gridY;
    private GridManager gridManager;

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
}