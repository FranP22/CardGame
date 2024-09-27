using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardDatabase", menuName = "Card Game/Card Database")]
public class CardDatabase : ScriptableObject
{
    public List<ChampionCard> championsCards;
    
    public List<EquipCard> equipmentCards;

    public List<MagicCard> magicCards;

    public List<AllyCard> allyCards;
    public List<AllyCard> summonCards;
}
