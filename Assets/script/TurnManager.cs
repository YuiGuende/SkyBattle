using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public List<NetworkPlayer> players = new List<NetworkPlayer>();
    public NetworkPlayer localPlayer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public NetworkPlayer GetCurrentPlayer()
    {
        if (NetworkGameManager.Instance == null) return null;
        int index = NetworkGameManager.Instance.turnIndex;
        if (index >= 0 && index < players.Count)
            return players[index];
        return null;
    }

    public void SetCurrentTurn(int index)
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].RpcSetupTurn(i == index);
        }
    }
}
