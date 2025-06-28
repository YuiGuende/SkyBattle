////using UnityEngine;

////public class GridCell : MonoBehaviour
////{
////    private int gridX;
////    private int gridY;
////    private GridManager gridManager;

////    public GameObject missilePrefab;

////    public void Initialize(int x, int y, GridManager manager)
////    {
////        gridX = x;
////        gridY = y;
////        gridManager = manager;
////    }

////    void OnMouseDown()
////    {
////        Debug.Log("trang thai game:"+gridManager.isGameStarted);
////        if (gridManager == null || !gridManager.isGameStarted)
////            return;

////        // Không cho bắn vào sân của chính mình
////        if (TurnManager.Instance.GetCurrentTurn() == gridManager.gridOwner)
////        {
////            Debug.Log("🚫 Bạn không thể bắn vào sân của mình.");
////            return;
////        }

////        // Kiểm tra nếu ô đã bị bắn → bỏ qua
////        if (gridManager.HasBeenShot(gridX, gridY))
////        {
////            Debug.Log("⚠️ Ô này đã bị bắn rồi.");
////            return;
////        }

////        // Thực hiện bắn
////        HitType result = gridManager.ShootAt(gridX, gridY);
////        LaunchMissile();

////        switch (result)
////        {
////            case HitType.HeadHit:
////                Debug.Log("🎯 HEAD SHOT!");
////                break;
////            case HitType.BodyHit:
////                Debug.Log("💥 BODY HIT!");
////                break;
////            case HitType.Miss:
////                Debug.Log("💨 MISS!");
////                break;
////        }

////        // ✅ Chỉ đổi lượt nếu bắn hợp lệ
////        TurnManager.Instance.EndTurn();
////    }



////    //    void LaunchMissile()
////    //{
////    //    if (missilePrefab == null) return;

////    //    // Launch from screen bottom center (adjust if needed)
////    //    Vector3 launchPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0, 10));
////    //    launchPos.z = 0;

////    //    GameObject missile = Instantiate(missilePrefab, launchPos, Quaternion.identity);
////    //    missile.GetComponent<missile_animation>().targetPosition = transform.position;

////    //    TurnManager.Instance.EndTurn();
////    //    }
////    void LaunchMissile()
////    {
////        if (missilePrefab == null) return;

////        Vector3 launchPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0, 10));
////        launchPos.z = 0;

////        GameObject missile = Instantiate(missilePrefab, launchPos, Quaternion.identity);
////        missile.GetComponent<missile_animation>().targetPosition = transform.position;
////    }


////}
//using UnityEngine;

//public class GridCell : MonoBehaviour
//{
//    private int gridX;
//    private int gridY;
//    private GridManager gridManager;

//    public void Initialize(int x, int y, GridManager manager)
//    {
//        gridX = x;
//        gridY = y;
//        gridManager = manager;
//    }

//    void OnMouseDown()
//    {
//        if (!gridManager.isGameStarted)
//            return;

//        // Chỉ cho local player điều khiển
//        NetworkPlayer local = TurnManager.Instance.GetComponent<TurnManager>().localPlayer;
//        if (local == null) return;

//        local.CmdRequestShoot(gridX, gridY); // gửi yêu cầu bắn
//    }
//}
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
        Debug.Log("onmouse down được nhận để bắn");

        if (!gridManager.isGameStarted) return;

        NetworkPlayer local = TurnManager.Instance.localPlayer;
        if (local == null)
        {
            Debug.LogError("❌ localPlayer null");
            return;
        }

        Debug.Log($"local.isLocalPlayer = {local.isLocalPlayer}, hasAuthority = {local.isOwned}");

        Debug.Log("chuẩn bị bắn");

        NetworkPlayer player = TurnManager.Instance.localPlayer;
        //Debug.Log("skill duoc chon de cbi kich hoat: " + player.selectedSkill.name);
        Debug.Log("player: " + player.name);
        if (player != null && player.selectedSkill != null)
        {
           
            Debug.Log($"💥 Thi triển kỹ năng {player.selectedSkill.skillType} tại ô ({gridX},{gridY})");
            player.CmdUseSkillOnCell(player.selectedSkill.skillType, gridX, gridY);
            player.selectedSkill = null; // reset kỹ năng sau khi dùng
        }
        else
        {
            local.CmdRequestShoot(gridX, gridY);
        }
            
        Debug.Log("Bắn xong");
    }

}

