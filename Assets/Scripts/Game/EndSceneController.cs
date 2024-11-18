using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EndSceneController : NetworkBehaviour
{
    public static EndSceneController instance;

    public GameObject endScreenUI;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void StartEndScene()
    {
        UpdateUI_Rpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateUI_Rpc()
    {
        endScreenUI.GetComponent<EndScreen>().UpdateScreen(GameController.instance.playerWonId.Value, GameController.instance.playerLostId.Value);
        Destroy(GameController.instance.gameObject);
        SteamManager.instance.Disconnect();
    }
}
