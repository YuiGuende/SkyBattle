using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class ReadyButtonHandler : MonoBehaviour
{
    public Button readyButton;

    void Start()
    {
        readyButton.onClick.AddListener(OnReadyClicked);
    }

    void OnReadyClicked()
    {
        Debug.Log("🟢 Player clicked ready");
        NetworkPlayer player = NetworkClient.connection.identity.GetComponent<NetworkPlayer>();
        if (player != null)
        {
            player.CmdSetReady();
            readyButton.interactable = false;
        }
    }
}
