using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatBlock {
    public Player Player { get; private set; }
    public Enemy Enemy { get; private set; }
    public List<EnemyAbilities> Abilities { get; private set; }
    public EnemyAttack Attack { get; private set; }
    public BlockCard BlockCard { get; private set; }

    public int TotalDamage { get; private set; } = 0;
    public bool ArcaneImmunity { get; private set; } = false;

    public CombatBlock(Combat combat, Enemy enemy, EnemyAttack attack, BlockCard blockCard) {
        Player = combat.Player;
        Enemy = enemy;
        Abilities = combat.SummonedEnemies.ContainsKey(enemy) ? combat.SummonedEnemies[enemy].Abilities : enemy.Abilities;
        Attack = attack;
        BlockCard = blockCard;

        TotalDamage = Attack.Damage;

        if (Abilities.Contains(EnemyAbilities.Swift)) TotalDamage *= 2;
        if (Abilities.Contains(EnemyAbilities.ArcaneImmunity)) ArcaneImmunity = true;
    }

    public float Calculate() {
        float multiplier = 1;
        float block = BlockCard.Block;

        if (BlockCard.CombatBlockModifier != null) {
            block += BlockCard.CombatBlockModifier(this);
        }

        // Resistance check
        if (
            Attack.Element == CombatElements.Ice && (BlockCard.CombatElement == CombatElements.Physical || BlockCard.CombatElement == CombatElements.Ice) ||
            Attack.Element == CombatElements.Fire && (BlockCard.CombatElement == CombatElements.Physical || BlockCard.CombatElement == CombatElements.Fire) ||
            Attack.Element == CombatElements.ColdFire && !(BlockCard.CombatElement == CombatElements.ColdFire)
        ) {
            multiplier *= 0.5f;
        }

        return block * multiplier;
    }
}
