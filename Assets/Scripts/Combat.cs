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

    public CombatPhases CombatPhase { get; private set; } = CombatPhases.Range;
    public Player Player { get; private set; }

    public ReadOnlyCollection<Enemy> Enemies { get => enemies.AsReadOnly(); }
    public ReadOnlyCollection<Enemy> Alive { get => alive.AsReadOnly(); }
    public ReadOnlyCollection<Enemy> Defeated { get => defeated.AsReadOnly(); }
    public Dictionary<Enemy, List<EnemyAttack>> UnassignedAttacks => unassignedAttacks;

    private CombatResult result;

    /* STATE MACHINE - END */

    /* EVENT DEFINITIONS - START */
    public static event EventHandler OnCombatStart;
    public static event EventHandler OnCombatAttack;
    public static event EventHandler OnCombatBlock;
    public static event EventHandler OnCombatAssign;
    public static event EventHandler<OnCombatResultArgs> OnCombatEnd;
    public static event EventHandler<OnCombatPhaseChangeArgs> OnCombatPhaseChange;
    public class OnCombatPhaseChangeArgs : EventArgs {
        public Combat combat;
        public CombatPhases phase;
    }
    public static event EventHandler<OnCombatResultArgs> OnGenerateCombatResult;
    public class OnCombatResultArgs : EventArgs {
        public Combat combat;
        public CombatResult result;
    }
    public static void ResetStaticData() {
        OnCombatAttack = null;
        OnCombatBlock = null;
        OnCombatAssign = null;
        OnCombatStart = null;
        OnCombatEnd = null;
        OnGenerateCombatResult = null;
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
        OnCombatStart?.Invoke(this, EventArgs.Empty);
        OnCombatPhaseChange?.Invoke(this, new OnCombatPhaseChangeArgs { combat = this, phase = CombatPhases.Range });

        ButtonInputManager.Instance.OnCombatEnemyChooseClick += ButtonInput_OnCombatEnemyChooseClick;
        ButtonInputManager.Instance.OnCombatNextPhaseClick += ButtonInput_OnCombatNextPhaseClick;
        ButtonInputManager.Instance.OnCombatBlockChooseClick += ButtonInput_OnCombatBlockChooseClick;
        ButtonInputManager.Instance.OnAssignEnemyDamageClick += ButtonInput_OnAssignEnemyDamageClick;

        CombatAction.OnCombatNextStateClick += CombatAction_OnCombatNextStateClick;
        CombatAction.OnTargetsSelectedClick += CombatAction_OnTargetsSelectedClick;
        CombatAction.OnAttackSelectedClick += CombatAction_OnAttackSelectedClick;
        CombatAction.OnDamageAssignClick += CombatAction_OnDamageAssignClick;
    }

    public void Dispose() {
        ButtonInputManager.Instance.OnCombatEnemyChooseClick -= ButtonInput_OnCombatEnemyChooseClick;
        ButtonInputManager.Instance.OnCombatNextPhaseClick -= ButtonInput_OnCombatNextPhaseClick;
        ButtonInputManager.Instance.OnCombatBlockChooseClick -= ButtonInput_OnCombatBlockChooseClick;
        ButtonInputManager.Instance.OnAssignEnemyDamageClick -= ButtonInput_OnAssignEnemyDamageClick;
    }

    public ReadOnlyCollection<CombatData> CombatCards => combatCards.AsReadOnly();

    public bool CombatEnded { get => CombatPhase == CombatPhases.End; }

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
        Debug.Log(combatData);
    }

    private void NextPhase() {
        if (alive.Count == 0) {
            CombatPhase = CombatPhases.End;
        }

        switch (CombatPhase) {
            case CombatPhases.Range:
                CombatPhase = CombatPhases.Block;
                break;
            case CombatPhases.Block:
                CombatPhase = CombatPhases.AssignDamage;
                break;
            case CombatPhases.AssignDamage:
                CombatPhase = CombatPhases.Attack;
                break;
            case CombatPhases.Attack:
                CombatPhase = CombatPhases.End;
                break;
        }
        Debug.Log("Next phase: " + CombatPhase);
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

        OnCombatAttack?.Invoke(this, EventArgs.Empty);
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

        OnCombatBlock?.Invoke(this, EventArgs.Empty);
        combatCards.Clear();
    }

    private void CombatNextPhase() {
        NextPhase();
        OnCombatPhaseChange?.Invoke(this, new OnCombatPhaseChangeArgs { combat = this, phase = CombatPhase });
        if (CombatEnded) {
            CombatResult result = new CombatResult {
                Player = Player,
                Alive = alive,
                Defeated = defeated,
                Fame = defeated.Sum(enemy => enemy.Fame),
            };
            OnGenerateCombatResult?.Invoke(this, new OnCombatResultArgs { combat = this, result = result });
        }
    }

    private void AssignDamageToPlayer(Enemy enemy, EnemyAttack attack) {
        // TODO: Brutal ability and stuff
        Player.TakeWounds(1);
        DamageToAssign -= Player.Armor;

        if (DamageToAssign <= 0) {
            unassignedAttacks[enemy].Remove(attack);
        }

        OnCombatAssign?.Invoke(this, EventArgs.Empty);
    }

    /* ------------------- EVENTS ---------------------- */

    private void ButtonInput_OnCombatEnemyChooseClick(object sender, ButtonInputManager.OnCombatEnemyChooseClickArgs e) {
        // AttackEnemies(e.enemies);
    }

    private void ButtonInput_OnCombatNextPhaseClick(object sender, EventArgs e) {
        // CombatNextPhase();
    }

    private void ButtonInput_OnCombatBlockChooseClick(object sender, ButtonInputManager.OnCombatBlockChooseClickArgs e) {
        // BlockEnemyAttack(e.blockedEnemy);
    }

    private void ButtonInput_OnAssignEnemyDamageClick(object sender, ButtonInputManager.OnAssignEnemyDamageClickArgs e) {
        // AssignDamageToPlayer(e.damagingEnemy);
    }

    ////////////////////////
    ///
    public static event EventHandler<OnCombatStateEnterArgs> OnCombatStateEnter;
    public class OnCombatStateEnterArgs : EventArgs {
        public Combat Combat;
        public States State;
    }

    StateMachine stateMachine;

    public States GetCurrentState() => (States)stateMachine.GetCurrentState().ID;

    public void OnStateEnter() {
        GoNextState = false;
        States state = GetCurrentState();
        if (state == States.RangedStart || state == States.BlockStart || state == States.AssignStart || state == States.AttackStart) {
            AttackToHandle = null;
            Targets.Clear();
        }

        OnCombatStateEnter?.Invoke(this, new OnCombatStateEnterArgs { Combat = this, State = GetCurrentState() });
    }

    public bool GoNextState { get; private set; } = false;
    public List<Enemy> Targets { get; private set; } = new List<Enemy>();

    public EnemyAttack AttackToHandle;
    public int DamageToAssign = 0;

    public enum States { Start, RangedStart, RangedPlay, BlockStart, BlockPlay, AssignStart, AssignDamage, AttackStart, AttackPlay, Result, End}

    private void ExecuteRangedPlay() {
        AttackEnemies(Targets, true);
    }

    private void ExecuteAttackPlay() {
        AttackEnemies(Targets, false);
    }

    private void ExecuteBlockPlay() {
        BlockEnemyAttack(Targets[0], AttackToHandle);
    }

    private void InitResult() {
        result = new CombatResult {
            Player = Player,
            Alive = alive,
            Defeated = defeated,
            Fame = defeated.Sum(enemy => enemy.Fame),
            Reputation = 0 // TODO: Reputation calculation
        };
        OnGenerateCombatResult?.Invoke(this, new OnCombatResultArgs { combat = this, result = result });
    }

    private void ExecuteResult() {
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

        RangedPlay.AddAction(OnStateExitAction.Create(ExecuteRangedPlay));
        BlockPlay.AddAction(OnStateExitAction.Create(ExecuteBlockPlay));
        AttackPlay.AddAction(OnStateExitAction.Create(ExecuteAttackPlay));
        Result.AddAction(OnStateEnterAction.Create(InitResult));
        End.AddAction(OnStateEnterAction.Create(ExecuteResult));

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

public class CombatAttack {
    public Player Player { get; private set; }
    public List<Enemy> Enemies { get; private set; }

    public List<CombatData> CombatCards { get; private set; }
    public bool RangePhase{ get; private set; }

    public int TotalArmor { get; private set; } = 0;

    public CombatAttack(Player player, List<Enemy> enemies, List<CombatData> combatCards, bool rangePhase) {
        Player = player;
        Enemies = enemies;
        CombatCards = combatCards;
        RangePhase = rangePhase;

        Enemies.ForEach(enemy => TotalArmor += enemy.Armor);

        foreach (CombatData combatCard in CombatCards) {
            DealDamage(combatCard);
        }
    }

    private void DealDamage(CombatData combatCard) {
        if (combatCard.CombatType == CombatTypes.Block) {
            return;
        }

        // TODO: add resistance checking
        // TODO: add siege checking
        if (RangePhase) {
            if (combatCard.CombatType == CombatTypes.Range || combatCard.CombatType == CombatTypes.Siege) {
                ReduceArmor(combatCard);
            }
        } else {
            ReduceArmor(combatCard);
        }
    }

    private void ReduceArmor(CombatData combatCard) {
        TotalArmor -= combatCard.Damage;
        if (combatCard.CombatAttackModifier != null) {
            TotalArmor -= combatCard.CombatAttackModifier(this);
        }
    }

    public bool IsEnemyDead() {
        return TotalArmor <= 0;
    }
}
public class CombatBlock { 
    public Player Player { get; private set; }
    public Enemy Enemy { get; private set; }
    public EnemyAttack Attack { get; private set; }
    public List<CombatData> CombatCards { get; private set; }

    public int TotalDamage { get; private set; } = 0;
    public int TotalBlock { get; private set; } = 0;

    private bool combatPrevented = false;

    public CombatBlock(Player player, Enemy enemy, EnemyAttack attack, List<CombatData> combatCards) {
        Player = player;
        Enemy = enemy;
        Attack = attack;
        CombatCards = combatCards;

        // Modify this attack depending on modifiers
        TotalDamage = Attack.Damage;

        foreach (CombatData combatCard in combatCards) {
            BlockDamage(combatCard);
        }
    }
    public int PlayerReceivedDamage() {
        return FullyBlocked ? 0 : TotalDamage;
    }

    public bool FullyBlocked { get => combatPrevented || TotalDamage <= TotalBlock; }

    public void PreventEnemyAttack() => combatPrevented = true;

    private void BlockDamage(CombatData combatCard) {
        if (combatCard.CombatType != CombatTypes.Block) {
            return;
        }
        // TODO: add resistance and other checking
        TotalBlock += combatCard.Damage;
        if (combatCard.CombatBlockModifier != null) {
            TotalBlock += combatCard.CombatBlockModifier(this);
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