using UnityEngine;
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public enum PlayerTurn { Player1, Player2 }
    public PlayerTurn currentTurn = PlayerTurn.Player1;

    public GameObject gridPlayer1;
    public GameObject gridPlayer2;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateGridVisibility();
    }

    public void EndTurn()
    {
        currentTurn = (currentTurn == PlayerTurn.Player1) ? PlayerTurn.Player2 : PlayerTurn.Player1;
        Debug.Log("Lượt hiện tại: " + currentTurn);
        UpdateGridVisibility();

        if (currentTurn == PlayerTurn.Player1)
        {
            // ẩn UI player2
            var p2Grid = gridPlayer2.GetComponentInChildren<GridManager2>(true);
            p2Grid?.HideUI();

            // bắt đầu lượt player1
            var p1Grid = gridPlayer1.GetComponentInChildren<GridManager>(true);
            p1Grid?.StartTurn();
        }
        else
        {
            var p1Grid = gridPlayer1.GetComponentInChildren<GridManager>(true);
            p1Grid?.HideUI();

            var p2Grid = gridPlayer2.GetComponentInChildren<GridManager2>(true);
            Debug.Log("p2Grid: " + p2Grid);
            p2Grid?.StartTurn();
        }

    }

    void UpdateGridVisibility()
    {
        // Hiện sân của người bị bắn (đối thủ)
        if (currentTurn == PlayerTurn.Player1)
        {
            gridPlayer1.SetActive(false); // ẩn sân Player1
            gridPlayer2.SetActive(true);  // hiện sân Player2 để Player1 bắn
        }
        else
        {
            gridPlayer1.SetActive(true);  // hiện sân Player1 để Player2 bắn
            gridPlayer2.SetActive(false); // ẩn sân Player2
        }
    }

    public PlayerTurn GetCurrentTurn()
    {
        return currentTurn;
    }
}

