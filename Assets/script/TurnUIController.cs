using UnityEngine;
using TMPro;

public class TurnUIController : MonoBehaviour
{
    public static TurnUIController Instance;
    public TextMeshProUGUI turnText;

    void Awake()
    {
        Instance = this;
    }

    public void SetTurnUI(bool isMyTurn)
    {

        if (turnText != null)
        {
            turnText.text = isMyTurn ? "🎯 Lượt của bạn" : "⏳ Đợi đối thủ...";
        }
    }
}
