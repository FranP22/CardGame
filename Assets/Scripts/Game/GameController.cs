//using System;
//using System.Collections.Generic;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class GameController : NetworkBehaviour
{
    public static GameController instance;

    [HideInInspector]
    public NetworkVariable<bool> arePlayersIn = new NetworkVariable<bool>(false);
    [HideInInspector]
    public NetworkVariable<bool> hasGameStarted = new NetworkVariable<bool>(false);
    [HideInInspector]
    public NetworkVariable<bool> hasGameEnded = new NetworkVariable<bool>(false);

    [HideInInspector]
    public NetworkList<NetworkObjectReference> playerArray;
    [HideInInspector]
    private NetworkVariable<NetworkObjectReference> p1 = new NetworkVariable<NetworkObjectReference>();
    [HideInInspector]
    private NetworkVariable<NetworkObjectReference> p2 = new NetworkVariable<NetworkObjectReference>();

    [HideInInspector]
    private NetworkVariable<NetworkObjectReference> turnPlayer = new NetworkVariable<NetworkObjectReference>();
    [HideInInspector]
    private NetworkVariable<NetworkObjectReference> playerWon = new NetworkVariable<NetworkObjectReference>();


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            playerArray = new NetworkList<NetworkObjectReference>();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        if (!hasGameStarted.Value)
        {
            if (arePlayersIn.Value)
            {
                PlayerController pc1 = ((NetworkObject)p1.Value).GetComponent<PlayerController>();
                PlayerController pc2 = ((NetworkObject)p2.Value).GetComponent<PlayerController>();
                if (pc1.isDeckSet.Value && pc2.isDeckSet.Value)
                {
                    hasGameStarted.Value = true;
                    StartGame_ServerRpc();
                }
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        playerArray.OnListChanged += PlayersChanged;
    }

    private void OnApplicationQuit()
    {
        playerArray.OnListChanged -= PlayersChanged;
    }

    private void PlayersChanged(NetworkListEvent<NetworkObjectReference> changeEvent)
    {
        switch (playerArray.Count)
        {
            case 0:
                return;
            case 1:
                p1.Value = playerArray[0];
                //p2.Value = null;
                break;
            case 2:
                p1.Value = playerArray[0];
                p2.Value = playerArray[1];

                arePlayersIn.Value = true;
                //StartGame_ServerRpc();
                break;
            default:
                p1.Value = playerArray[0];
                p2.Value = playerArray[1];
                break;
        }

        Debug.Log((NetworkObject)p1.Value + " " + (NetworkObject)p2.Value);
    }

    [Rpc(SendTo.Server)]
    private void StartGame_ServerRpc()
    {
        int player = UnityEngine.Random.Range(0, 2);
        turnPlayer.Value = playerArray[player];

        ((NetworkObject)p1.Value).GetComponent<PlayerController>().StartGame_ServerRpc();
        ((NetworkObject)p2.Value).GetComponent<PlayerController>().StartGame_ServerRpc();
    }

    [Rpc(SendTo.Server)]
    public void AddPlayer_ServerRpc(NetworkObjectReference player)
    {
        playerArray.Add(player);
    }

    public bool IsPlayersTurn(NetworkObjectReference player)
    {
        if((NetworkObject)player == (NetworkObject)turnPlayer.Value) return true;
        return false;
    }

    [Rpc(SendTo.Server)]
    public void PlayCard_ServerRpc(NetworkObjectReference player, CardType type, int cardId, int handId = -1)
    {
        PlayerController pc = ((NetworkObject)player).GetComponent<PlayerController>();

        if (!IsPlayersTurn(player) && handId != -1)
        {
            pc.PlayCardCallBack(CardPlaySuccess.NotThePlayersTurn);
            return;
        }

        if ((NetworkObject)player == (NetworkObject)p1.Value)
        {
            GameBoard.instance.PlayCard_ServerRpc(player, type, cardId, 1, handId);
        }
        else if((NetworkObject)player == (NetworkObject)p2.Value)
        {
            GameBoard.instance.PlayCard_ServerRpc(player, type, cardId, 2, handId);
        }
        else
        {
            pc.PlayCardCallBack(CardPlaySuccess.Fail);
        }

        
        return;
    }

    [Rpc(SendTo.Server)]
    public void CardCombat_ServerRpc(NetworkObjectReference player, NetworkObjectReference attacker, NetworkObjectReference defender)
    {
        NetworkObject attackerOb = (NetworkObject)attacker;
        NetworkObject defenderOb = (NetworkObject)defender;

        CardObjectField attackerScript = attackerOb.GetComponent<CardObjectField>();
        CardObjectField defenderScript = defenderOb.GetComponent<CardObjectField>();

        defenderScript.Damage(attacker, attackerScript.card.Value.allyStats.currentAttack);
        attackerScript.Damage(defender, defenderScript.card.Value.allyStats.currentAttack);

        attackerScript.attacksLeft.Value--;
    }

    [Rpc(SendTo.Server)]
    public void EndTurn_ServerRpc(NetworkObjectReference player)
    {
        if(!IsPlayersTurn(player))
        {
            return;
        }

        int turnPlayerId = turnPlayer.Value.Equals(p1.Value) ? 1 : 2;

        List<NetworkObjectReference> units = new List<NetworkObjectReference>();
        units = GameBoard.GetAllUnits(turnPlayerId);

        foreach (NetworkObjectReference unit in units)
        {
            CardObjectField script = ((NetworkObject)unit).GetComponent<CardObjectField>();
            script.StartEffectTrigger(EffectTrigger.OnEndOfTurn);
        }

        //units = new List<NetworkObjectReference>();
        units = GameBoard.GetAllUnitsSpecificField(turnPlayerId == 1 ? 1 : 2);

        foreach (NetworkObjectReference unit in units)
        {
            CardObjectField script = ((NetworkObject)unit).GetComponent<CardObjectField>();
            script.StartEffectTrigger(EffectTrigger.OnEndOfYourTurn);
        }

        //units = new List<NetworkObjectReference>();
        units = GameBoard.GetAllUnitsSpecificField(turnPlayerId == 1 ? 2 : 1);

        foreach (NetworkObjectReference unit in units)
        {
            CardObjectField script = ((NetworkObject)unit).GetComponent<CardObjectField>();
            script.StartEffectTrigger(EffectTrigger.OnEndOfEnemyTurn);
        }

        StartTurn_ServerRpc(player);
    }

    [Rpc(SendTo.Server)]
    public void StartTurn_ServerRpc(NetworkObjectReference playerEnded)
    {
        if ((NetworkObject)playerEnded == (NetworkObject)p1.Value)
        {
            turnPlayer.Value = p2.Value;
            ((NetworkObject)p2.Value).GetComponent<PlayerController>().DrawCard_ServerRpc(1);
        }
        if ((NetworkObject)playerEnded == (NetworkObject)p2.Value)
        {
            turnPlayer.Value = p1.Value;
            ((NetworkObject)p1.Value).GetComponent<PlayerController>().DrawCard_ServerRpc(1);
        }

        int turnPlayerId = turnPlayer.Value.Equals(p1.Value) ? 1 : 2;

        List<NetworkObjectReference> units = new List<NetworkObjectReference>();
        units = GameBoard.GetAllUnits(turnPlayerId);

        foreach (NetworkObjectReference unit in units)
        {
            CardObjectField script = ((NetworkObject)unit).GetComponent<CardObjectField>();
            script.StartEffectTrigger(EffectTrigger.OnStartOfTurn);
        }

        units = GameBoard.GetAllUnitsSpecificField(turnPlayerId == 1 ? 1 : 2);

        foreach (NetworkObjectReference unit in units)
        {
            CardObjectField script = ((NetworkObject)unit).GetComponent<CardObjectField>();
            script.StartEffectTrigger(EffectTrigger.OnStartOfYourTurn);
        }

        units = GameBoard.GetAllUnitsSpecificField(turnPlayerId == 1 ? 2 : 1);

        foreach (NetworkObjectReference unit in units)
        {
            CardObjectField script = ((NetworkObject)unit).GetComponent<CardObjectField>();
            script.StartEffectTrigger(EffectTrigger.OnStartOfEnemyTurn);
        }
    }

    [Rpc(SendTo.Server)]
    public void Defeat_ServerRpc(NetworkObjectReference player)
    {
        if((NetworkObject)player == (NetworkObject)p1.Value)
        {
            hasGameEnded.Value = true;
            playerWon.Value = p2.Value;
        }
        if ((NetworkObject)player == (NetworkObject)p2.Value)
        {
            hasGameEnded.Value = true;
            playerWon.Value = p1.Value;
        }
    }

    public NetworkObjectReference GetCardPlayerRef(NetworkObjectReference card, bool otherPlayer = false)
    {
        int id = GameBoard.instance.GetCardBoardId(card);
        if (id == 1)
        {
            if (otherPlayer)
            {
                return (NetworkObject)p2.Value;
            }
            return (NetworkObject)p1.Value;
        }
        if (id == 2)
        {
            if (otherPlayer)
            {
                return (NetworkObject)p1.Value;
            }
            return (NetworkObject)p2.Value;
        }
        return new NetworkObjectReference();
    }
}
