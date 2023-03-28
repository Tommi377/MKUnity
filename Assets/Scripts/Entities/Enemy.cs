using System;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyAbilities {
    Summon,
    Swift,
    Brutal,
    Poison,
    Paralyze,
    Assassination,
    Cumbersome,
    Vampiric,
    Fortified,
    Unfortified,
    Elusive,
    ArcaneImmunity,
    Defend
}

public class Enemy : Entity {
    public EnemySO EnemySO;

    public string Name => EnemySO.Name;
    public int Armor => EnemySO.Armor;
    public int Fame => EnemySO.Fame;
    public List<CombatElements> Resistances => EnemySO.Resistances;
    public override EntityTypes EntityType => EnemySO.EntityType;
    public List<EnemyAttack> Attacks => EnemySO.Attacks;
    public List<EnemyAbilities> Abilities => EnemySO.Abilities;
    public Sprite sprite => EnemySO.Sprite;
    public bool Rampaging { get; protected set; }

    public event EventHandler OnInit;

    public void Init(EnemySO enemySO) {
        this.EnemySO = enemySO;
        OnInit?.Invoke(this, EventArgs.Empty);
        OnInit = null;
    }

    public void SetRampaging(bool rampaging = true) {
        this.Rampaging = rampaging;
    }
}