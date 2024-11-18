using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardDatabaseManager : MonoBehaviour
{
    [SerializeField]
    private CardDatabase cardDatabase;
    public static CardDatabaseManager instance;

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public List<ChampionCard> GetChampionCards()
    {
        //List<ChampionCard> list = cardDatabase.championsCards.ToList();
        return cardDatabase.championsCards;
    }
    public ChampionCard GetChampionCard(int id)
    {
        ChampionCard card = cardDatabase.championsCards[id].ChampionClone();
        card.cardInfo.SetId(id);
        return card;
    }

    public List<AllyCard> GetAllyCards()
    {
        return cardDatabase.allyCards;
    }
    public AllyCard GetAllyCard(int id)
    {
        AllyCard card = cardDatabase.allyCards[id].AllyClone();
        card.cardInfo.SetId(id);
        return card;
    }

    public List<AllyCard> GetSummonCards()
    {
        return cardDatabase.summonCards;
    }
    public AllyCard GetSummonCard(int id)
    {
        AllyCard card = cardDatabase.summonCards[id].AllyClone();
        card.cardInfo.SetId(id);
        return card;
    }

    public List<EquipCard> GetEquipCards()
    {
        return cardDatabase.equipmentCards;
    }
    public EquipCard GetEquipCard(int id)
    {
        EquipCard card = cardDatabase.equipmentCards[id].EquipClone();
        card.cardInfo.SetId(id);
        return card;
    }

    public List<MagicCard> GetMagicCards()
    {
        return cardDatabase.magicCards;
    }
    public MagicCard GetMagicCard(int id)
    {
        //MagicCard card = cardDatabase.magicCards[id]
        return cardDatabase.magicCards[id];
    }

    /*public List<Card> GetCards(CardType type, CardFilter[] filters)
    {
        List<Card> cards = new List<Card>();
        switch (type)
        {
            case CardType.Champion:
            case CardType.Equipment:
            case CardType.Minion:
            case CardType.Summon:
            case CardType.Magic:
            default:
                return cards;
        }

        return cards;
    }

    public bool CardQuery(CardFilter operation, Card c, CardClass cl)
    {
        return operation(c, cl);
    }
    public delegate bool CardFilter(Card c, CardClass cl);
    public static bool QueryClass(Card c, CardClass cl)
    {
        return c.cardInfo.cardClasses.Contains(cl);
    }

    public void test()
    {
        CardQuery(QueryClass, new Card(), CardClass.Warrior);
        //GetCards(CardType.Champion);
    }*/
}
