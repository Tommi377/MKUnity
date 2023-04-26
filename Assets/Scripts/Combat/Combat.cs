using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using static UnityEngine.GraphicsBuffer;

public class Combat {
    private List<Enemy> enemies = new List<Enemy>();
    private List<Enemy> alive = new List<Enemy>();
    private List<Enemy> defeated = new List<Enemy>();
    private List<Enemy> fullyBlocked = new List<Enemy>();

    private List<CombatCard> combatCards = new List<CombatCard>();

    private Dictionary<Enemy, EnemySO> summonedEnemies = new Dictionary<Enemy, EnemySO>();

    private Dictionary<Enemy, int> enemyArmorMap = new Dictionary<Enemy, int>();
    private Dictionary<Enemy, Dictionary<EnemyAttack, int>> enemyAttackMap = new Dictionary<Enemy, Dictionary<EnemyAttack, int>>();

    public enum States { Start, Block, Assign, Attack, Result, End }

    public Player Player { get; private set; }
    public Enemy TargetEnemy { get; private set; }
    public EnemyAttack TargetAttack { get; private set; }
    public int DamageToAssign { get; private set; } = 0;

    public List<CombatCard> CombatCards => combatCards;
    public ReadOnlyCollection<Enemy> Enemies => enemies.AsReadOnly();
    public ReadOnlyCollection<Enemy> Alive => alive.AsReadOnly();
    public ReadOnlyCollection<Enemy> Defeated => defeated.AsReadOnly();
    public Dictionary<Enemy, EnemySO> SummonedEnemies => summonedEnemies;
    public Dictionary<Enemy, Dictionary<EnemyAttack, int>> UnassignedAttacks => enemyAttackMap;

    public States GetCurrentState() => stateMachine.GetCurrentState();

    private CombatResult result;
    private CombatStateMachine stateMachine;

    /* EVENT DEFINITIONS - START */
    public static event EventHandler OnCombatDamageAssign;
    public static event EventHandler<OnCombatResultArgs> OnCombatEnd;
    public static event EventHandler<OnCombatResultArgs> OnGenerateCombatResult;
    public class OnCombatResultArgs : EventArgs {
        public Combat combat;
        public CombatResult result;
    }
    public static event EventHandler OnCombatCardPlayed;
    public static event EventHandler<OnCombatStateEnterArgs> OnCombatStateEnter;
    public class OnCombatStateEnterArgs : EventArgs {
        public Combat Combat;
        public States State;
    }

    public static void ResetStaticData() {
        OnCombatDamageAssign = null;
        OnCombatEnd = null;
        OnGenerateCombatResult = null;
        OnCombatCardPlayed = null;
        OnCombatStateEnter = null;
    }
    /* EVENT DEFINITIONS - END */

    public Combat(Player player, IEnumerable<Enemy> enemies) {
        Player = player;
        this.enemies.AddRange(enemies);
        this.alive.AddRange(enemies);

        foreach (Enemy enemy in enemies) {
            enemyAttackMap[enemy] = new Dictionary<EnemyAttack, int>();

            if (enemy.Abilities.Contains(EnemyAbilities.Summon)) {
                EnemySO summoned = EntityManager.Instance.GetRandomEnemySO(EntityTypes.Dungeon);
                summonedEnemies.Add(enemy, summoned);

                summoned.Attacks.ForEach(attack => enemyAttackMap[enemy][attack] = attack.Damage);
            } else {
                enemy.Attacks.ForEach(attack => enemyAttackMap[enemy][attack] = attack.Damage);
            }

            // Track enemy armor
            enemyArmorMap.Add(enemy, enemy.Armor);
        }

        stateMachine = new CombatStateMachine(this);
    }

    public void Init() {
        CombatAction.OnCombatNextStateClick += CombatAction_OnCombatNextStateClick;

        CombatAction.OnCombatSelectTargetClick += CombatAction_OnCombatSelectTargetClick;
    }

    public void Dispose() {
        CombatAction.OnCombatNextStateClick -= CombatAction_OnCombatNextStateClick;

        CombatAction.OnCombatSelectTargetClick -= CombatAction_OnCombatSelectTargetClick;
    }

    public bool EveryEnemyDefeated() => Alive.Count == 0;
    public bool HasUnassignedAttacks() => enemyAttackMap.Count > 0;

    public int GetEnemyArmor(Enemy enemy) => enemyArmorMap[enemy];
    public int GetEnemyAttack(Enemy enemy, EnemyAttack attack) => enemyAttackMap[enemy][attack];

