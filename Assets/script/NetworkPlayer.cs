using Mirror;
using UnityEngine;
using System.Collections;

public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar] public bool isReady = false;
    [SyncVar] public bool isMyTurn = false;
    private bool isShooting = false;
    public int shootRemaining=1;
    public PlaneSkill selectedSkill;

    [SyncVar(hook = nameof(OnGridAssigned))]
    public NetworkIdentity myGridIdentity;

    [SyncVar]
    public NetworkIdentity enemyGridIdentity;

    public GridManager myGrid;
    public GridManager enemyGrid;

    public ManaNotificationUI manaNotification;
    public ManaNotificationUI enemyManaNotification;

    public override void OnStartLocalPlayer()
    {
        TurnManager.Instance.localPlayer = this;
        Debug.Log($"[LocalPlayer] isServer = {isServer}, isClient = {isClient}, isLocalPlayer = {isLocalPlayer}");

        if (myGridIdentity != null)
        {
            Debug.Log("Manual call to OnGridAssigned from OnStartLocalPlayer");
            OnGridAssigned(null, myGridIdentity);
        }
    }

    void OnGridAssigned(NetworkIdentity oldGrid, NetworkIdentity newGrid)
    {
        Debug.Log("OnGridAssigned is called");
        if (newGrid != null)
        {
            myGrid = newGrid.GetComponent<GridManager>();

            if (isLocalPlayer)
            {
                StartCoroutine(HideOtherGridsAfterGridAssigned());
            }
        }

        if (enemyGridIdentity != null)
        {
            enemyGrid = enemyGridIdentity.GetComponent<GridManager>();
        }
    }

    IEnumerator HideOtherGridsAfterGridAssigned()
    {
        yield return null;

        foreach (var grid in FindObjectsOfType<GridManager>())
        {
            bool isMine = (grid == myGrid);
            SetGridVisible(grid, isMine);
            Debug.Log($"[LocalPlayer] Checking grid {grid.name} isMine = {isMine}");
        }

        Debug.Log("✅ [LocalPlayer] Grid visibility applied.");
    }

    void SetGridVisible(GridManager grid, bool visible)
    {
        Debug.Log("set grid visible is called ,Is it will be visible " + visible);
        foreach (Transform child in grid.transform)
        {
            var sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.enabled = visible;
        }
    }

    [Command]
    public void CmdUseSkillOnCell(PlaneSkillType skillType, int x, int y)
    {
        Debug.Log($"[Server] Dùng skill {skillType} tại ({x},{y})");

        switch (skillType)
        {
            case PlaneSkillType.FighterBoost:
                CmdRequestFighterBoost(x, y);   
                break;
            case PlaneSkillType.RadarScan:
                CmdRequestBOMBDUYMOM(x, y);    
                break;
            case PlaneSkillType.AreaBomb:
                
                break;
            default:
                Debug.LogWarning("Skill chưa xử lý: " + skillType);
                break;
        }
    }
    [Command]
    public void CmdRequestBOMBDUYMOM(int x, int y)
    {
        if (isShooting)
        {
            Debug.Log("⛔ Đang xử lý bắn, chờ tí...");
            return;
        }

        if (!NetworkGameManager.Instance.gameStarted)
        {
            Debug.Log("⛔ Game chưa bắt đầu.");
            return;
        }

        if (!isMyTurn)
        {
            Debug.Log("⛔ Không phải lượt của bạn.");
            return;
        }

        GridManager enemy = enemyGridIdentity != null ? enemyGridIdentity.GetComponent<GridManager>() : null;
        if (enemy == null)
        {
            Debug.LogError("❌ CmdRequestBOMBDUYMOM: enemyGridIdentity null hoặc không có GridManager!");
            return;
        }

        isShooting = true;

        HitType result = enemy.ShootAt(x, y);
        RpcShowResult(x, y, result);

        if (result == HitType.Shooted)
        {
            Debug.Log("⛔ Ô đã bắn trước đó.");
            isShooting = false;
            return;
        }

        if (enemy.AreAllPlanesDestroyed())
        {
            Debug.Log("🎯 TẤT CẢ MÁY BAY ĐÃ BỊ TIÊU DIỆT!");
            RpcGameOver(true);
            isShooting = false;
            return;
        }

        shootRemaining--; // Dù đúng hay sai vẫn trừ lượt

        if (shootRemaining <= 0)
        {
            StartCoroutine(EndTurnAfterDelay());
        }
        else
        {
            isShooting = false;
        }
    }



    [Command]
    public void CmdRequestFighterBoost(int x, int y)
    {
        if (isShooting)
        {
            Debug.Log("⛔ Đang xử lý bắn, chờ tí...");
            return;
        }

        if (!NetworkGameManager.Instance.gameStarted)
        {
            Debug.Log("game is not started");
            return;
        }

        if (!isMyTurn)
        {
            Debug.Log("Không phải lượt của bạn");
            return;
        }

        GridManager enemy = enemyGridIdentity != null ? enemyGridIdentity.GetComponent<GridManager>() : null;
        if (enemy == null)
        {
            Debug.LogError("❌ CmdRequestShoot: enemyGridIdentity is null or has no GridManager!");
            return;
        }

        isShooting = true; // ✅ Khóa lại

        HitType result = enemy.ShootAt(x, y);
        RpcShowResult(x, y, result);

        if (result == HitType.Shooted)
        {
            Debug.Log("⛔ Ô đã bắn → không đổi lượt, không reset khóa vì chưa bắn hợp lệ.");
            isShooting = false;
            return;
        }

        if (enemy.AreAllPlanesDestroyed())
        {
            Debug.Log("🎯 TẤT CẢ MÁY BAY ĐÃ BỊ TIÊU DIỆT!");
            RpcGameOver(true);
            isShooting = false;
            return;
        }

        if (shootRemaining<=0)
        {
            // ❌ Miss → kết thúc lượt
            StartCoroutine(EndTurnAfterDelay());
        }
        else
        {
            // ✅ Hit → cho phép bắn tiếp
            isShooting = false;
            shootRemaining--;
        }

    }


    [Command]
    public void CmdSetReady()
    {
        isReady = true;
        NetworkGameManager.Instance.TryStartGame();
    }

    [Command]
    public void CmdSetSkill(PlaneSkill skillLinked)
    {
        Debug.Log("skilllimnk" + skillLinked.name);
        selectedSkill = skillLinked;

    }

    [Command]
    public void CmdRequestShoot(int x, int y)
    {
        if (isShooting)
        {
            Debug.Log("⛔ Đang xử lý bắn, chờ tí...");
            return;
        }

        if (!NetworkGameManager.Instance.gameStarted)
        {
            Debug.Log("game is not started");
            return;
        }

        if (!isMyTurn)
        {
            Debug.Log("Không phải lượt của bạn");
            return;
        }

        GridManager enemy = enemyGridIdentity != null ? enemyGridIdentity.GetComponent<GridManager>() : null;
        if (enemy == null)
        {
            Debug.LogError("❌ CmdRequestShoot: enemyGridIdentity is null or has no GridManager!");
            return;
        }

        isShooting = true; // ✅ Khóa lại

        HitType result = enemy.ShootAt(x, y);
        RpcShowResult(x, y, result);

        if (result == HitType.Shooted)
        {
            Debug.Log("⛔ Ô đã bắn → không đổi lượt, không reset khóa vì chưa bắn hợp lệ.");
            isShooting = false;
            return;
        }

        if (enemy.AreAllPlanesDestroyed())
        {
            Debug.Log("🎯 TẤT CẢ MÁY BAY ĐÃ BỊ TIÊU DIỆT!");
            RpcGameOver(true);
            isShooting = false;
            return;
        }

        if (result == HitType.Miss )
        {
            // ❌ Miss → kết thúc lượt
            StartCoroutine(EndTurnAfterDelay());
        }
        else
        {
            // ✅ Hit → cho phép bắn tiếp
            isShooting = false;
     
        }
        
    }



    [ClientRpc]
    void RpcGameOver(bool youWin)
    {
        if (!isLocalPlayer) return;

        string message = youWin ? "🏆 Bạn đã thắng!" : "💥 Bạn đã thua!";
        Debug.Log(message);

        // Hiện toàn bộ máy bay
        foreach (var p in FindObjectsOfType<maybay>())
        {
            if (p.gridManager == enemyGrid) // Chỉ hiện máy bay của đối thủ
            {
                var sr = p.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) sr.enabled = true;
            }
        }


        // Hiển thị kết quả
        if (myGrid.notification != null)
        {
            myGrid.notification.text = message;
        }

        // Ngăn người chơi bắn thêm
        enabled = false;
    }



    [TargetRpc]
    public void TargetSetManaUI(NetworkConnection target, NetworkIdentity manaObj)
    {
        Debug.Log("✅ TargetSetManaUI called");
        manaNotification = manaObj.GetComponent<ManaNotificationUI>();
    }
    IEnumerator EndTurnAfterDelay()
    {
        Debug.Log("end turn");
        yield return new WaitForSeconds(3f);
        isShooting=false; 


        foreach (var plane in FindObjectsOfType<maybay>())
        {
            if (plane.gridManager == myGrid) // chỉ máy bay của mình
            {
                PlaneSkill skill = plane.GetComponent<PlaneSkill>();
                if (skill != null)
                {
                    Debug.Log("skill cooldown done");
                    skill.ReduceCooldown();
                }
            }
        }

        if (manaNotification != null && manaNotification.mana < manaNotification.maxMana)
        {
            manaNotification.mana += 1;
            manaNotification.UpdateVisual();
        }
        if (enemyManaNotification != null && enemyManaNotification.mana < enemyManaNotification.maxMana)
        {
            enemyManaNotification.mana += 1;
            enemyManaNotification.UpdateVisual();
        }


        // Cập nhật UI cả 2 bên
        manaNotification.UpdateVisual();
        enemyManaNotification.UpdateVisual(); // nếu muốn xem mana của đối phương
        
        NetworkGameManager.Instance.NextTurn();
    }

    [TargetRpc]
    public void TargetMoveManaUI(NetworkConnection target, Vector3 offset)
    {
        if (manaNotification != null)
            manaNotification.transform.position += offset;
    }

    [ClientRpc]
    void RpcShowResult(int x, int y, HitType result)
    {
        Debug.Log($"[{netId}] Shot result at ({x},{y}) = {result}");
        if (result == HitType.Shooted) return;

        //if (enemyGrid == null)
        //{
        //    Debug.LogWarning("⚠️ RpcShowResult: enemyGrid is null");
        //    return;
        //}
        if (enemyGrid == null)
        {
            enemyGrid = FindEnemyGrid(); // 🆕 thêm hàm này bên dưới
            if (enemyGrid == null)
            {
                Debug.LogWarning("⚠️ RpcShowResult: enemyGrid is still null");
                return;
            }
        }
        enemyGrid.ApplyHitVisual(x, y, result); 
        Vector3 target = enemyGrid.GetCell(x, y).transform.position;
        Vector3 start = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1.1f, 0));
        start.z = 0;

        GameObject missile = Instantiate(myGrid.missilePrefab, start, Quaternion.identity);
        missile.GetComponent<missile_animation>().targetPosition = target;
    }

    [ClientRpc]
    public void RpcSetupTurn(bool isMyTurn)
    {
        if (isMyTurn && isLocalPlayer)
        {
            shootRemaining = 1;
        }

        Debug.Log($"[NetworkPlayer] RpcSetupTurn, isMyTurn: {isMyTurn}");
        shootRemaining = 1;
        if (!isLocalPlayer) return;

        foreach (var g in FindObjectsOfType<GridManager>())
        {
            bool isMyGrid = g == myGrid;
            bool shouldShow = isMyTurn ? !isMyGrid : isMyGrid;

            g.SetGridVisible(shouldShow);
            Debug.Log($"[RpcSetupTurn] Grid: {g.name}, isMyGrid={isMyGrid}, shouldShow={shouldShow}");
        }

        foreach (var plane in FindObjectsOfType<maybay>())
        {
            if (plane.gridManager == null) continue;

            bool isMyPlane = (plane.gridManager == myGrid);

            // 🔄 Lấy SpriteRenderer ở child thay vì chính object
            SpriteRenderer sr = plane.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = isMyTurn ? !isMyPlane : isMyPlane;
            }
            else
            {
                Debug.LogWarning($"⚠️ maybay '{plane.name}' is missing a SpriteRenderer in children!");
            }
        }

    }
    [Command]
    public void CmdConsumeMana(int amount)
    {
        if (manaNotification == null) return;

        manaNotification.mana -= amount;
        if (manaNotification.mana < 0) manaNotification.mana = 0;

        manaNotification.UpdateVisual();
    }

    GridManager FindEnemyGrid()
    {
        foreach (var g in FindObjectsOfType<GridManager>())
        {
            if (g != myGrid)
                return g;
        }
        return null;
    }

}
