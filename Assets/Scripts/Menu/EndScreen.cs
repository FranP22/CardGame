using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI text;

    public void UpdateScreen(ulong winId, ulong loseId)
    {
        bool hasWon = false;
        ulong localId = NetworkManager.Singleton.LocalClientId;

        Debug.Log(localId + " " + winId);

        if(localId == winId) hasWon = true;

        if (hasWon)
        {
            text.text = "Victory";
        }
        else
        {
            text.text = "Defeat";
        }
    }

    public void ExitGame()
    {
        SceneManager.LoadScene(1);
    }
}
