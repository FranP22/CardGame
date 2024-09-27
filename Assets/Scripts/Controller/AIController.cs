using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

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
                    if (mainController.hand.Value[i].cardInfo.currentMana <= mainController.currentMana.Value)
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
}
