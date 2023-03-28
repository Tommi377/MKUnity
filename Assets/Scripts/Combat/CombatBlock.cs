using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatBlock {
    public Player Player { get; private set; }
    public Enemy Enemy { get; private set; }
    public EnemyAttack Attack { get; private set; }
    public List<CombatData> CombatCards { get; private set; }

    public int TotalDamage { get; private set; } = 0;

    private bool combatPrevented = false;

    public CombatBlock(Player player, Enemy enemy, EnemyAttack attack, List<CombatData> combatCards) {
        Player = player;
        Enemy = enemy;
        Attack = attack;
        CombatCards = combatCards;

        // Modify this attack depending on modifiers
        TotalDamage = Attack.Damage;
    }
    public int PlayerReceivedDamage() {
        return FullyBlocked ? 0 : TotalDamage;
    }

    public bool FullyBlocked { get => combatPrevented || TotalDamage <= Calculate(); }

    public void PreventEnemyAttack() => combatPrevented = true;

    public int Calculate() {
        int block = 0;
        foreach (CombatData combatCard in CombatCards) {
            if (combatCard.CombatType != CombatTypes.Block) {
                continue;
            }

            // TODO: add resistance and other checking
            block += combatCard.Damage;
            if (combatCard.CombatBlockModifier != null) {
                block += combatCard.CombatBlockModifier(this);
            }
        }

        return block;
    }
}
