using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatAttack {
    public Player Player { get; private set; }
    public List<Enemy> Enemies { get; private set; }

    public List<CombatData> CombatCards { get; private set; }
    public bool RangePhase { get; private set; } = false;
    public bool Fortified { get; private set; } = false;
    public bool DoubleFortified { get; private set; } = false;

    public int TotalArmor { get; private set; } = 0;

    public Dictionary<CombatElements, bool> Resistances { get; private set; } = new Dictionary<CombatElements, bool> {
        { CombatElements.Physical, false },
        { CombatElements.Ice, false },
        { CombatElements.Fire, false },
        { CombatElements.ColdFire, false },
    };

    public CombatAttack(Combat combat, List<Enemy> enemies, bool rangePhase, bool fortifiedSite = false) {
        Player = combat.Player;
        Enemies = enemies;
        CombatCards = combat.CombatCards;
        RangePhase = rangePhase;

        foreach (Enemy enemy in enemies) {
            TotalArmor += enemy.Armor;
            if (enemy.Abilities.Contains(EnemyAbilities.Elusive) && !combat.EnemyIsFullyBlocked(enemy)) {
                TotalArmor += enemy.Armor;
            }

            if (!Fortified && enemy.Abilities.Contains(EnemyAbilities.Fortified)) {
                Fortified = true;
                if (fortifiedSite) DoubleFortified = true;
            }

            enemy.Resistances.ForEach(resistance => Resistances[resistance] = true);
        }

        // Set coldfire resistance
        if (Resistances[CombatElements.Ice] && Resistances[CombatElements.Ice]) {
            Resistances[CombatElements.ColdFire] = true;
        }

        if (!Fortified) {
            // Fortified if at least 1 enemy is not unfortified
            Fortified = fortifiedSite && !enemies.All(e => e.Abilities.Contains(EnemyAbilities.Unfortified));
        }
    }

    private int GetDamage(CombatData combatCard) {
        float multiplier = Resistances[combatCard.CombatElement] ? 0.5f : 1;
        int damage = combatCard.Damage;

        if (combatCard.CombatAttackModifier != null) {
            damage += combatCard.CombatAttackModifier(this);
        }

        return (int)(damage * multiplier);
    }

    public int Calculate() {
        int damage = 0;
        foreach (CombatData combatCard in CombatCards) {
            if (combatCard.CombatType == CombatTypes.Block) {
                continue;
            }

            if (RangePhase) {
                if (combatCard.CombatType == CombatTypes.Range && !Fortified || combatCard.CombatType == CombatTypes.Siege && !DoubleFortified) {
                    damage += GetDamage(combatCard);
                }
            } else {
                damage += GetDamage(combatCard);
            }
        }
        return damage;
    }

    public bool IsEnemyDead() {
        return TotalArmor <= Calculate();
    }
}