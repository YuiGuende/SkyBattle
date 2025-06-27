using Mirror;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance;

    [SyncVar] public bool gameStarted = false;
    [SyncVar] public int turnIndex = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    public override void OnStartLocalPlayer()
    {
        Debug.Log($"[LocalPlayer] NetworkGameManager.Instance = {NetworkGameManager.Instance}");
    }

    [Server]
    public void TryStartGame()
    {
        var players = FindObjectsOfType<NetworkPlayer>();
        if (players.Length < 2) return;

        if (players.All(p => p.isReady))
        {
            gameStarted = true;
            Debug.Log("✅ Game started!");

            RpcStartGame();
            RpcDisablePlaneColliders();

            StartCoroutine(SetupFirstTurn(players));
        }
    }

    IEnumerator SetupFirstTurn(NetworkPlayer[] players)
    {
        yield return new WaitForSeconds(1f); // Nhẹ delay tránh lỗi khởi tạo

        players[0].isMyTurn = true;
        players[1].isMyTurn = false;

        players[0].RpcSetupTurn(true);
        players[1].RpcSetupTurn(false);
    }

    [ClientRpc]
    void RpcStartGame()
    {
        Debug.Log($"🎮 RpcStartGame called on client: isServer={isServer}, isClient={isClient}, isLocalPlayer={(TurnManager.Instance?.localPlayer)}");

        foreach (var obj in NetworkClient.spawned.Values)
        {
            GridManager g = obj.GetComponent<GridManager>();
            if (g != null)
            {
                Debug.Log($"[GridManager] StartGame from Rpc: {g.gameObject.name}");
                g.StartGame();
            }
        }

        Debug.Log("🎮 All players' grids started!");
    }

    [Server]
    public void NextTurn()
    {
        var players = FindObjectsOfType<NetworkPlayer>();
        if (players.Length != 2) return;

        players[0].isMyTurn = !players[0].isMyTurn;
        players[1].isMyTurn = !players[1].isMyTurn;

        players[0].RpcSetupTurn(players[0].isMyTurn);
        players[1].RpcSetupTurn(players[1].isMyTurn);
    }

    [ClientRpc]
    void RpcDisablePlaneColliders()
    {
        foreach (var plane in FindObjectsOfType<maybay>())
        {
            var col = plane.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;
            if (plane.isOwned)
            {
                plane.CmdSetFinalPlanePosition(plane.transform.position);
            }
        }

        Debug.Log("✈️ Plane colliders disabled after ready.");
    }
}
