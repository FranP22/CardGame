using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Steamworks;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject main;

    [SerializeField]
    private GameObject playMenu;
    [SerializeField]
    private GameObject deckEditor;

    [SerializeField]
    private GameObject playMenuOnline;

    [SerializeField]
    private GameObject playLobby;
    [SerializeField]
    private TextMeshProUGUI lobbyId;

    [SerializeField]
    private TextMeshProUGUI player1Text;
    [SerializeField]
    private TextMeshProUGUI player2Text;

    [SerializeField]
    private GameObject joinLobby;
    [SerializeField]
    private TextMeshProUGUI lobbyIdInput;

    private GameObject currentMenu;

    private void Start()
    {
        currentMenu = main;
    }

    private void Update()
    {
        if(currentMenu == playLobby && SteamManager.instance.currentLobby != null)
        {
            lobbyId.text = SteamManager.instance.currentLobby?.Id.ToString();

            if(SteamManager.instance.singlePlayer == true)
            {
                player1Text.text = SteamManager.instance.currentLobby.Value.Owner.Name;
                player2Text.text = "AI";
            }

            /*IEnumerable<Friend> members = LobbyManager.instance.currentLobby?.Members;

            Friend[] f = members.ToArray();

            if(f.Length == 0) {
                player1Text.text = "";
                player2Text.text = "";
            }
            else if(f.Length == 1)
            {
                player1Text.text = f[0].Name;
                player2Text.text = "";
            }
            else
            {
                player1Text.text = f[0].Name;
                player2Text.text = f[1].Name;
            }*/
        }
    }

    public void OpenMainMenu()
    {
        ChangeMenu(main);
    }

    public void OpenPlayMenu()
    {
        ChangeMenu(playMenu);
    }

    public void OpenDeckEditor()
    {
        ChangeMenu(deckEditor);
    }

    public void OpenOnline()
    {
        ChangeMenu(playMenuOnline);
    }

    public void OpenLobby(bool multiplayer = false)
    {
        ChangeMenu(playLobby);
    }

    public void OpenJoinLobby()
    {
        ChangeMenu(joinLobby);
    }

    private void ChangeMenu(GameObject menu)
    {
        currentMenu.SetActive(false);
        currentMenu = menu;
        currentMenu.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void CreateMatch()
    {
        SteamManager.instance.StartHost();
        OpenLobby(true);
    }

    public async void JoinMatch()
    {
        ulong ID;
        string text = lobbyIdInput.text;
        text = text.Remove(text.Length - 1);
        bool parsed = ulong.TryParse(text, out ID);

        if (!parsed) return;

        bool result = await SteamManager.instance.JoinLobbyWithID(ID);
        if (result)
        {
            OpenLobby(true);
        }
    }

    public void CreateSingleplayer()
    {
        SteamManager.instance.StartSingleplayer();
        OpenLobby(true);
    }

    public void LeaveMatch()
    {
        SteamManager.instance.Disconnect();
    }

    public void StartGame()
    {
        SteamManager.instance.StartGameServer();
    }

    public void CopyId()
    {
        GUIUtility.systemCopyBuffer = lobbyId.text;
    }
}
