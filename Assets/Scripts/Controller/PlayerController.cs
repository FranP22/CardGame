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
    public NetworkVariable<int> maxMana = new NetworkVariable<int>(0);
    [HideInInspector]
    public NetworkVariable<int> currentMana = new NetworkVariable<int>(0);

    [SerializeField]
    private GameObject camera;
    [SerializeField]
    private GameObject UIPrefab;
    private GameObject UI;

    private GameObject objectClicked1 = null;
    private GameObject objectClicked2 = null;

    void Update()
    {
        //Debug.Log(deck.Value.cards.Count);
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Escape) && !AI.Value)
        {
            GameController.instance.EndTurn_ServerRpc(gameObject);
        }

        if (Input.GetMouseButtonDown(0) && !AI.Value)
        {
            GameObject obj = HitDetect();
            if(obj != null)
            {
                objectClicked1 = obj;
            }
        }

        if (Input.GetMouseButtonUp(0) && !AI.Value)
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

        //GameController.instance.CardCombat_ServerRpc(player, ob1, ob2);

        if (pSelf == (NetworkObject)player && pEnemy != (NetworkObject)player)
        {
            if(((NetworkObject)ob1).GetComponent<CardObjectField>().attacksLeft.Value >= 1)
            {
                GameController.instance.CardCombat_ServerRpc(gameObject, ob1, ob2);
                //((NetworkObject)ob1).GetComponent<CardObjectField>().attacksLeft.Value--;
                //CombatCallBack(CardBattleSuccess.Success);
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

    }

    public override void OnNetworkSpawn()
    {

        base.OnNetworkSpawn();

        if (IsOwner)
        {
            //ADD INITIAL POSITION
            deck.OnValueChanged += DeckValueChanged;
            hand.OnValueChanged += HandValueChanged;

            CreateUI_OwnerRpc();
        }
        else
        {
            this.enabled = false;
        }
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
    public void StartGame_ServerRpc()
    {
        GameController.instance.PlayCard_ServerRpc(gameObject, CardType.Champion, deck.Value.champion.cardInfo.GetId());
        DrawCard_ServerRpc(5);
    }

    [Rpc(SendTo.Server)]
    public void SetDeck_ServerRpc(Deck newDeck)
    {
        deck.Value = newDeck;
        isDeckSet.Value = true;
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
            Debug.Log("deck: " + deck.Value.cards.Count + ", Hand: " + hand.Value.Count);
        }
        HandValueChanged(previousValue, hand.Value);
    }

    [Rpc(SendTo.Server)]
    public void PlayCardInit_ServerRpc(int handId)
    {
        Card c = hand.Value[handId];
        if (c.cardInfo.currentMana < currentMana.Value) return;

        //PlayCardCallBack callback = PlayCardCallBack;
        GameController.instance.PlayCard_ServerRpc(gameObject, hand.Value[handId].cardInfo.cardType, hand.Value[handId].cardInfo.GetId(), handId);

        return;
    }

    public void PlayCardCallBack(CardPlaySuccess success, int handId = -1)
    {
        if(success == CardPlaySuccess.Success)
        {
            List<Card> pCards = hand.Value.ToList();
            currentMana.Value -= hand.Value[handId].cardInfo.currentMana;
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
}
