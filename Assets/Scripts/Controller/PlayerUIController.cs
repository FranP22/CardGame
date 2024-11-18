using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Timeline;

public class PlayerUIController : MonoBehaviour
{
    [HideInInspector]
    public NetworkObjectReference player;

    [SerializeField]
    private GameObject hand;
    [SerializeField]
    private GameObject cardPrefab;

    [SerializeField]
    private GameObject equipScreen;

    //private int cardHandOffset = 50;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void UpdateUI(List<Card> updatedHand)
    {
        for (int i = 0; i < hand.transform.childCount; i++)
        {
            GameObject child = hand.transform.GetChild(i).gameObject;
            Destroy(child);
        }

        List<GameObject> cardList = new List<GameObject>();

        for(int i = 0; i < updatedHand.Count; i++)
        {
            Card card = updatedHand[i];
            /*switch (card.cardInfo.cardType)
            {
                case CardType.Minion:
                    AllyCard allyCard = card as AllyCard;
                    GameObject c = Instantiate(cardPrefab);

                    CardObjectUI cardScript = c.GetComponent<CardObjectUI>();
                    cardScript.card = allyCard;
                    cardScript.UpdateCard();
                    break;
                case CardType.Magic:
                    MagicCard magicCard = card as MagicCard;
                    break;
                default:
                    break;
            }*/
            GameObject c = Instantiate(cardPrefab);

            CardObjectUI cardScript = c.GetComponent<CardObjectUI>();
            cardScript.handId = i;
            cardScript.card = card.CardClone();
            cardScript.playerOwner = player;
            cardScript.UpdateCard();

            c.transform.SetParent(hand.transform, false);
            c.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            cardList.Add(c);
        }
    }

    public void StartEquipSelect()
    {
        for(int i = 0; i < equipScreen.transform.GetChild(0).childCount; i++)
        {
            GameObject child = equipScreen.transform.GetChild(0).GetChild(i).gameObject;
            Destroy(child);
        }

        equipScreen.SetActive(true);

        List<EquipCard> cards = ((NetworkObject)player).GetComponent<PlayerController>().deck.Value.equipCards;

        for(int i = 0; i < cards.Count; i++)
        {
            GameObject o = Instantiate(cardPrefab);
            CardObjectUI cardScript = o.GetComponent<CardObjectUI>();
            cardScript.card = ((EquipCard)cards[i]).EquipClone();
            cardScript.playerOwner = player;
            cardScript.handId = i;
            cardScript.UpdateCard();
            cardScript.SetSelectable(true);

            o.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            o.transform.SetParent(equipScreen.transform.GetChild(0));
        }
    }

    public void EndEquipSelect()
    {
        Transform cards = equipScreen.transform.GetChild(0);
        List<CardObjectUI> selectedCards = new List<CardObjectUI>();
        for(int i = 0; i < cards.childCount; i++)
        {
            CardObjectUI c = cards.GetChild(i).GetComponent<CardObjectUI>();
            if (c.isSelected)
            {
                selectedCards.Add(c);
            }
        }

        if (selectedCards.Count == 1)
        {
            //Debug.Log(selectedCards[0].card.cardInfo.GetId());
            PlayerController pc = ((NetworkObject)player).GetComponent<PlayerController>();

            Debug.Log(pc.deck.Value.equipCards[selectedCards[0].handId].cardInfo.GetId() + "/" + pc.deck.Value.equipCards[selectedCards[0].handId].cardInfo.name);

            GameBoard.instance.PlayCard_ServerRpc(player, CardType.Equipment, selectedCards[0].card.cardInfo.GetId());
            equipScreen.SetActive(false);

            pc.deck.Value.equipCards.RemoveAt(selectedCards[0].handId);
            pc.AfterLevelup_ServerRpc();
            pc.isSelecting.Value = false;
        }
    }
}
