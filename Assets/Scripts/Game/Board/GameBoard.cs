using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameBoard : NetworkBehaviour
{
    public static GameBoard instance;

    public GameObject fcardPrefab;
    public GameObject ecardPrefab;

    public NetworkObject allies1, allies2;
    public NetworkObject champion1, champion2;
    public TextMeshProUGUI mana1, mana2;
    public TextMeshProUGUI exp1, exp2, level1, level2;
    public int sizeFromEdge = 50;
    public int sizeBetweenCards = 30;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
            return;
        }
    }

    void Start()
    {
        CreateAlliesWidth_ServerRpc();
    }

    /*[Rpc(SendTo.Server)]
    private void UpdateMana_ServerRpc(player)
    {

    }*/

    [Rpc(SendTo.Server)]
    private void CreateAlliesWidth_ServerRpc()
    {
        RectTransform rect1 = allies1.GetComponent<RectTransform>();
        RectTransform rect2 = allies2.GetComponent<RectTransform>();

        float size = GameManager.instance.fieldAllyAmount * 200 + (GameManager.instance.fieldAllyAmount - 1) * sizeBetweenCards + sizeFromEdge;

        rect1.sizeDelta = new Vector2(size, 250);
        rect2.sizeDelta = new Vector2(size, 250);
    }

    [Rpc(SendTo.Server)]
    public void UpdateMana_ServerRpc(NetworkObjectReference player, int id = 0)
    {
        int maxMana = ((NetworkObject)player).GetComponent<PlayerController>().maxMana.Value;
        int currentMana = ((NetworkObject)player).GetComponent<PlayerController>().currentMana.Value;
        if (id == 1)
        {
            mana1.text = currentMana + "/" + maxMana;
        }
        if(id == 2)
        {
            mana2.text = currentMana + "/" + maxMana;
        }
        return;
    }

    [Rpc(SendTo.Server)]
    public void UpdateExp_ServerRpc(NetworkObjectReference player, int id = 0)
    {
        int level = ((NetworkObject)player).GetComponent<PlayerController>().level.Value;
        int exp = ((NetworkObject)player).GetComponent<PlayerController>().experience.Value;
        int expReq = PlayerLevel.GetRequiredExp(level);

        string expText = level < 5 ? exp.ToString() + "/" + expReq.ToString() : exp.ToString();

        Debug.Log(expText);
        if (id == 1)
        {
            level1.text = level.ToString();
            exp1.text = expText;
        }
        if (id == 2)
        {
            level2.text = level.ToString();
            exp2.text = expText;
        }
        return;
    }

    [Rpc(SendTo.Server)]
    public void PlayCard_ServerRpc(NetworkObjectReference playerRef, CardType type, int cardId, int player = 0, int handId = -1)
    {
        PlayerController pc = ((NetworkObject)playerRef).GetComponent<PlayerController>();
       
        switch (type)
        {
            case CardType.Champion:
                PlayCardChampion(cardId, player, playerRef);
                break;
            case CardType.Equipment:
                PlayCardEquip(cardId, player, playerRef);
                break;
            case CardType.Minion:
                PlayCardMinion(type, cardId, player, playerRef, handId);
                break;
            case CardType.Summon:
                PlayCardMinion(type, cardId, player, playerRef, handId);
                break;
            case CardType.Magic:
                //PlayCardMagic(card, player, playerRef);
                break;
            default:
                pc.PlayCardCallBack_ServerRpc(CardPlaySuccess.Fail);
                return;
        }

        //pc.PlayCardCallBack(CardPlaySuccess.Fail);
        return;
    }

    private void PlayCardChampion(int cardId, int player, NetworkObjectReference playerRef, int handId = -1)
    {
        PlayerController pc = ((NetworkObject)playerRef).GetComponent<PlayerController>();

        NetworkObject championZone = GetChampionZone(player);

        GameObject champion = Instantiate(fcardPrefab);

        champion.GetComponent<NetworkObject>().Spawn(true);

        CardObjectField championScript = champion.GetComponent<CardObjectField>();
        championScript.CreateCard(CardType.Champion, cardId);
        championScript.SetPlayerRef(playerRef);

        champion.transform.SetParent(championZone.transform);
        champion.transform.rotation = Quaternion.Euler(90, 0, 0);
        champion.transform.position = championZone.transform.position;

        pc.SetChampion_ServerRpc(champion);
        return;
    }

    private void PlayCardEquip(int cardId, int player, NetworkObjectReference playerRef, int handId = -1)
    {
        PlayerController pc = ((NetworkObject)playerRef).GetComponent<PlayerController>();

        GameObject equip = Instantiate(ecardPrefab);

        equip.GetComponent<NetworkObject>().Spawn(true);

        CardObjectEquip equipScript = equip.GetComponent<CardObjectEquip>();
        equipScript.CreateCard(cardId);
        equipScript.SetPlayerRef(playerRef);

        equipScript.StartEffectTrigger(EffectTrigger.OnEquip);
        return;
    }

    private void PlayCardMinion(CardType type, int cardId, int player, NetworkObjectReference playerRef, int handId = -1)
    {
        PlayerController pc = ((NetworkObject)playerRef).GetComponent<PlayerController>();

        NetworkObject field = GetAllyField(player);
        if (field.transform.GetChild(0).childCount == GameManager.instance.fieldAllyAmount)
        {
            pc.PlayCardCallBack_ServerRpc(CardPlaySuccess.NotEnoughRoom);
            return;
        }

        GameObject minion = Instantiate(fcardPrefab);

        minion.GetComponent<NetworkObject>().Spawn(true);

        CardObjectField minionScript = minion.GetComponent<CardObjectField>();
        minionScript.CreateCard(type, cardId);

        minion.transform.SetParent(field.transform.GetChild(0));
        minion.transform.rotation = Quaternion.Euler(90, 0, 0);

        pc.PlayCardCallBack_ServerRpc(CardPlaySuccess.Success, handId);

        minionScript.StartEffectTrigger(EffectTrigger.OnEnter);
        return;
    }

    private void PlayCardMagic(Card card, int player, NetworkObjectReference playerRef, int handId = -1)
    {
        return;
    }

    private NetworkObject GetAllyField(int player)
    {
        NetworkObject field;
        if (player == 1)
        {
            field = allies1;
        }
        else if (player == 2)
        {
            field = allies2;
        }
        else
        {
            return null;
        }

        return field;
    }

    private NetworkObject GetChampionZone(int player)
    {
        NetworkObject field;
        if (player == 1)
        {
            field = champion1;
        }
        else if (player == 2)
        {
            field = champion2;
        }
        else
        {
            return null;
        }

        return field;
    }

    public int GetCardBoardId(NetworkObjectReference card)
    {
        if (card.Equals(champion1.transform.GetChild(0).gameObject)) return 1;
        for (int i = 0; i < allies1.transform.GetChild(0).childCount; i++)
        {
            if (card.Equals(allies1.transform.GetChild(0).GetChild(i).gameObject))
            {
                return 1;
            }
        }

        if (card.Equals(champion2.transform.GetChild(0).gameObject)) return 2;
        for (int i = 0; i < allies2.transform.GetChild(0).childCount; i++)
        {
            if (card.Equals(allies2.transform.GetChild(0).GetChild(i).gameObject))
            {
                return 2;
            }
        }

        return 0;
    }

    public static List<NetworkObjectReference> GetAllUnits(int turnPlayer = 0, bool withChampion = true)
    {
        List<NetworkObjectReference> units = new List<NetworkObjectReference>();
        if (turnPlayer == 1)
        {
            if (withChampion)
            {
                units.Add(GetChampionOnBoard(1));
                units.Add(GetChampionOnBoard(2));
            }

            units.AddRange(GetMinionsOnBoard(1));
            units.AddRange(GetMinionsOnBoard(2));
        }
        else if (turnPlayer == 2)
        {
            if (withChampion)
            {
                units.Add(GetChampionOnBoard(2));
                units.Add(GetChampionOnBoard(1));
            }

            units.AddRange(GetMinionsOnBoard(2));
            units.AddRange(GetMinionsOnBoard(1));
        }

        return units;
    }

    public static List<NetworkObjectReference> GetAllUnitsSpecificField(int field = 0, bool withChampion = true)
    {
        List<NetworkObjectReference> units = new List<NetworkObjectReference>();
        if (field == 1)
        {
            if (withChampion)
            {
                units.Add(GetChampionOnBoard(1));
            }

            units.AddRange(GetMinionsOnBoard(1));
        }
        else if (field == 2)
        {
            if (withChampion)
            {
                units.Add(GetChampionOnBoard(2));
            }

            units.AddRange(GetMinionsOnBoard(2));
        }

        return units;
    }

    public static List<NetworkObjectReference> GetMinionsOnBoard(int board)
    {
        List<NetworkObjectReference> cards = new List<NetworkObjectReference>();
        if (board == 1)
        {
            for (int i = 0; i < instance.allies1.transform.GetChild(0).childCount; i++)
            {
                cards.Add(instance.allies1.transform.GetChild(0).GetChild(i).gameObject);
            }
        }

        if (board == 2)
        {
            for (int i = 0; i < instance.allies2.transform.GetChild(0).childCount; i++)
            {
                cards.Add(instance.allies2.transform.GetChild(0).GetChild(i).gameObject);
            }
        }

        return cards;
    }

    public static NetworkObjectReference GetChampionOnBoard(int board)
    {
        if (board == 1)
        {
            return instance.champion1.transform.GetChild(0).gameObject;
        }

        if (board == 2)
        {
            return instance.champion2.transform.GetChild(0).gameObject;
        }

        return new NetworkObjectReference();
    }
}
