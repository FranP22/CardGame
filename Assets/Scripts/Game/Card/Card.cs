using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using static Steamworks.InventoryItem;
using static UnityEngine.GraphicsBuffer;

[System.Serializable]
public struct CardInfo : INetworkSerializable
{
    private int id;
    public string name;
    //public Texture2D image;
    public CardType cardType;
    public CardElement cardElement;
    public List<CardClass> cardClasses;

    public int mana;
    [HideInInspector]
    public int currentMana;

    public void DefaultCardInfo()
    {
        id = -1;
        name = string.Empty;
        cardType = CardType.Minion;
        cardElement = CardElement.Basic;
        cardClasses = new List<CardClass>();
    }

    public CardInfo(CardInfo copy)
    {
        this.id = copy.id;
        this.name = copy.name;
        this.cardType = copy.cardType;
        this.cardElement = copy.cardElement;
        this.cardClasses = new List<CardClass>(copy.cardClasses);
        this.mana = copy.mana;
        this.currentMana = copy.currentMana;
    }

    public int GetId()
    {
        return id;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out id);
            reader.ReadValueSafe(out name);

            reader.ReadValueSafe(out cardType);
            reader.ReadValueSafe(out cardElement);

            ushort classCount = (ushort)0;
            reader.ReadValueSafe(out classCount);
            List<CardClass> classList = new List<CardClass>();
            for(int i = 0; i < classCount; i++)
            {
                CardClass c;
                reader.ReadValueSafe<CardClass>(out c);
                classList.Add(c);
            }
            cardClasses = classList;

            reader.ReadValueSafe(out mana);
            reader.ReadValueSafe(out currentMana);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(id);
            writer.WriteValueSafe(name);

            writer.WriteValueSafe(cardType);
            writer.WriteValueSafe(cardElement);

            ushort classCount = (ushort)cardClasses.Count;
            writer.WriteValueSafe(classCount);
            for (int i = 0; i < classCount; i++)
            {
                CardClass c = cardClasses[i];
                writer.WriteValueSafe<CardClass>(c);
            }

            writer.WriteValueSafe(mana);
            writer.WriteValueSafe(currentMana);
        }
    }
}

[System.Serializable]
public struct CardAllyStats : INetworkSerializable
{
    public int attack;
    public int health;

    [HideInInspector]
    public int currentAttack;
    [HideInInspector]
    public int currentHealth;

    public CardAllyStats(CardAllyStats copy)
    {
        this.attack = copy.attack;
        this.health = copy.health;
        this.currentAttack = copy.currentAttack;
        this.currentHealth = copy.currentHealth;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out attack);
            reader.ReadValueSafe(out health);
            reader.ReadValueSafe(out currentAttack);
            reader.ReadValueSafe(out currentHealth);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(attack);
            writer.WriteValueSafe(health);
            writer.WriteValueSafe(currentAttack);
            writer.WriteValueSafe(currentHealth);
        }
    }
}

[System.Serializable]
public struct CardKeyWord : INetworkSerializable
{
    public CardKeyWords keyword;
    public int level;

    public CardKeyWord(CardKeyWord copy)
    {
        this.keyword = copy.keyword;
        this.level = copy.level;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe<CardKeyWords>(out keyword);
            reader.ReadValueSafe(out level);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe<CardKeyWords>(keyword);
            writer.WriteValueSafe(level);
        }
    }
}

[System.Serializable]
public struct CardEffect : INetworkSerializable
{
    public EffectTrigger trigger;
    public EffectTarget target;
    public Effect effect;
    public int amount;

    public CardEffect(CardEffect copy)
    {
        this.trigger = copy.trigger;
        this.target = copy.target;
        this.effect = copy.effect;
        this.amount = copy.amount;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe<EffectTrigger>(out trigger);
            reader.ReadValueSafe<EffectTarget>(out target);
            reader.ReadValueSafe<Effect>(out effect);
            reader.ReadValueSafe(out amount);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe<EffectTrigger>(trigger);
            writer.WriteValueSafe<EffectTarget>(target);
            writer.WriteValueSafe<Effect>(effect);
            writer.WriteValueSafe(amount);
        }
    }
}

[System.Serializable]
public struct CardSpell : INetworkSerializable
{
    public EffectTarget target;
    public Effect effect;

    public CardSpell(CardSpell copy)
    {
        this.target = copy.target;
        this.effect = copy.effect;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe<EffectTarget>(out target);
            reader.ReadValueSafe<Effect>(out effect);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe<EffectTarget>(target);
            writer.WriteValueSafe<Effect>(effect);
        }
    }
}



[System.Serializable]
//#nullable disable
public class Card : INetworkSerializable, IEquatable<Card>
{
    public CardInfo cardInfo;

    public Card CardClone()
    {
        Card other = (Card)MemberwiseClone();
        other.cardInfo = new CardInfo(cardInfo);
        return other;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();

            ReadValues(reader);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            
            WriteValues(writer);
        }
    }

    public bool Equals(Card other)
    {
        return (cardInfo.GetId() == other.cardInfo.GetId() && cardInfo.name == other.cardInfo.name);
    }

    public virtual void ReadValues(FastBufferReader reader)
    {
        reader.ReadValueSafe(out cardInfo);
    }

    public virtual void WriteValues(FastBufferWriter writer)
    {
        writer.WriteValueSafe(cardInfo);
    }
}

