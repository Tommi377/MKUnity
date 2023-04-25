using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System;

public class Combat {
    private List<Enemy> enemies = new List<Enemy>();
    private List<Enemy> alive = new List<Enemy>();
    private List<Enemy> forced = new List<Enemy>();
    private List<Enemy> defeated = new List<Enemy>();
    private List<Enemy> fullyBlocked = new List<Enemy>();

    private Dictionary<Enemy, EnemySO> summonedEnemies = new Dictionary<Enemy, EnemySO>();
    private Dictionary<Enemy, List<EnemyAttack>> unassignedAttacks = new Dictionary<Enemy, List<EnemyAttack>>();

    private List<CombatData> combatCards = new List<CombatData>();
    private List<ItemCard> assignedUnits = new List<ItemCard>();

    public enum States { Start, RangedStart, RangedPlay, BlockStart, BlockPlay, AssignStart, AssignDamage, AttackStart, AttackPlay, Result, End }

    public Player Player { get; private set; }
    public List<Enemy> Targets { get; private set; } = new List<Enemy>();
    public EnemyAttack AttackToHandle { get; private set; }
    public int DamageToAssign { get; private set; } = 0;

    public List<CombatData> CombatCards => combatCards;
    public ReadOnlyCollection<Enemy> Enemies => enemies.AsReadOnly();
    public ReadOnlyCollection<Enemy> Alive => alive.AsReadOnly();
    public ReadOnlyCollection<Enemy> Forced => forced.AsReadOnly();
    public ReadOnlyCollection<Enemy> Defeated => defeated.AsReadOnly();
    public Dictionary<Enemy, EnemySO> SummonedEnemies => summonedEnemies;
    public Dictionary<Enemy, List<EnemyAttack>> UnassignedAttacks => unassignedAttacks;

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

    public Combat(Player player, IEnumerable<Enemy> enemies, IEnumerable<Enemy> forced) {
        Player = player;
        this.enemies.AddRange(enemies);
        this.alive.AddRange(enemies);
        this.forced.AddRange(forced);

        foreach (Enemy enemy in enemies) {
            unassignedAttacks[enemy] = new List<EnemyAttack>();

            if (enemy.Abilities.Contains(EnemyAbilities.Summon)) {
                EnemySO summoned = EntityManager.Instance.GetRandomEnemySO(EntityTypes.Dungeon);
                summonedEnemies.Add(enemy, summoned);
                unassignedAttacks[enemy].AddRange(summoned.Attacks);
            } else {
                unassignedAttacks[enemy].AddRange(enemy.Attacks);
            }
        }

        stateMachine = new CombatStateMachine(this);
    }

    public void Init() {
        CombatAction.OnCombatNextStateClick += CombatAction_OnCombatNextStateClick;
        CombatAction.OnTargetsSelectedClick += CombatAction_OnTargetsSelectedClick;
        CombatAction.OnAttackSelectedClick += CombatAction_OnAttackSelectedClick;
        CombatAction.OnDamageAssignClick += CombatAction_OnDamageAssignClick;
    }

    public void Dispose() {
        CombatAction.OnCombatNextStateClick -= CombatAction_OnCombatNextStateClick;
        CombatAction.OnTargetsSelectedClick -= CombatAction_OnTargetsSelectedClick;
        CombatAction.OnAttackSelectedClick -= CombatAction_OnAttackSelectedClick;
        CombatAction.OnDamageAssignClick -= CombatAction_OnDamageAssignClick;
    }

    public bool EveryEnemyDefeated() => Alive.Count == 0;
    public bool HasUnassignedAttacks() => UnassignedAttacks.Any(item => item.Value.Any());

    public void RemoveTargetEnemies() {
        if (Targets.Count == Enemies.Count) return;

        List<Enemy> removed = enemies.Where(enemy => !Targets.Contains(enemy)).ToList();
        foreach (Enemy enemy in removed) {
            if (!Forced.Contains(enemy)) {
                enemies.Remove(enemy);
                alive.Remove(enemy);
                unassignedAttacks.Remove(enemy);
                Debug.Log("removed");
            } else {
                Debug.LogError("Can't remove forced enemy from combat");
            }
        }
    }

    public void PlayAttackCard(int damage, CombatTypes combatType, CombatElements combatElement, Func<CombatAttack, int> attackFunc = null) {
        PlayCombatCard(new CombatData(damage, combatType, combatElement, attackFunc));
    }
    public void PlayBlockCard(int damage, CombatElements combatElement, Func<CombatBlock, int> blockFunc = null) {
        PlayCombatCard(new CombatData(damage, CombatTypes.Block, combatElement, null, blockFunc));
    }

    public void PlayCombatCard(CombatData combatData) {
        combatCards.Add(combatData);
        OnCombatCardPlayed?.Invoke(this, EventArgs.Empty);
        Debug.Log(combatData);
    }

    public float CalculateDamage(bool rangePhase) {
        CombatAttack combatAttack = new CombatAttack(this, Targets, rangePhase);
        return combatAttack.Calculate();
    }

    public float CalculateBlock() {
        CombatBlock combatBlock = new CombatBlock(this, Targets[0], AttackToHandle);
        return combatBlock.Calculate();
    }

    public int CalculateEnemyArmor() => (new CombatAttack(this, Targets, true)).TotalArmor;

    public int CalculateEnemyAttack() => (new CombatBlock(this, Targets[0], AttackToHandle)).TotalDamage;

    public bool EnemyIsFullyBlocked(Enemy enemy) => fullyBlocked.Contains(enemy);

