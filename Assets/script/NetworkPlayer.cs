using Mirror;
using UnityEngine;
using System.Collections;

public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar] public bool isReady = false;
    [SyncVar] public bool isMyTurn = false;

    [SyncVar(hook = nameof(OnGridAssigned))]
    public NetworkIdentity myGridIdentity;

    [SyncVar]
    public NetworkIdentity enemyGridIdentity;

    public GridManager myGrid;
    public GridManager enemyGrid;

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
    public void CmdSetReady()
    {
        isReady = true;
        NetworkGameManager.Instance.TryStartGame();
    }

    [Command]
    public void CmdRequestShoot(int x, int y)
    {
        Debug.Log(" CmdRequestShoot: called!");

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

        HitType result = enemy.ShootAt(x, y);
        RpcShowResult(x, y, result);
        if (enemy.AreAllPlanesDestroyed())
        {
            Debug.Log("🎯 TẤT CẢ MÁY BAY ĐÃ BỊ TIÊU DIỆT!");

            // Thông báo client thắng và dừng game
            RpcGameOver(true);
        }
        else
            StartCoroutine(EndTurnAfterDelay());
        
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




    IEnumerator EndTurnAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        NetworkGameManager.Instance.NextTurn();
    }

    [ClientRpc]
    void RpcShowResult(int x, int y, HitType result)
    {
        Debug.Log($"[{netId}] Shot result at ({x},{y}) = {result}");

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
        Debug.Log($"[NetworkPlayer] RpcSetupTurn, isMyTurn: {isMyTurn}");

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
