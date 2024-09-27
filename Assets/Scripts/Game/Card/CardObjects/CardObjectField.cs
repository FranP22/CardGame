using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Security.Cryptography;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardObjectField : NetworkBehaviour
{
    [HideInInspector]
    public NetworkVariable<AllyCard> card = new NetworkVariable<AllyCard>();
    private NetworkVariable<bool> cardAssigned = new NetworkVariable<bool>(false);
    [HideInInspector]
    public NetworkVariable<int> attacksLeft = new NetworkVariable<int>(0);

    private NetworkVariable<NetworkObjectReference> player = new NetworkVariable<NetworkObjectReference>();

    [SerializeField]
    private GameObject image;
    private RawImage imageComponent;

    public TextMeshProUGUI attackText;
    public TextMeshProUGUI healthText;

    void Start()
    {
        imageComponent = image.GetComponent<RawImage>();
    }


    void Update()
    {
        if (cardAssigned.Value == false) return;

        UpdateCard();
    }

    public void UpdateCard()
    {
        attackText.text = card.Value.allyStats.currentAttack.ToString();
        healthText.text = card.Value.allyStats.currentHealth.ToString();
    }

    public void CreateCard(CardType type, int id)
    {
        switch (type)
        {
            case CardType.Champion:
                ChampionCard cc = CardDatabaseManager.instance.GetChampionCard(id);
                card = new NetworkVariable<AllyCard>(cc.ChampionClone());
                CreateMinion();
                break;
            case CardType.Minion:
                AllyCard ac = CardDatabaseManager.instance.GetAllyCard(id);
                card.Value = ac.AllyClone();
                CreateMinion();
                break;
            case CardType.Summon:
                AllyCard sc = CardDatabaseManager.instance.GetSummonCard(id);
                card.Value = sc.AllyClone();
                CreateMinion();
                break;
            default:
                break;
        }

        SetImage(card.Value.cardInfo.name);

        cardAssigned.Value = true;
    }

    private void CreateMinion()
    {
        card.Value.allyStats.currentAttack = card.Value.allyStats.attack;
        card.Value.allyStats.currentHealth = card.Value.allyStats.health;
    }

    private void SetImage(string name, string folder = "")
    {
        string path;
        if (folder == "")
        {
            path = "Assets/Materials/Images/" + name + ".png";
        }
        else
        {
            path = "Assets/Materials/Images/" + folder + "/" + name + ".png";
        }
        if (!File.Exists(path))
        {
            Debug.Log("Image doesn't exist");
            return;
        }

        byte[] imageData = File.ReadAllBytes(path);


        Texture2D tex = new Texture2D(2, 2);
        if (tex.LoadImage(imageData))
        {
            imageComponent = image.GetComponent<RawImage>();
            imageComponent.texture = tex;
        }
        else
        {
            Debug.Log("Load image failed");
        }
    }

    public void SetPlayerRef(NetworkObjectReference player)
    {
        if(card.Value.cardInfo.cardType == CardType.Champion)
        {
            this.player.Value = player;
        }
    }

    public void Damage(NetworkObjectReference source, int amount)
    {
        card.Value.allyStats.currentHealth -= amount;

        CardObjectField sourceScript = ((NetworkObject)source).GetComponent<CardObjectField>();
        if(amount > 0)
        {
            sourceScript.StartEffectTrigger(EffectTrigger.OnInflictDamage);
            StartEffectTrigger(EffectTrigger.OnDealtDamage);
        }

        if (card.Value.allyStats.currentHealth <= 0)
        {
            StartEffectTrigger(EffectTrigger.OnDeath);
            sourceScript.StartEffectTrigger(EffectTrigger.OnKill);
            Destroy(gameObject);
        }
    }

    public void Heal(NetworkObjectReference source, int amount)
    {
        bool healSuccess = true;
        if (card.Value.allyStats.currentHealth >= card.Value.allyStats.health)
            healSuccess = false;

        card.Value.allyStats.currentHealth += amount;
        if(card.Value.allyStats.currentHealth >= card.Value.allyStats.health) 
            card.Value.allyStats.currentHealth = card.Value.allyStats.health;

        CardObjectField sourceScript = ((NetworkObject)source).GetComponent<CardObjectField>();
        if (healSuccess)
        {

        }
    }

    public void StartEffectTrigger(EffectTrigger trigger, NetworkObject source = null)
    {
        AdditionalEffectTriggers(trigger);

        List<CardEffect> effects = card.Value.effects;
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i].trigger == trigger)
            {
                TriggerEffect(effects[i]);
            }
        }
    }

    private void AdditionalEffectTriggers(EffectTrigger trigger)
    {
        if (trigger == EffectTrigger.OnStartOfYourTurn)
        {
            attacksLeft.Value = 1;
        }

        if(trigger == EffectTrigger.OnEnter)
        {
            for(int i = 0;i < card.Value.keyWords.Count; i++)
            {
                if (card.Value.keyWords[i].keyword == CardKeyWords.Haste) attacksLeft.Value = 1;
            }
        }

        if (trigger == EffectTrigger.OnDeath)
        {
            if (card.Value.cardInfo.cardType == CardType.Champion)
            {
                GameController.instance.Defeat_ServerRpc(player.Value);
            }
        }
    }

    public void TriggerEffect(CardEffect effect)
    {
        List<NetworkObjectReference> targets = new List<NetworkObjectReference>();

        switch (effect.target)
        {
            case EffectTarget.Player:
                targets.Add(GameController.instance.GetCardPlayerRef(gameObject));
                break;

            case EffectTarget.EnemyPlayer:
                targets.Add(GameController.instance.GetCardPlayerRef(gameObject, true));
                break;

            case EffectTarget.BothPlayers:
                targets.Add(GameController.instance.GetCardPlayerRef(gameObject, false));
                targets.Add(GameController.instance.GetCardPlayerRef(gameObject, true));
                break;

            default:
                return;
        }

        DoEffect(targets, effect);
    }

    private void DoEffect(List<NetworkObjectReference> targets, CardEffect effect)
    {
        for(int i = 0;i < targets.Count; i++)
        {
            NetworkObject target = (NetworkObject) targets[i];

            if(effect.target == EffectTarget.Player || effect.target == EffectTarget.EnemyPlayer || effect.target == EffectTarget.BothPlayers)
            {
                var targetScript = target.GetComponent<PlayerController>();
                switch (effect.effect)
                {
                    case Effect.Draw:
                        targetScript.DrawCard_ServerRpc(effect.amount);
                        break;

                    default:
                        continue;
                }
            }
            else
            {
                var targetScript = target.GetComponent<CardObjectField>();
                switch (effect.effect)
                {
                    case Effect.Heal:
                        targetScript.Heal(gameObject, effect.amount);
                        break;

                    case Effect.Damage:
                        targetScript.Damage(gameObject, effect.amount);
                        break;

                    default:
                        continue;
                }
            }
        }
    }
}
