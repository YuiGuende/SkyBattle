using Mirror;
using UnityEngine;

public class GridNetworkManager : NetworkManager
{
    public GameObject gridPrefab;
    public GameObject[] mayBayPrefabs;
    public Vector3 gridPosP1;
    public Vector3 gridPosP2;
    public GameObject networkGameManagerPrefab;
    void Start()
    {
        Debug.Log("✅ GridNetworkManager started");
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        if (NetworkGameManager.Instance == null)
        {
            GameObject gm = Instantiate(networkGameManagerPrefab);
            NetworkServer.Spawn(gm); // 🔑 Cho phép SyncVar hoạt động
        }
        int index = numPlayers - 1;
        Vector3 gridPos = (index == 0) ? gridPosP1 : gridPosP2;

        GameObject gridObj = Instantiate(gridPrefab, gridPos, Quaternion.identity);
        NetworkServer.Spawn(gridObj, conn);
        GridManager gridManager = gridObj.GetComponent<GridManager>();

        NetworkPlayer player = conn.identity.GetComponent<NetworkPlayer>();
        player.myGridIdentity = gridObj.GetComponent<NetworkIdentity>();

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

        for (int i = 0; i < mayBayPrefabs.Length; i++)
        {
            Vector3 pos = gridPos + new Vector3(i * 2f, 3f, -0.1f);
            GameObject plane = Instantiate(mayBayPrefabs[i], pos, Quaternion.identity);

            var mb = plane.GetComponent<maybay>();
            mb.gridManager = gridManager;
            mb.gridIdentity = gridObj.GetComponent<NetworkIdentity>(); // 🔑

            NetworkServer.Spawn(plane, conn);
        }
    }
}
