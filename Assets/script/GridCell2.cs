using UnityEngine;

public class GridCell2 : MonoBehaviour
{
    private int gridX;
    private int gridY;
    private GridManager2 gridManager;

    public GameObject missilePrefab;

    public void Initialize(int x, int y, GridManager2 manager)
    {
        gridX = x;
        gridY = y;
        gridManager = manager;
    }

    void OnMouseDown()
    {
        Debug.Log("✅ GridCell2 click");
        if (gridManager == null || !gridManager.isGameStarted)
        {
            Debug.Log("❌ GridManager null hoặc chưa start");
            return;
        }

        if (gridManager == null || !gridManager.isGameStarted)
            return;

        // Không cho bắn vào sân của chính mình
        if (TurnManager.Instance.GetCurrentTurn() == gridManager.gridOwner)
        {
            Debug.Log("🚫 Bạn không thể bắn vào sân của mình.");
            return;
        }

        // Kiểm tra nếu ô đã bị bắn → bỏ qua
        if (gridManager.HasBeenShot(gridX, gridY))
        {
            Debug.Log("⚠️ Ô này đã bị bắn rồi.");
            return;
        }

        // Thực hiện bắn
        HitType result = gridManager.ShootAt(gridX, gridY);
        LaunchMissile();

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

        // ✅ Chỉ đổi lượt khi hết lượt bắn
        if (gridManager.shotsRemaining <= 0)
        {
            TurnManager.Instance.EndTurn();
        }
    }



    //    void LaunchMissile()
    //{
    //    if (missilePrefab == null) return;

    //    // Launch from screen bottom center (adjust if needed)
    //    Vector3 launchPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0, 10));
    //    launchPos.z = 0;

    //    GameObject missile = Instantiate(missilePrefab, launchPos, Quaternion.identity);
    //    missile.GetComponent<missile_animation>().targetPosition = transform.position;

    //    TurnManager.Instance.EndTurn();
    //    }
    void LaunchMissile()
    {
        if (missilePrefab == null) return;

        Vector3 launchPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0, 10));
        launchPos.z = 0;

        GameObject missile = Instantiate(missilePrefab, launchPos, Quaternion.identity);
        missile.GetComponent<missile_animation>().targetPosition = transform.position;
    }


}