    public void PlayAttackCard(int damage, CombatElements combatElement, bool fast = false, Func<CombatAttack, int> attackFunc = null) {
        if (TargetEnemy != null) {
            if (GetCurrentState() == States.Block && !fast) {
                Debug.Log("Can't play non-fast attack cards during block phase");
                return;
            }

            AttackCard attackCard = new AttackCard(damage, combatElement, fast, attackFunc);
            CombatAttack combatAttack = new CombatAttack(this, TargetEnemy, attackCard);

            combatCards.Add(attackCard);

            enemyArmorMap[TargetEnemy] -= (int)combatAttack.Calculate();
            if (enemyArmorMap[TargetEnemy] <= 0) {
                defeated.Add(TargetEnemy);
                fullyBlocked.Add(TargetEnemy);
                alive.Remove(TargetEnemy);
                Debug.Log("Enemy defeated!! remaining: " + alive.Count);
            }

            OnCombatCardPlayed?.Invoke(this, EventArgs.Empty);
        }
    }
    public void PlayBlockCard(int damage, CombatElements combatElement, Func<CombatBlock, int> blockFunc = null) {
        if (TargetEnemy != null && TargetAttack != null) {
            BlockCard blockCard = new BlockCard(damage, combatElement, blockFunc);
            CombatBlock combatBlock = new CombatBlock(this, TargetEnemy, TargetAttack, blockCard);

            combatCards.Add(blockCard);

            enemyAttackMap[TargetEnemy][TargetAttack] -= (int)combatBlock.Calculate();

            if (enemyAttackMap[TargetEnemy][TargetAttack] <= 0) {
                enemyAttackMap[TargetEnemy].Remove(TargetAttack);

                if (enemyAttackMap[TargetEnemy].Count == 0) {
                    fullyBlocked.Add(TargetEnemy);
                    enemyAttackMap.Remove(TargetEnemy);
                    Debug.Log("Enemy fully blocked!! remaining: " + alive.Count);
                }

                Debug.Log("Attack fully blocked");
            }

            OnCombatCardPlayed?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool EnemyIsFullyBlocked(Enemy enemy) => fullyBlocked.Contains(enemy);

    public bool CanApply(CardChoice cardChoice) {
        // Check Cumbersome
        if (TargetEnemy != null && cardChoice.ActionType == ActionTypes.Move && TargetEnemy.Abilities.Contains(EnemyAbilities.Cumbersome)) {
            return true;
        }

        return false;
    }

    private void AssignDamageToPlayer() {
        Enemy enemy = TargetEnemy;
        List<EnemyAbilities> Abilities = SummonedEnemies.ContainsKey(enemy) ? SummonedEnemies[enemy].Abilities : enemy.Abilities;

        Player.TakeWound(Abilities.Contains(EnemyAbilities.Poison));
        DamageToAssign -= Player.Armor;

        if (DamageToAssign <= 0) {
            enemyAttackMap[enemy].Remove(TargetAttack);
        }

        if (Abilities.Contains(EnemyAbilities.Paralyze)) {
            Player.DiscardAllNonWounds();
        }

        OnCombatDamageAssign?.Invoke(this, EventArgs.Empty);
    }

    public void CombatStateEnter() {
        States state = GetCurrentState();
        TargetEnemy = null;
        TargetAttack = null;

        OnCombatStateEnter?.Invoke(this, new OnCombatStateEnterArgs { Combat = this, State = GetCurrentState() });
    }

    public void GenerateCombatResult() {
        result = new CombatResult {
            Player = Player,
            Alive = alive,
            Defeated = defeated,
            Fame = defeated.Sum(enemy => enemy.Fame),
            Reputation = 0 // TODO: Reputation calculation
        };
        OnGenerateCombatResult?.Invoke(this, new OnCombatResultArgs { combat = this, result = result });
    }

    public void CombatEnd() {
        OnCombatEnd?.Invoke(this, new OnCombatResultArgs { combat = this, result = result });
    }

    public void SetTargets(Enemy target) {
        TargetEnemy = target;
    }

    /* ------------------- EVENTS ---------------------- */

    private void CombatAction_OnCombatNextStateClick(object sender, EventArgs e) {
        States prev = GetCurrentState();

        stateMachine.AttemptStateTransfer();

        Debug.Log("Combat statemachine: Old state: " + prev + ". New state: " + GetCurrentState());
    }

    private void CombatAction_OnCombatSelectTargetClick(object sender, CombatAction.OnCombatSelectTargetClickArgs e) {
        TargetEnemy = e.Enemy;
        TargetAttack = e.Attack;
    }
}

public abstract class CombatCard { }

public class AttackCard : CombatCard {
    public int Damage;
    public CombatElements CombatElement;
    public bool Fast;
    public Func<CombatAttack, int> CombatAttackModifier;

    public AttackCard(int damage, CombatElements combatElement, bool fast, Func<CombatAttack, int> attackFunc = null) {
        Damage = damage;
        CombatElement = combatElement;
        Fast = fast;
        CombatAttackModifier = attackFunc;
    }
}

public class BlockCard : CombatCard {
    public int Block;
    public CombatElements CombatElement;
    public Func<CombatBlock, int> CombatBlockModifier;

    public BlockCard(int damage, CombatElements combatElement, Func<CombatBlock, int> blockFunc = null) {
        Block = damage;
        CombatElement = combatElement;
        CombatBlockModifier = blockFunc;
    }
}

public class CombatResult {
    public Player Player;
    public List<Enemy> Alive;
    public List<Enemy> Defeated;
    public int Fame;
    public int Reputation;
}

public enum CombatTypes {
    Normal,
    Fast,
    Block
}
public enum CombatElements {
    Physical,
    Fire,
    Ice,
    ColdFire
}