using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class Deck : INetworkSerializable, IEquatable<Deck>
{
    public ChampionCard champion = null;
    public List<EquipCard> equipCards = new List<EquipCard>();
    public List<Card> cards = new List<Card>();

    public bool CheckDeckValid()
    {
        if(champion == null || champion.cardInfo.cardType != CardType.Champion) return false;

        foreach(Card c in equipCards)
            if(c.cardInfo.cardType != CardType.Equipment) return false;

        if (cards.Count < GameManager.instance.deckSizeMin || cards.Count > GameManager.instance.deckSizeMax) return false;

        return true;
    }

    public Deck(ChampionCard champion, List<EquipCard> equips, List<Card> cards)
    {
        this.champion = champion;
        this.equipCards = equips;
        this.cards = cards;
    }

    public Deck()
    {

    }

    public static Deck CreateDemoDeck()
    {
        ChampionCard champion = CardDatabaseManager.instance.GetChampionCard(0).ChampionClone();
        List<EquipCard> equips = new List<EquipCard>();
        List<Card> cards = new List<Card>();

        for(int i = 0; i < 5; i++)
        {
            equips.Add(CardDatabaseManager.instance.GetEquipCard(i).EquipClone());
        }
        for (int i = 0; i < 5; i++)
        {
            equips.Add(CardDatabaseManager.instance.GetEquipCard(4).EquipClone());
        }

        for (int i = 0; i < GameManager.instance.deckSizeMin; i++)
        {
            cards.Add(CardDatabaseManager.instance.GetAllyCard(0).AllyClone());
        }

        Deck deck = new Deck(champion, equips, cards);

        return deck;
    }

    public void ShuffleDeck()
    {
        cards.Shuffle();
    }

    public bool Equals(Deck other)
    {
        return (champion == other.champion && equipCards.Equals(other.equipCards) && cards.Equals(other.cards));
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();

            //CHAMPION
            reader.ReadValueSafe(out champion);

            //EQUIPMENT
            ushort equipCount = 0;
            List<EquipCard> equipCards = new List<EquipCard>();
            reader.ReadValueSafe(out equipCount);
            for(int i = 0; i < equipCount; i++)
            {
                EquipCard c;
                reader.ReadValueSafe(out c);
                equipCards.Add(c);
            }
            this.equipCards = equipCards;

            //CARDS
            ushort cardCount = (int)0;
            List<Card> cards = new List<Card>();
            reader.ReadValueSafe(out cardCount);
            for (int i = 0; i < cardCount; i++)
            {
                CardType type;
                reader.ReadValueSafe<CardType>(out type);

                switch (type)
                {
                    case CardType.Minion:
                        AllyCard c1;
                        reader.ReadValueSafe<AllyCard>(out c1);
                        cards.Add(c1);
                        break;

                    case CardType.Magic:
                        MagicCard c2;
                        reader.ReadValueSafe<MagicCard>(out c2);
                        cards.Add(c2);
                        break;

                    default:
                        Card c;
                        reader.ReadValueSafe(out c);
                        cards.Add(c);
                        break;
                }
            }
            this.cards = cards;
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();

            //CHAMPION
            writer.WriteValueSafe(champion);

            //EQUIPMENT
            ushort equipCount = (ushort)this.equipCards.Count;
            writer.WriteValueSafe(equipCount);
            for (int i = 0; i < equipCount; i++)
            {
                EquipCard c = this.equipCards[i];
                writer.WriteValueSafe(c);
            }

            //CARDS
            ushort cardCount = (ushort)this.cards.Count;
            writer.WriteValueSafe(cardCount);
            for (int i = 0; i < cardCount; i++)
            {
                Card c = this.cards[i];
                CardType type = c.cardInfo.cardType;
                writer.WriteValueSafe<CardType>(type);

                switch (type)
                {
                    case CardType.Minion:
                        AllyCard c1 = c as AllyCard;
                        writer.WriteValueSafe<AllyCard>(c1);
                        break;

                    case CardType.Magic:
                        MagicCard c2 = c as MagicCard;
                        writer.WriteValueSafe<MagicCard>(c2);
                        break;

                    default:
                        writer.WriteValueSafe(c);
                        break;
                }
            }
        }
    }
}