    public void AttackEnemies(bool rangePhase) {
        if (Targets.Count > 0) {
            CombatAttack combatAttack = new CombatAttack(this, Targets, rangePhase);
            if (combatAttack.IsEnemyDead()) {
                defeated.AddRange(Targets);
                foreach (Enemy target in Targets) {
                    alive.Remove(target);
                }
                Debug.Log("Enemy defeated!! remaining: " + alive.Count);
            }
            Debug.Log("Enemy left with hp: " + combatAttack.TotalArmor);
        } else {
            Debug.Log("Must have targets to attack");
        }

        ResetAfterPlay();
    }

    public void BlockEnemyAttack() {
        Enemy enemy = Targets[0];
        if (unassignedAttacks[enemy].Contains(AttackToHandle)) {
            CombatBlock combatBlock = new CombatBlock(this, enemy, AttackToHandle);
            int damageReceived = combatBlock.PlayerReceivedDamage();

            if (combatBlock.FullyBlocked) {
                Debug.Log("Enemy attack was fully blocked");
                unassignedAttacks[enemy].Remove(AttackToHandle);

                if (unassignedAttacks[enemy].Count == 0) {
                    fullyBlocked.Add(enemy);
                }
            } else {
                Debug.Log("Enemy attack was not fully blocked, take " + damageReceived + " damage");
            }
        }

        ResetAfterPlay();
    }

    public bool CanApply(CardChoice cardChoice) {
        // Check Cumbersome
        if (Targets.Count == 1 && cardChoice.ActionType == ActionTypes.Move && Targets[0].Abilities.Contains(EnemyAbilities.Cumbersome)) {
            return true;
        }

        return false;
    }

    private void AssignDamageToPlayer() {
        Enemy enemy = Targets[0];
        List<EnemyAbilities> Abilities = SummonedEnemies.ContainsKey(enemy) ? SummonedEnemies[enemy].Abilities : enemy.Abilities;

        Player.TakeWound(Abilities.Contains(EnemyAbilities.Poison));
        DamageToAssign -= Player.Armor;

        if (DamageToAssign <= 0) {
            unassignedAttacks[enemy].Remove(AttackToHandle);
        }

        if (Abilities.Contains(EnemyAbilities.Paralyze)) {
            Player.DiscardAllNonWounds();
        }

        OnCombatDamageAssign?.Invoke(this, EventArgs.Empty);
    }

    public void ResetAfterPlay() {
        combatCards.Clear();
        Player.ResetValues();
    }

    public void CombatStateEnter() {
        States state = GetCurrentState();
        if (state == States.RangedStart || state == States.BlockStart || state == States.AssignStart || state == States.AttackStart) {
            AttackToHandle = null;
            Targets.Clear();
        }
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
        assignedUnits.Clear();
        OnCombatEnd?.Invoke(this, new OnCombatResultArgs { combat = this, result = result });
    }

    public void SetTargets(List<Enemy> targets) {
        Targets.Clear();
        foreach (Enemy enemy in targets) {
            if (!Targets.Contains(enemy)) {
                Targets.Add(enemy);
            } else {
                Debug.Log("Enemy already targeted");
            }
        }

        Debug.Log("Targeted " + Targets.Count + " enemies");
    }

    /* ------------------- EVENTS ---------------------- */

    private void CombatAction_OnCombatNextStateClick(object sender, EventArgs e) {
        States prev = GetCurrentState();

        stateMachine.AttemptStateTransfer();

        Debug.Log("Combat statemachine: Old state: " + prev + ". New state: " + GetCurrentState());
    }

    private void CombatAction_OnTargetsSelectedClick(object sender, CombatAction.OnTargetsSelectedClickArgs e) {
        SetTargets(e.Targets);
    }

    private void CombatAction_OnAttackSelectedClick(object sender, CombatAction.OnAttackSelectedClickArgs e) {
        Targets.Clear();
        AttackToHandle = null;

        Targets.Add(e.Target);
        AttackToHandle = e.Attack;
        DamageToAssign = e.Attack.Damage;

        if (e.Target.Abilities.Contains(EnemyAbilities.Brutal)) DamageToAssign *= 2;
    }

    private void CombatAction_OnDamageAssignClick(object sender, CombatAction.OnDamageAssignClickArgs e) {
        AssignDamageToPlayer();
    }
}

public class CombatData {
    public int Damage;
    public CombatTypes CombatType;
    public CombatElements CombatElement;
    public Func<CombatAttack, int> CombatAttackModifier;
    public Func<CombatBlock, int> CombatBlockModifier;

    public CombatData(int damage, CombatTypes combatType, CombatElements combatElement, Func<CombatAttack, int> attackFunc = null, Func<CombatBlock, int> blockFunc = null) {
        Damage = damage;
        CombatType = combatType;
        CombatElement = combatElement;
        CombatAttackModifier = attackFunc;
        CombatBlockModifier = blockFunc;
    }

    public override string ToString() {
        return "Combat data: " + Damage + "dmg " + CombatElement + " " + CombatType + " attack";
    }
}

public class CombatResult {
    public Player Player;
    public List<Enemy> Alive;
    public List<Enemy> Defeated;
    public int Fame;
    public int Reputation;
}

public enum CombatPhases {
    Range,
    Block,
    AssignDamage,
    Attack,
    End
}

public enum CombatTypes {
    Normal,
    Range,
    Siege,
    Block
}
public enum CombatElements {
    Physical,
    Fire,
    Ice,
    ColdFire
}