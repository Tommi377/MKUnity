using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System;

public class Combat {
    private StateMachine stateMachine;
    private List<Enemy> enemies = new List<Enemy>();
    private List<Enemy> alive = new List<Enemy>();
    private List<Enemy> defeated = new List<Enemy>();
    private Dictionary<Enemy, List<EnemyAttack>> unassignedAttacks = new Dictionary<Enemy, List<EnemyAttack>>();

    private List<CombatData> combatCards = new List<CombatData>();

    public enum States { Start, RangedStart, RangedPlay, BlockStart, BlockPlay, AssignStart, AssignDamage, AttackStart, AttackPlay, Result, End }

    public Player Player { get; private set; }
    public bool GoNextState { get; private set; } = false;
    public List<Enemy> Targets { get; private set; } = new List<Enemy>();
    public EnemyAttack AttackToHandle { get; private set; }
    public int DamageToAssign { get; private set; } = 0;

    public ReadOnlyCollection<CombatData> CombatCards => combatCards.AsReadOnly();
    public ReadOnlyCollection<Enemy> Enemies { get => enemies.AsReadOnly(); }
    public ReadOnlyCollection<Enemy> Alive { get => alive.AsReadOnly(); }
    public ReadOnlyCollection<Enemy> Defeated { get => defeated.AsReadOnly(); }
    public Dictionary<Enemy, List<EnemyAttack>> UnassignedAttacks => unassignedAttacks;

    public States GetCurrentState() => (States)stateMachine.GetCurrentState().ID;

    private CombatResult result;

    /* STATE MACHINE - END */

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

        StateMachineInit();
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

