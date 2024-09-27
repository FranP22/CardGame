using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks.Data;
using TMPro;
using System.Threading.Tasks;
using System;
using Unity.Netcode;
using Netcode.Transports.Facepunch;
using UnityEngine.SceneManagement;

public class SteamManager : MonoBehaviour
{
    public static SteamManager instance;

    public Lobby? currentLobby = null;
    public bool singlePlayer = false;

    private FacepunchTransport transport = null;
    //private ulong hostId;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        transport = GetComponent<FacepunchTransport>();

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }

    private void OnDestroy()
    {
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;

        if(NetworkManager.Singleton == null)
        {
            return;
        }
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        //NetworkManager.Singleton.OnClientConnectedCallback -=
        //NetworkManager.Singleton.OnClientDisconnectCallback -=

    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId id)
    {
        RoomEnter joinedLobby = await lobby.Join();
        if(joinedLobby == RoomEnter.Success)
        {
            currentLobby = lobby;
            Debug.Log("Lobby joined");
        }
    }

    private void OnLobbyMemberLeave(Lobby lobby, Friend friend)
    {
        Debug.Log(friend.Name + " Left");
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        Debug.Log(friend.Name + " Joined");
    }

    private void OnLobbyEntered(Lobby lobby)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            return;
        }

        StartClient(currentLobby.Value.Owner.Id);

        Debug.Log("Lobby entered " + currentLobby.Value.Owner.Id + " (" + NetworkManager.Singleton.LocalClientId + ")");
    }

    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if(result != Result.OK)
        {
            return;
        }

        if(!singlePlayer)
        {
            lobby.SetPublic();
            lobby.SetJoinable(true);
        }
        else
        {
            lobby.SetJoinable(false);
        }

        lobby.SetGameServer(lobby.Owner.Id);

        Debug.Log("Lobby created " + lobby.Owner.Id);
    }

    private void OnServerStarted()
    {
        Debug.Log("Host started");
    }

    public async void StartHost()
    {
        singlePlayer = false;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.StartHost();
        GameManager.instance.clientId = NetworkManager.Singleton.LocalClientId;
        currentLobby = await SteamMatchmaking.CreateLobbyAsync(2);
    }

    public async void StartSingleplayer()
    {
        singlePlayer = true;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.StartHost();
        GameManager.instance.clientId = NetworkManager.Singleton.LocalClientId;
        currentLobby = await SteamMatchmaking.CreateLobbyAsync(1);
    }

    public bool StartClient(SteamId id)
    {
        //NetworkManager.Singleton.OnClientConnectedCallback +=
        //NetworkManager.Singleton.OnClientDisconnectCallback +=
        transport.targetSteamId = id;
        GameManager.instance.clientId = NetworkManager.Singleton.LocalClientId;
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Client started");
            return true;
        }

        return false;
    }

    public void Disconnect() 
    {
        currentLobby?.Leave();
        if(NetworkManager.Singleton == null)
        {
            return;
        }
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        }
        else
        {
            //NetworkManager.Singleton.OnClientConnectedCallback -=
            //NetworkManager.Singleton.OnClientDisconnectCallback -=
        }
        NetworkManager.Singleton.Shutdown(true);
        Debug.Log("Disconnected");
    }

    /*public async void HostLobby()
    {
        await SteamMatchmaking.CreateLobbyAsync(2);
    }

    public async Task<Lobby[]> FindAllLobbies(bool empty = true)
    {
        Lobby[] lobbies;
        if (empty)
        {
            lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();
        }
        else
        {
            lobbies = await SteamMatchmaking.LobbyList.RequestAsync();
        }

        return lobbies;
    }*/

    public async Task<bool> JoinLobbyWithID(ulong id)
    {
        Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();

        foreach (Lobby lobby in lobbies)
        {
            if (lobby.Id == id)
            {
                Debug.Log("Room Found");

                currentLobby = lobby;
                RoomEnter room = await lobby.Join();

                if (room == RoomEnter.Success)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /*public void LeaveLobby()
    {
        LobbyManager.instance.currentLobby?.Leave();
        LobbyManager.instance.currentLobby = null;
        NetworkManager.Singleton.Shutdown();
    }*/

    public void StartGameServer()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log(currentLobby?.MemberCount);
            if(!singlePlayer) 
                if (currentLobby?.MemberCount < 2) return;

            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }
    }
}
