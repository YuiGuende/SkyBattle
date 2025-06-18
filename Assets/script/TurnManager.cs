using UnityEngine;
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public enum PlayerTurn { Player1, Player2 }
    public PlayerTurn currentTurn = PlayerTurn.Player1;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void EndTurn()
    {
        currentTurn = (currentTurn == PlayerTurn.Player1) ? PlayerTurn.Player2 : PlayerTurn.Player1;
        Debug.Log("Turn chuyển sang: " + currentTurn);
    }

    public PlayerTurn GetCurrentTurn()
    {
        return currentTurn;
    }
}
