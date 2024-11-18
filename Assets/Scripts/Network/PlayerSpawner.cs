using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField]
    private GameObject player;

    private bool isStarted = false;

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadScene;
    }

    private void OnLoadScene(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (isStarted)
        {
            if(IsHost && sceneName == SceneManager.GetSceneByBuildIndex(3).name)
            {
                if (EndSceneController.instance != null)
                {
                    EndSceneController.instance.StartEndScene();

                    isStarted = false;
                }
            }

            return;
        }

        if (IsHost && sceneName == SceneManager.GetSceneByBuildIndex(2).name)
        {
            foreach (ulong id in clientsCompleted)
            {
                GameObject p = Instantiate(player);
                p.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
                p.GetComponent<PlayerController>().CreatePlayer_ServerRpc(Deck.CreateDemoDeck());
                StartCoroutine(AddPlayer(p));
            }

            if (SteamManager.instance.singlePlayer)
            {
                GameObject ai = Instantiate(player);
                ai.GetComponent<NetworkObject>().SpawnAsPlayerObject(0, true);
                ai.GetComponent<PlayerController>().CreateAI_ServerRpc();
                StartCoroutine(AddAI(ai));
            }
        }

        isStarted = true;
    }

    IEnumerator AddPlayer(GameObject p) {
        yield return new WaitUntil(() => GameController.instance != null);
        GameController.instance.AddPlayer_ServerRpc(p);
    }

    IEnumerator AddAI(GameObject ai)
    {
        yield return new WaitUntil(() => GameController.instance != null);
        GameController.instance.AddPlayer_ServerRpc(ai);
    }
}
