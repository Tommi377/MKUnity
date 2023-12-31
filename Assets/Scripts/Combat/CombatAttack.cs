using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatAttack {
    public Player Player { get; private set; }
    public Enemy Enemy { get; private set; }
    public AttackCard AttackCard { get; private set; }

    public bool Fortified { get; private set; } = false;
    public bool ArcaneImmunity { get; private set; } = false;

    public int TotalArmor { get; private set; } = 0;

    public Dictionary<CombatElements, bool> Resistances { get; private set; } = new Dictionary<CombatElements, bool> {
        { CombatElements.Physical, false },
        { CombatElements.Ice, false },
        { CombatElements.Fire, false },
        { CombatElements.ColdFire, false },
    };

    public CombatAttack(Combat combat, Enemy enemy, AttackCard attackCard) {
        Player = combat.Player;
        Enemy = enemy;
        AttackCard = attackCard;

        TotalArmor += enemy.Armor;
        if (enemy.Abilities.Contains(EnemyAbilities.Elusive) && !combat.EnemyIsFullyBlocked(enemy)) {
            TotalArmor += enemy.Armor;
        }

        if (enemy.Abilities.Contains(EnemyAbilities.Fortified)) {
            Fortified = true;
        }

        if (!ArcaneImmunity && enemy.Abilities.Contains(EnemyAbilities.Fortified)) {
            ArcaneImmunity = true;
        }

        enemy.Resistances.ForEach(resistance => Resistances[resistance] = true);

        // Set coldfire resistance
        if (Resistances[CombatElements.Ice] && Resistances[CombatElements.Ice]) {
            Resistances[CombatElements.ColdFire] = true;
        }
    }

    public float Calculate() {
        float multiplier = Resistances[AttackCard.CombatElement] ? 0.5f : 1;
        float damage = AttackCard.Damage;

        if (AttackCard.CombatAttackModifier != null) {
            damage += AttackCard.CombatAttackModifier(this);
        }

        return damage * multiplier;
    }
}