    private void AttackEnemies(List<Enemy> targets, bool rangePhase) {
        if (targets.Count > 0) {
            CombatAttack combatAttack = new CombatAttack(Player, targets, combatCards, rangePhase);
            if (combatAttack.IsEnemyDead()) {
                defeated.AddRange(targets);
                foreach (Enemy target in targets) {
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

    private void BlockEnemyAttack(Enemy enemy, EnemyAttack attack) {
        if (unassignedAttacks[enemy].Contains(attack)) {
            CombatBlock combatBlock = new CombatBlock(Player, enemy, attack, combatCards);
            int damageReceived = combatBlock.PlayerReceivedDamage();

            if (combatBlock.FullyBlocked) {
                Debug.Log("Enemy attack was fully blocked");
                unassignedAttacks[enemy].Remove(attack);
            } else {
                Debug.Log("Enemy attack was not fully blocked, take " + damageReceived + " damage");
            }
        }

        combatCards.Clear();
    }

    private void AssignDamageToPlayer(Enemy enemy, EnemyAttack attack) {
        // TODO: Brutal ability and stuff
        Player.TakeWounds(1);
        DamageToAssign -= Player.Armor;

        if (DamageToAssign <= 0) {
            unassignedAttacks[enemy].Remove(attack);
        }

        OnCombatDamageAssign?.Invoke(this, EventArgs.Empty);
    }

    public void OnStateEnter() {
        GoNextState = false;
        States state = GetCurrentState();
        if (state == States.RangedStart || state == States.BlockStart || state == States.AssignStart || state == States.AttackStart) {
            AttackToHandle = null;
            Targets.Clear();
        }

        OnCombatStateEnter?.Invoke(this, new OnCombatStateEnterArgs { Combat = this, State = GetCurrentState() });
    }

    private void ExitRangedPlay() {
        AttackEnemies(Targets, true);
    }

    private void ExitAttackPlay() {
        AttackEnemies(Targets, false);
    }

    private void ExitBlockPlay() {
        BlockEnemyAttack(Targets[0], AttackToHandle);
    }

    private void EnterResult() {
        result = new CombatResult {
            Player = Player,
            Alive = alive,
            Defeated = defeated,
            Fame = defeated.Sum(enemy => enemy.Fame),
            Reputation = 0 // TODO: Reputation calculation
        };
        OnGenerateCombatResult?.Invoke(this, new OnCombatResultArgs { combat = this, result = result });
    }

    private void EnterEnd() {
        OnCombatEnd?.Invoke(this, new OnCombatResultArgs { combat = this, result = result });
    }

    private void StateMachineInit() {
        CombatDefaultAction defaultAction = new CombatDefaultAction(this);

        State Start = new State((int)States.Start, defaultAction);
        State RangedStart = new State((int)States.RangedStart, defaultAction);
        State RangedPlay = new State((int)States.RangedPlay, defaultAction);
        State BlockStart = new State((int)States.BlockStart, defaultAction);
        State BlockPlay = new State((int)States.BlockPlay, defaultAction);
        State AssignStart = new State((int)States.AssignStart, defaultAction);
        State AssignDamage = new State((int)States.AssignDamage, defaultAction);
        State AttackStart = new State((int)States.AttackStart, defaultAction);
        State AttackPlay = new State((int)States.AttackPlay, defaultAction);
        State Result = new State((int)States.Result, defaultAction);
        State End = new State((int)States.End, defaultAction);

        RangedPlay.AddAction(OnStateExitAction.Create(ExitRangedPlay));
        BlockPlay.AddAction(OnStateExitAction.Create(ExitBlockPlay));
        AttackPlay.AddAction(OnStateExitAction.Create(ExitAttackPlay));
        Result.AddAction(OnStateEnterAction.Create(EnterResult));
        End.AddAction(OnStateEnterAction.Create(EnterEnd));

        List<StateTransition> transitions = new List<StateTransition>() {
            new StateTransition(Start, RangedStart, () => GoNextState), // Start
            new StateTransition(RangedStart, RangedPlay, () => GoNextState && Targets.Count > 0), // Choose target(s) to range attack
            new StateTransition(RangedPlay, RangedStart, () => GoNextState), // Play cards to range attack with
            new StateTransition(RangedStart, BlockStart, () => GoNextState && Targets.Count == 0 && Alive.Count > 0), // End ranged phase (enemies alive)
            new StateTransition(RangedStart, Result, () => GoNextState && Targets.Count == 0 && Alive.Count == 0), // End ranged phase (no enemies alive)
            new StateTransition(BlockStart, BlockPlay, () => GoNextState && Targets.Count == 1 && AttackToHandle != null), // Choose enemy attack to block
            new StateTransition(BlockPlay, BlockStart, () => GoNextState), // Play cards to block with
            new StateTransition(BlockStart, AssignStart, () => GoNextState && HasUnassignedAttacks() && AttackToHandle == null), // End block phase with unblocked enemies
            new StateTransition(BlockStart, AttackStart, () => GoNextState && !HasUnassignedAttacks() && AttackToHandle == null), // End block phase with all enemies blocked
            new StateTransition(AssignStart, AssignDamage, () => GoNextState && Targets.Count == 1 && AttackToHandle != null), // Choose an enemy attack to assign damange
            new StateTransition(AssignDamage, AssignStart, () => GoNextState && DamageToAssign <= 0 && HasUnassignedAttacks()), // Choose a player/unit to assign damage to (no leftover attack & unassigned enemies left)
            new StateTransition(AssignDamage, AttackStart, () => GoNextState && DamageToAssign <= 0 && !HasUnassignedAttacks()), // Choose a player/unit to assign damage to (no leftover attack & no unassigned enemies left)
            new StateTransition(AttackStart, AttackPlay, () => GoNextState && Targets.Count > 0), // Choose target(s) to attack
            new StateTransition(AttackPlay, AttackStart, () => GoNextState), // Play cards to attack with
            new StateTransition(AttackStart, Result, () => GoNextState && Targets.Count == 0), // End attack phase
            new StateTransition(Result, End, () => GoNextState), // End attack phase
        };

        stateMachine = new StateMachine(Start, transitions);
    }

    /* ------------------- EVENTS ---------------------- */

    private void CombatAction_OnCombatNextStateClick(object sender, EventArgs e) {
        States prev = GetCurrentState();

        GoNextState = true;
        stateMachine.Tick();

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
            AssignDamageToPlayer(Targets[0], AttackToHandle);
        }
    }
}

public class CombatDefaultAction : StateAction {
    private Combat combat;

    public CombatDefaultAction(Combat combat) {
        this.combat = combat;
    }

    public override void OnTick() {
        //Debug.Log(combat.GoNextState + " " + combat.Targets.Count + " " + combat.Enemies.Count);
        //Debug.Log(combat.GoNextState && combat.Targets.Count == 0 && combat.Enemies.Count > 0);
    }

    public override void OnStateEnter() {
        combat.OnStateEnter();
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