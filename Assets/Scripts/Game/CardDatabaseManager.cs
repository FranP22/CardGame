using System;
using System.Collections;
using System.Collections.Generic;
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
        return cardDatabase.championsCards;
    }
    public ChampionCard GetChampionCard(int id)
    {
        return cardDatabase.championsCards[id];
    }

    public List<AllyCard> GetAllyCards()
    {
        return cardDatabase.allyCards;
    }
    public AllyCard GetAllyCard(int id)
    {
        return cardDatabase.allyCards[id];
    }

    public List<AllyCard> GetSummonCards()
    {
        return cardDatabase.summonCards;
    }
    public AllyCard GetSummonCard(int id)
    {
        return cardDatabase.summonCards[id];
    }

    public List<EquipCard> GetEquipCards()
    {
        return cardDatabase.equipmentCards;
    }
    public EquipCard GetEquipCard(int id)
    {
        return cardDatabase.equipmentCards[id];
    }

    public List<MagicCard> GetMagicCards()
    {
        return cardDatabase.magicCards;
    }
    public MagicCard GetMagicCard(int id)
    {
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
