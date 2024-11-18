using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class AIController : NetworkBehaviour
{
    public PlayerController mainController;

    private bool doingAnAction = false;
    [HideInInspector]
    public int actionStage = 0;

    void Start()
    {
        
    }

    void Update()
    {
        if (GameController.instance.IsPlayersTurn(gameObject))
        {
            if (!doingAnAction) StartCoroutine(Behaviour());
        }
    }

    IEnumerator Behaviour()
    {
        doingAnAction = true;
        yield return new WaitForSeconds(0.5f);
        StartBehaviour();
        doingAnAction = false;
    }

    private void StartBehaviour()
    {
        //attack
        if (actionStage == 0)
        {
            List<NetworkObjectReference> myCards = GameBoard.GetAllUnitsSpecificField(2);
            List<NetworkObjectReference> enemyCards = GameBoard.GetAllUnitsSpecificField(1);

            foreach (NetworkObjectReference card in myCards)
            {
                NetworkObjectReference randomEnemy = enemyCards[Random.Range(0, enemyCards.Count)];
                gameObject.GetComponent<PlayerController>().CombatInit_ServerRpc(gameObject, card, randomEnemy);
            }
            actionStage++;
            return;
        }

        //play cards
        if (actionStage == 1)
        {
            int handCount = mainController.hand.Value.Count;
            if (handCount > 0)
            {
                List<int> playable = new List<int>();

                for(int i = 0; i<handCount; i++)
                {
                    if (mainController.hand.Value[i].cardInfo.mana <= mainController.currentMana.Value)
                    {
                        playable.Add(i);
                    }
                }

                if(playable.Count > 0)
                {
                    int card = Random.Range(0, playable.Count);

                    mainController.PlayCardInit_ServerRpc(card);
                }
                else
                {
                    actionStage++;
                }

                return;
            }
            else
            {
                actionStage++;
                return;
            }
        }

        //end turn
        if (actionStage == 2)
        {
            GameController.instance.EndTurn_ServerRpc(gameObject);
            actionStage = 0;
        }
    }

    public void EquipCard()
    {
        List<EquipCard> ec = mainController.deck.Value.equipCards;
        if (ec.Count == 0) return;

        int rand = Random.Range(0, ec.Count);
        GameController.instance.PlayCard_ServerRpc(gameObject, CardType.Equipment, ec[rand].cardInfo.GetId());
        mainController.deck.Value.equipCards.RemoveAt(rand);
    }
}
