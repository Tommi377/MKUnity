using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAttack {
    public Player Player { get; private set; }
    public List<Enemy> Enemies { get; private set; }

    public List<CombatData> CombatCards { get; private set; }
    public bool RangePhase { get; private set; }

    public int TotalArmor { get; private set; } = 0;

    public CombatAttack(Player player, List<Enemy> enemies, List<CombatData> combatCards, bool rangePhase) {
        Player = player;
        Enemies = enemies;
        CombatCards = combatCards;
        RangePhase = rangePhase;

        Enemies.ForEach(enemy => TotalArmor += enemy.Armor);
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
            // TODO: add siege checking
            if (RangePhase) {
                if (combatCard.CombatType == CombatTypes.Range || combatCard.CombatType == CombatTypes.Siege) {
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