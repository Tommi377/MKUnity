using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System;

public class Combat {
    private List<Enemy> enemies = new List<Enemy>();
    private List<Enemy> alive = new List<Enemy>();
    private List<Enemy> defeated = new List<Enemy>();
    private Dictionary<Enemy, List<EnemyAttack>> unassignedAttacks = new Dictionary<Enemy, List<EnemyAttack>>();

    private List<CombatData> combatCards = new List<CombatData>();

    public enum States { Start, RangedStart, RangedPlay, BlockStart, BlockPlay, AssignStart, AssignDamage, AttackStart, AttackPlay, Result, End }

    public Player Player { get; private set; }
    public List<Enemy> Targets { get; private set; } = new List<Enemy>();
    public EnemyAttack AttackToHandle { get; private set; }
    public int DamageToAssign { get; private set; } = 0;

    public ReadOnlyCollection<CombatData> CombatCards => combatCards.AsReadOnly();
    public ReadOnlyCollection<Enemy> Enemies { get => enemies.AsReadOnly(); }
    public ReadOnlyCollection<Enemy> Alive { get => alive.AsReadOnly(); }
    public ReadOnlyCollection<Enemy> Defeated { get => defeated.AsReadOnly(); }
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

    public Combat(Player player, IEnumerable<Enemy> enemies) {
        Player = player;
        this.enemies.AddRange(enemies);
        this.alive.AddRange(enemies);

        foreach (Enemy enemy in enemies) {
            if (!unassignedAttacks.ContainsKey(enemy)) {
                unassignedAttacks[enemy] = new List<EnemyAttack>();
            }
            unassignedAttacks[enemy].AddRange(enemy.Attacks);
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

    public int CalculateDamage(bool rangePhase) {
        CombatAttack combatAttack = new CombatAttack(Player, Targets, combatCards, rangePhase);
        return combatAttack.Calculate();
    }

    public int CalculateBlock() {
        CombatBlock combatBlock = new CombatBlock(Player, Targets[0], AttackToHandle, combatCards);
        return combatBlock.Calculate();
    }

    public int CalculateEnemyArmor() {
        return Targets.Sum(enemy => enemy.Armor);
    }

    public void AttackEnemies(bool rangePhase) {
        if (Targets.Count > 0) {
            CombatAttack combatAttack = new CombatAttack(Player, Targets, combatCards, rangePhase);
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

        combatCards.Clear();
    }

    public void BlockEnemyAttack() {
        Enemy enemy = Targets[0];
        if (unassignedAttacks[enemy].Contains(AttackToHandle)) {
            CombatBlock combatBlock = new CombatBlock(Player, enemy, AttackToHandle, combatCards);
            int damageReceived = combatBlock.PlayerReceivedDamage();

            if (combatBlock.FullyBlocked) {
                Debug.Log("Enemy attack was fully blocked");
                unassignedAttacks[enemy].Remove(AttackToHandle);
            } else {
                Debug.Log("Enemy attack was not fully blocked, take " + damageReceived + " damage");
            }
        }

        combatCards.Clear();
    }

    private void AssignDamageToPlayer() {
        // TODO: Brutal ability and stuff
        Player.TakeWounds(1);
        DamageToAssign -= Player.Armor;

        if (DamageToAssign <= 0) {
            unassignedAttacks[Targets[0]].Remove(AttackToHandle);
        }

        OnCombatDamageAssign?.Invoke(this, EventArgs.Empty);
    }

    private void AssignDamageToUnit(UnitCard unit) {
        unit.WoundUnit();
        DamageToAssign -= unit.Armor;

        if (DamageToAssign <= 0) {
            unassignedAttacks[Targets[0]].Remove(AttackToHandle);
        }

        OnCombatDamageAssign?.Invoke(this, EventArgs.Empty);
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
        OnCombatEnd?.Invoke(this, new OnCombatResultArgs { combat = this, result = result });
    }

    /* ------------------- EVENTS ---------------------- */

    private void CombatAction_OnCombatNextStateClick(object sender, EventArgs e) {
        States prev = GetCurrentState();

        stateMachine.AttemptStateTransfer();

        Debug.Log("Old state: " + prev + ". New state: " + GetCurrentState());
    }

    private void CombatAction_OnTargetsSelectedClick(object sender, CombatAction.OnTargetsSelectedClickArgs e) {
        Targets.Clear();
        foreach (Enemy enemy in e.Targets) {
            if (!Targets.Contains(enemy)) {
                Targets.Add(enemy);
            } else {
                Debug.Log("Enemy already targeted");
            }
        }

        Debug.Log("Targeted " + Targets.Count + " enemies");
    }

    private void CombatAction_OnAttackSelectedClick(object sender, CombatAction.OnAttackSelectedClickArgs e) {
        Targets.Clear();
        AttackToHandle = null;

        Targets.Add(e.Target);
        AttackToHandle = e.Attack;
        DamageToAssign = e.Attack.Damage;
    }

    private void CombatAction_OnDamageAssignClick(object sender, CombatAction.OnDamageAssignClickArgs e) {
        if (e.ChoiceId == -1) {
            AssignDamageToPlayer();
        } else {
            UnitCard unit = Player.GetUnits()[e.ChoiceId];
            AssignDamageToUnit(unit);
        }
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