using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardType
{
    Champion,
    Minion,
    Summon,
    Magic,
    Equipment
}

public enum CardClass
{
    Warrior,
    Archer,
    Mage
}

public enum CardElement
{
    Basic,
    Fire,
    Water,
    Wind,
    Earth
}

public enum CardKeyWords
{
    Stealth,
    Ranged,
    Multiattack,
    Haste
}

public enum EffectTrigger
{
    OnStartOfTurn,
    OnStartOfYourTurn,
    OnStartOfEnemyTurn,
    OnEndOfTurn,
    OnEndOfYourTurn,
    OnEndOfEnemyTurn,
    OnEquip,
    OnUnequip,
    OnEnter,
    OnDeath,
    OnKill,
    OnAttack,
    OnInflictDamage,
    OnDealtDamage,
    OnHealed
}

public enum EffectTarget
{
    Self,
    Everyone,
    Allies,
    Enemies,
    Player,
    EnemyPlayer,
    BothPlayers,
    AllyMinions,
    EnemyMinions
}

public enum Effect
{
    Draw,
    Damage,
    Heal
}

public enum CardPlaySuccess
{
    Success,
    NotEnoughRoom,
    NotEnoughMana,
    NotThePlayersTurn,
    Fail
}

public enum CardBattleSuccess
{
    Success,
    Fail,
    ZeroAttack,
    CannotAttackYet,
    NoAttacksLeft,
    FirstObjectIncorrect,
    SecondObjectIncorrect
}