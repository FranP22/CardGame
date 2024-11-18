using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [HideInInspector]
    public NetworkVariable<Deck> deck = new NetworkVariable<Deck>();
    [HideInInspector]
    public NetworkVariable<bool> isDeckSet = new NetworkVariable<bool>(false);
    [HideInInspector]
    public NetworkVariable<List<Card>> hand = new NetworkVariable<List<Card>>();

    private NetworkVariable<bool> AI = new NetworkVariable<bool>(false);

    [HideInInspector]
    public NetworkVariable<NetworkObjectReference> champion = new NetworkVariable<NetworkObjectReference>();

    [HideInInspector]
    public NetworkVariable<int> maxMana = new NetworkVariable<int>(0);
    [HideInInspector]
    public NetworkVariable<int> currentMana = new NetworkVariable<int>(0);
    [HideInInspector]
    public NetworkVariable<int> level = new NetworkVariable<int>(0);
    [HideInInspector]
    public NetworkVariable<int> experience = new NetworkVariable<int>(0);

    [SerializeField]
    private GameObject camera;
    [SerializeField]
    private GameObject UIPrefab;
    private GameObject UI;

    private GameObject objectClicked1 = null;
    private GameObject objectClicked2 = null;

    [HideInInspector]
    public NetworkVariable<bool> isSelecting = new NetworkVariable<bool>(false);

    public bool IsAI()
    {
        return AI.Value;
    }

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Escape) && !AI.Value && !isSelecting.Value)
        {
            GameController.instance.EndTurn_ServerRpc(gameObject);
        }

        if (Input.GetKeyDown(KeyCode.Space) && !AI.Value && !isSelecting.Value)
        {
            Debug.Log(gameObject.GetComponent<NetworkObject>().OwnerClientId);
            GameController.instance.Defeat_ServerRpc(gameObject);
        }

        if (Input.GetMouseButtonDown(0) && !AI.Value && !isSelecting.Value)
        {
            GameObject obj = HitDetect();
            if(obj != null)
            {
                objectClicked1 = obj;
            }
        }

        if (Input.GetMouseButtonUp(0) && !AI.Value && !isSelecting.Value)
        {
            GameObject obj = HitDetect();
            if (obj != null)
            {
                objectClicked2 = obj;

                if(objectClicked1.CompareTag("Ally") && objectClicked2.CompareTag("Ally"))
                {
                    CombatInit_ServerRpc(gameObject, objectClicked1, objectClicked2);
                }
            }
        }
    }

    private GameObject HitDetect()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {

            GameObject hitObject = hit.transform.gameObject;
            return hitObject;

        }
        return null;
    }

    [Rpc(SendTo.Server)]
    public void CombatInit_ServerRpc(NetworkObjectReference player, NetworkObjectReference ob1, NetworkObjectReference ob2)
    {
        NetworkObject pSelf = GameController.instance.GetCardPlayerRef(ob1);
        NetworkObject pEnemy = GameController.instance.GetCardPlayerRef(ob2);

        Debug.Log((pSelf == (NetworkObject)player) + " " + (pEnemy != (NetworkObject)player));

        if (pSelf == (NetworkObject)player && pEnemy != (NetworkObject)player)
        {
            CardObjectField c = ((NetworkObject)ob1).GetComponent<CardObjectField>();

            if(c.card.Value.allyStats.attack == 0)
            {
                CombatCallBack(CardBattleSuccess.ZeroAttack);
                return;
            }

            if(c.attacksLeft.Value >= 1)
            {
                GameController.instance.CardCombat_ServerRpc(gameObject, ob1, ob2);
            }
            else
            {
                CombatCallBack(CardBattleSuccess.NoAttacksLeft);
            }
        }
        else
        {
            CombatCallBack(CardBattleSuccess.Fail);
        }
    }

    public void CombatCallBack(CardBattleSuccess success)
    {
        Debug.Log(success.ToString());
    }

    public override void OnNetworkSpawn()
    {

        base.OnNetworkSpawn();

        if (IsOwner)
        {
            //ADD INITIAL POSITION
            deck.OnValueChanged += DeckValueChanged;
            hand.OnValueChanged += HandValueChanged;

            maxMana.OnValueChanged += ManaValueChanged;
            currentMana.OnValueChanged += ManaValueChanged;

            level.OnValueChanged += ExpValueChanged;
            experience.OnValueChanged += ExpValueChanged;

            CreateUI_OwnerRpc();
        }
        else
        {
            this.enabled = false;
        }
    }

    private void OnApplicationQuit()
    {
        deck.OnValueChanged -= DeckValueChanged;
        hand.OnValueChanged -= HandValueChanged;

        maxMana.OnValueChanged -= ManaValueChanged;
        currentMana.OnValueChanged -= ManaValueChanged;

        level.OnValueChanged -= ExpValueChanged;
        experience.OnValueChanged -= ExpValueChanged;
    }

    [Rpc(SendTo.Server)]
    public void CreateAI_ServerRpc()
    {
        AI.Value = true;
        SetDeck_ServerRpc(Deck.CreateDemoDeck());
        deck.Value.ShuffleDeck();

        camera.SetActive(false);
        UI.SetActive(false);

        AIController aic = gameObject.AddComponent<AIController>();
        aic.mainController = this;
    }

    [Rpc(SendTo.Server)]
    public void CreatePlayer_ServerRpc(Deck deck)
    {
        AI.Value = false;
        SetDeck_ServerRpc(deck);
        this.deck.Value.ShuffleDeck();
    }

    [Rpc(SendTo.Server)]
    public void StartGame_ServerRpc(bool isFirstTurn = false)
    {
        GameController.instance.PlayCard_ServerRpc(gameObject, CardType.Champion, deck.Value.champion.cardInfo.GetId());
        maxMana.Value = deck.Value.champion.cardInfo.mana;

        level.Value = 1;
        //ExpValueChanged(0, 0);

        DrawCard_ServerRpc(5);

        if (isFirstTurn)
        {
            currentMana.Value = maxMana.Value;
        }
    }

    [Rpc(SendTo.Server)]
    public void StartTurn_ServerRpc()
    {
        bool isLeveling = false;
        int levelUp = PlayerLevel.GetPlayerLevelUp(level.Value, experience.Value);
        if (levelUp > 0)
        {
            isLeveling = true;
            level.Value++;
            experience.Value -= levelUp;

            maxMana.Value += 2;

            if (AI.Value)
            {
                GetComponent<AIController>().EquipCard();
            }
            else
            {
                isSelecting.Value = true;
                UI.GetComponent<PlayerUIController>().StartEquipSelect();
            }
        }

        if (!isLeveling)
        {
            AfterLevelup_ServerRpc();
        }
    }

    [Rpc(SendTo.Server)]
    public void AfterLevelup_ServerRpc()
    {
        currentMana.Value = maxMana.Value;
        DrawCard_ServerRpc(1);
    }

    [Rpc(SendTo.Server)]
    public void EndTurn_ServerRpc()
    {
        experience.Value += currentMana.Value * 15;
    }

    [Rpc(SendTo.Server)]
    public void SetDeck_ServerRpc(Deck newDeck)
    {
        deck.Value = newDeck;
        isDeckSet.Value = true;
    }

    [Rpc(SendTo.Server)]
    public void SetChampion_ServerRpc(NetworkObjectReference champion)
    {
        this.champion.Value = (NetworkObject)champion;
    }

    [Rpc(SendTo.Server)]
    public void DrawCard_ServerRpc(int amount)
    {
        List<Card> previousValue = hand.Value.ToList();

        for (int i = 0; i < amount; i++)
        {
            int lastIndex = deck.Value.cards.Count - 1;
            //if (lastIndex == -1) ; //handle deckout

            Card c = deck.Value.cards[lastIndex];
            deck.Value.cards.RemoveAt(lastIndex);
            hand.Value.Add(c);
            //Debug.Log("deck: " + deck.Value.cards.Count + ", Hand: " + hand.Value.Count);
        }
        HandValueChanged(previousValue, hand.Value);
    }

    [Rpc(SendTo.Server)]
    public void PlayCardInit_ServerRpc(int handId)
    {
        Card c = hand.Value[handId];
        if (currentMana.Value < c.cardInfo.mana) return;

        GameController.instance.PlayCard_ServerRpc(gameObject, hand.Value[handId].cardInfo.cardType, hand.Value[handId].cardInfo.GetId(), handId);

        return;
    }

    [Rpc(SendTo.Server)]
    public void PlayCardCallBack_ServerRpc(CardPlaySuccess success, int handId = -1)
    {
        if(success == CardPlaySuccess.Success)
        {
            List<Card> pCards = hand.Value.ToList();
            currentMana.Value -= hand.Value[handId].cardInfo.mana;
            experience.Value += hand.Value[handId].cardInfo.mana * 10;
            hand.Value.RemoveAt(handId);
            HandValueChanged(pCards, hand.Value);
        }
        
        if(success == CardPlaySuccess.NotEnoughRoom)
        {
            if(AI.Value == true)
            {
                gameObject.GetComponent<AIController>().actionStage++;
            }
        }
    }

    [Rpc(SendTo.Owner)]
    private void CreateUI_OwnerRpc()
    {
        UI = Instantiate(UIPrefab); 
        UI.GetComponent<Canvas>().worldCamera = Camera.main;
        UI.GetComponent<PlayerUIController>().player = gameObject;
    }

    private void DeckValueChanged(Deck previousValue, Deck newValue)
    {
        //Debug.Log((AI.Value ? "AI -" : "Player -") + " Deck changed");
    }

    private void HandValueChanged(List<Card> previousValue, List<Card> newValue)
    {
        //Debug.Log((AI.Value ? "AI -" : "Player -") + " Hand changed from " + previousValue.Count + " cards to " + newValue.Count + " cards");
        UI.GetComponent<PlayerUIController>().UpdateUI(newValue);
    }

    private void ManaValueChanged(int previousValue, int newValue)
    {
        GameController.instance.UpdatePlayerMana_ServerRpc(gameObject);
    }

    private void ExpValueChanged(int previousValue, int newValue)
    {
        Debug.Log(experience.Value + " " + level.Value);
        GameController.instance.UpdatePlayerExp_ServerRpc(gameObject);
    }
}
