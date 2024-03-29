using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatBlock {
    public Player Player { get; private set; }
    public Enemy Enemy { get; private set; }
    public List<EnemyAbilities> Abilities { get; private set; }
    public EnemyAttack Attack { get; private set; }
    public List<CombatData> CombatCards { get; private set; }

    public int TotalDamage { get; private set; } = 0;
    public bool ArcaneImmunity { get; private set; } = false;

    private bool combatPrevented = false;

    public CombatBlock(Combat combat, Enemy enemy, EnemyAttack attack) {
        Player = combat.Player;
        Enemy = enemy;
        Abilities = combat.SummonedEnemies.ContainsKey(enemy) ? combat.SummonedEnemies[enemy].Abilities : enemy.Abilities;
        Attack = attack;
        CombatCards = combat.CombatCards;

        TotalDamage = Attack.Damage;

        if (Abilities.Contains(EnemyAbilities.Swift)) TotalDamage *= 2;
        if (Abilities.Contains(EnemyAbilities.ArcaneImmunity)) ArcaneImmunity = true;
    }
    public int PlayerReceivedDamage() {
        return FullyBlocked ? 0 : TotalDamage;
    }

    public bool FullyBlocked { get => combatPrevented || TotalDamage <= Calculate(); }

    public void PreventEnemyAttack() => combatPrevented = true;

    public float Calculate() {
        float cumBlock = 0;
        foreach (CombatData combatCard in CombatCards) {
            if (combatCard.CombatType != CombatTypes.Block) {
                continue;
            }

            cumBlock += GetBlock(combatCard);
        }

        Debug.Log(Player.Movement);

        if (Abilities.Contains(EnemyAbilities.Cumbersome)) {
            cumBlock += Player.Movement;
        }

        return cumBlock;
    }

    private float GetBlock(CombatData combatCard) {
        float multiplier = 1;
        float block = 0;

        block += combatCard.Damage;
        if (combatCard.CombatBlockModifier != null) {
            block += combatCard.CombatBlockModifier(this);
        }

        // Resistance check
        if (
            Attack.Element == CombatElements.Ice && (combatCard.CombatElement == CombatElements.Physical || combatCard.CombatElement == CombatElements.Ice) ||
            Attack.Element == CombatElements.Fire && (combatCard.CombatElement == CombatElements.Physical || combatCard.CombatElement == CombatElements.Fire) ||
            Attack.Element == CombatElements.ColdFire && !(combatCard.CombatElement == CombatElements.ColdFire)
        ) {
            multiplier *= 0.5f;
        }

        return block * multiplier;
    }
}
