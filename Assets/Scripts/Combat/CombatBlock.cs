using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatBlock {
    public Player Player { get; private set; }
    public Enemy Enemy { get; private set; }
    public EnemyAttack Attack { get; private set; }
    public List<CombatData> CombatCards { get; private set; }

    public int TotalDamage { get; private set; } = 0;
    public bool ArcaneImmunity { get; private set; } = false;

    private bool combatPrevented = false;

    public CombatBlock(Combat combat, Enemy enemy, EnemyAttack attack) {
        Player = combat.Player;
        Enemy = enemy;
        Attack = attack;
        CombatCards = combat.CombatCards;

        // Modify this attack depending on modifiers
        TotalDamage = Attack.Damage;

        if (Enemy.Abilities.Contains(EnemyAbilities.Swift)) TotalDamage *= 2;
        if (Enemy.Abilities.Contains(EnemyAbilities.ArcaneImmunity)) ArcaneImmunity = true;
    }
    public int PlayerReceivedDamage() {
        return FullyBlocked ? 0 : TotalDamage;
    }

    public bool FullyBlocked { get => combatPrevented || TotalDamage <= Calculate(); }

    public void PreventEnemyAttack() => combatPrevented = true;

    public float Calculate() {
        float cumBlock = 0;
        foreach (CombatData combatCard in CombatCards) {
            float multiplier = 1;
            float block = 0;

            if (combatCard.CombatType != CombatTypes.Block) {
                continue;
            }

            block += combatCard.Damage;
            if (combatCard.CombatBlockModifier != null) {
                block += combatCard.CombatBlockModifier(this);
            }

            if (
                Attack.Element == CombatElements.Ice && (combatCard.CombatElement == CombatElements.Physical || combatCard.CombatElement == CombatElements.Ice) ||
                Attack.Element == CombatElements.Fire && (combatCard.CombatElement == CombatElements.Physical || combatCard.CombatElement == CombatElements.Fire) ||
                Attack.Element == CombatElements.ColdFire && !(combatCard.CombatElement == CombatElements.ColdFire)
            ) {
                multiplier *= 0.5f;
            }

            cumBlock += (int)(block * multiplier);
        }

        Debug.Log(Player.Movement);

        if (Enemy.Abilities.Contains(EnemyAbilities.Cumbersome)) {
            cumBlock += Player.Movement;
        }

        return cumBlock;
    }
}