[System.Serializable]
public class AllyCard : Card, IEquatable<AllyCard>
{
    //public CardStats cardStats;
    public CardAllyStats allyStats;
    public List<CardKeyWord> keyWords = new List<CardKeyWord>();
    public List<CardEffect> effects = new List<CardEffect>();

    public AllyCard AllyClone()
    {
        AllyCard other = (AllyCard)MemberwiseClone();
        other.cardInfo = new CardInfo(cardInfo);
        other.keyWords = new List<CardKeyWord>();
        other.effects = new List<CardEffect>();

        foreach (CardKeyWord keyword in keyWords)
        {
            other.keyWords.Add(keyword);
        }

        foreach (CardEffect effect in effects)
        {
            other.effects.Add(new CardEffect(effect));
        }

        return other;
    }

    public void Heal(int amount)
    {
        if (allyStats.currentHealth + amount <= allyStats.health)
        {
            allyStats.currentHealth += amount;
        }
        else
        {
            allyStats.currentHealth = allyStats.health;
        }
    }

    public void Damage(int amount)
    {
        allyStats.currentHealth -= amount;
    }

    public override void ReadValues(FastBufferReader reader)
    {
        base.ReadValues(reader);

        //reader.ReadValueSafe(out cardStats);
        reader.ReadValueSafe(out allyStats);

        ushort keywordsCount = 0;
        reader.ReadValueSafe(out keywordsCount);
        List<CardKeyWord> keywordList = new List<CardKeyWord>();
        for (int i = 0; i < keywordsCount; i++) 
        {
            CardKeyWord keyWord;
            reader.ReadValueSafe<CardKeyWord>(out keyWord);
            keywordList.Add(keyWord);
        }
        keyWords = keywordList;

        ushort effectsCount = 0;
        reader.ReadValueSafe(out effectsCount);
        List<CardEffect> effectsList = new List<CardEffect>();
        for(int i = 0;i < effectsCount; i++)
        {
            CardEffect effect;
            reader.ReadValueSafe<CardEffect>(out effect);
            effectsList.Add(effect);
        }
        effects = effectsList;
    }

    public override void WriteValues(FastBufferWriter writer)
    {
        base.WriteValues(writer);

        //writer.WriteValueSafe(cardStats);
        writer.WriteValueSafe(allyStats);

        ushort keywordsCount = (ushort)keyWords.Count;
        writer.WriteValueSafe(keywordsCount);
        for (int i = 0; i < keywordsCount; i++)
        {
            CardKeyWord keyWord = keyWords[i];
            writer.WriteValueSafe<CardKeyWord>(keyWord);
        }

        ushort effectsCount = (ushort)effects.Count;
        writer.WriteValueSafe(effectsCount);
        for (int i = 0; i < effectsCount; i++)
        {
            CardEffect effect = effects[i];
            writer.WriteValueSafe<CardEffect>(effect);
        }
    }

    public bool Equals(AllyCard other)
    {
        return (cardInfo.GetId() == other.cardInfo.GetId() && cardInfo.cardType == other.cardInfo.cardType);
    }
}

[System.Serializable]
public class ChampionCard : AllyCard, IEquatable<ChampionCard>
{
    public ChampionCard ChampionClone()
    {
        ChampionCard other = (ChampionCard)MemberwiseClone();
        other.cardInfo = new CardInfo(cardInfo);
        other.keyWords = new List<CardKeyWord>(keyWords);
        other.effects = new List<CardEffect>();

        foreach(CardEffect effect in effects)
        {
            other.effects.Add(new CardEffect(effect));
        }

        return other;
    }

    public override void ReadValues(FastBufferReader reader)
    {
        base.ReadValues(reader);
    }

    public override void WriteValues(FastBufferWriter writer)
    {
        base.WriteValues(writer);

    }

    public bool Equals(ChampionCard other)
    {
        return (cardInfo.GetId() == other.cardInfo.GetId() && cardInfo.cardType == other.cardInfo.cardType);
    }
}

[System.Serializable]
public class EquipCard : Card
{
    public override void ReadValues(FastBufferReader reader)
    {
        base.ReadValues(reader);
    }

    public override void WriteValues(FastBufferWriter writer)
    {
        base.WriteValues(writer);

    }
}

[System.Serializable]
public class MagicCard : Card
{
    //public CardStats cardStats;
    public List<CardSpell> effects = new List<CardSpell>();

    public void Cast()
    {

    }

    public override void ReadValues(FastBufferReader reader)
    {
        base.ReadValues(reader);

        //reader.ReadValueSafe(out cardStats);

        ushort effectsCount = 0;
        reader.ReadValueSafe(out effectsCount);
        List<CardSpell> spellList = new List<CardSpell>();
        for (int i = 0; i < effectsCount; i++)
        {
            CardSpell s;
            reader.ReadValueSafe<CardSpell>(out s);
            spellList.Add(s);
        }
        effects = spellList;
    }

    public override void WriteValues(FastBufferWriter writer)
    {
        base.WriteValues(writer);
        //writer.WriteValueSafe(cardStats);

        ushort effectsCount = (ushort)effects.Count;
        writer.WriteValueSafe(effectsCount);
        for(int i = 0;i < effectsCount; i++)
        {
            CardSpell s = effects[i];
            writer.WriteValueSafe<CardSpell>(s);
        }
    }
}