using Mirror;
using UnityEngine;

public class GridNetworkManager : NetworkManager
{
    public GameObject gridPrefab;
    public GameObject[] mayBayPrefabs;
    public Vector3 gridPosP1;
    public Vector3 gridPosP2;
    public GameObject networkGameManagerPrefab;
    public GameObject slotMachinePrefab;

    void Start()
    {
        Debug.Log("✅ GridNetworkManager started");
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        // 🧠 Spawn NetworkGameManager nếu chưa có
        if (NetworkGameManager.Instance == null)
        {
            GameObject gm = Instantiate(networkGameManagerPrefab);
            NetworkServer.Spawn(gm);
        }

        // 📍 Vị trí lưới cho người chơi
        int index = numPlayers - 1;
        Vector3 gridPos = (index == 0) ? gridPosP1 : gridPosP2;

        // 🟦 Spawn Grid
        GameObject gridObj = Instantiate(gridPrefab, gridPos, Quaternion.identity);
        NetworkServer.Spawn(gridObj, conn);
        GridManager gridManager = gridObj.GetComponent<GridManager>();

        // 🔗 Gán Grid cho Player
        NetworkPlayer player = conn.identity.GetComponent<NetworkPlayer>();
        player.myGridIdentity = gridObj.GetComponent<NetworkIdentity>();

        // 🔁 Gán enemyGrid khi đủ 2 người
        if (numPlayers == 2)
        {
            var allPlayers = FindObjectsOfType<NetworkPlayer>();
            if (allPlayers.Length == 2)
            {
                NetworkPlayer p1 = allPlayers[0];
                NetworkPlayer p2 = allPlayers[1];
                p1.enemyGridIdentity = p2.myGridIdentity;
                p2.enemyGridIdentity = p1.myGridIdentity;
            }
        }

        // 🎰 Spawn SlotMachine
        Vector3 slotPos = gridPos + new Vector3(0, 3f, 0);
        GameObject slotMachine = Instantiate(slotMachinePrefab, slotPos, Quaternion.identity);
        var sm = slotMachine.GetComponent<slotmachineManager>();

        // Gán thông tin Grid
        sm.assignedGridManager = gridManager;
        sm.assignedGridIdentity = gridObj.GetComponent<NetworkIdentity>();

        NetworkServer.Spawn(slotMachine, conn);

        // 🧠 Gọi TargetRpc để hiển thị slot trên client
        sm.TargetActivateSlotMachine(conn);
    }
}
