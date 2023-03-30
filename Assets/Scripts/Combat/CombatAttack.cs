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
        }

        if (!Fortified) {
            // Fortified if at least 1 enemy is not unfortified
            Fortified = fortifiedSite && !enemies.All(e => e.Abilities.Contains(EnemyAbilities.Unfortified));
        }
    }

    private int GetDamage(CombatData combatCard) {
        int damage = combatCard.Damage;
        if (combatCard.CombatAttackModifier != null) {
            damage += combatCard.CombatAttackModifier(this);
        }
        return damage;
    }

    public int Calculate() {
        int damage = 0;
        foreach (CombatData combatCard in CombatCards) {
            if (combatCard.CombatType == CombatTypes.Block) {
                continue;
            }

            // TODO: add resistance checking
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