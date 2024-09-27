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
            cardScript.card = card;
            cardScript.playerOwner = player;
            cardScript.UpdateCard();

            c.transform.SetParent(hand.transform, false);

            cardList.Add(c);
        }
    }
}
