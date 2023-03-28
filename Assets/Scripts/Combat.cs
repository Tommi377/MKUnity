using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using Unity.VisualScripting;
using static Combat;

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
    public int TotalFame;
}

public class Combat {
    private List<Enemy> enemies = new List<Enemy>();
    private List<Enemy> defeated = new List<Enemy>();

    private List<CombatData> combatCards = new List<CombatData>();

    public CombatPhases CombatPhase { get; private set; } = CombatPhases.Range;
    public Player Player { get; private set; }

    public ReadOnlyCollection<Enemy> Enemies { get => enemies.AsReadOnly(); }

    /* STATE MACHINE - END */

    /* EVENT DEFINITIONS - START */
    public static event EventHandler OnCombatStart;
    public static event EventHandler OnCombatAttack;
    public static event EventHandler OnCombatBlock;
    public static event EventHandler OnCombatAssign;
    public static event EventHandler<OnCombatPhaseChangeArgs> OnCombatPhaseChange;
    public class OnCombatPhaseChangeArgs : EventArgs {
        public Combat combat;
        public CombatPhases phase;
    }
    public static event EventHandler<OnCombatEndArgs> OnCombatEnd;
    public class OnCombatEndArgs : EventArgs {
        public Combat combat;
        public CombatResult result;
    }
    public static void ResetStaticData() {
        OnCombatAttack = null;
        OnCombatBlock = null;
        OnCombatAssign = null;
        OnCombatStart = null;
        OnCombatEnd = null;
    }
    /* EVENT DEFINITIONS - END */

    public struct UnassignedAttack {
        public Enemy Enemy;
        public EnemyAttack Attack;
    }

    public Combat(Player player, IEnumerable<Enemy> enemies) {
        Player = player;
        this.enemies.AddRange(enemies);

        foreach (Enemy enemy in enemies) {
            foreach (EnemyAttack attack in enemy.Attacks) {
                UnassignedAttacks.Add(new UnassignedAttack() { Enemy = enemy, Attack = attack });
            }
        }
    }

    public void Init() {
        OnCombatStart?.Invoke(this, EventArgs.Empty);
        OnCombatPhaseChange?.Invoke(this, new OnCombatPhaseChangeArgs { combat = this, phase = CombatPhases.Range });

        ButtonInputManager.Instance.OnCombatEnemyChooseClick += ButtonInput_OnCombatEnemyChooseClick;
        ButtonInputManager.Instance.OnCombatNextPhaseClick += ButtonInput_OnCombatNextPhaseClick;
        ButtonInputManager.Instance.OnCombatBlockChooseClick += ButtonInput_OnCombatBlockChooseClick;
        ButtonInputManager.Instance.OnAssignEnemyDamageClick += ButtonInput_OnAssignEnemyDamageClick;
    }

    public void Dispose() {
        ButtonInputManager.Instance.OnCombatEnemyChooseClick -= ButtonInput_OnCombatEnemyChooseClick;
        ButtonInputManager.Instance.OnCombatNextPhaseClick -= ButtonInput_OnCombatNextPhaseClick;
        ButtonInputManager.Instance.OnCombatBlockChooseClick -= ButtonInput_OnCombatBlockChooseClick;
        ButtonInputManager.Instance.OnAssignEnemyDamageClick -= ButtonInput_OnAssignEnemyDamageClick;
    }

    public ReadOnlyCollection<CombatData> CombatCards => combatCards.AsReadOnly();

    public bool CombatEnded { get => CombatPhase == CombatPhases.End; }

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
        if (enemies.Count == 0) {
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

    private void AttackEnemies(List<Enemy> targets) {
        if (targets.Count > 0) {
            Debug.Log(Player + ", " + targets + ", " + combatCards + ", " + CombatPhase);
            CombatAttack combatAttack = new CombatAttack(Player, targets, combatCards, CombatPhase);
            if (combatAttack.IsEnemyDead()) {
                defeated.AddRange(targets);
                foreach (Enemy target in targets) {
                    enemies.Remove(target);
                }
                Debug.Log("Enemy defeated!! remaining: " + enemies.Count);
            }
            Debug.Log("Enemy left with hp: " + combatAttack.TotalArmor);
        } else {
            Debug.Log("Must have targets to attack");
        }

        OnCombatAttack?.Invoke(this, EventArgs.Empty);
        combatCards.Clear();
    }

    private void BlockEnemyAttack(UnassignedAttack attack) {
        if (UnassignedAttacks.Contains(attack)) {
            CombatBlock combatBlock = new CombatBlock(Player, attack, combatCards);
            int damageReceived = combatBlock.PlayerReceivedDamage();

            if (combatBlock.FullyBlocked) {
                Debug.Log("Enemy attack was fully blocked");
                UnassignedAttacks.Remove(attack);
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
                Alive = enemies,
                Defeated = defeated,
                TotalFame = defeated.Sum(enemy => enemy.Fame),
            };
            OnCombatEnd?.Invoke(this, new OnCombatEndArgs { combat = this, result = result });
        }
    }

    private void AssignDamageToPlayer(UnassignedAttack attack) {
        // TODO: Brutal ability and stuff
        int woundCardAmount = Mathf.CeilToInt((float)attack.Attack.Damage / Player.Armor);
        Player.TakeWounds(woundCardAmount);

        UnassignedAttacks.Remove(attack);
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

    public void OnStateEnter() {
        OnCombatStateEnter?.Invoke(this, new OnCombatStateEnterArgs { Combat = this, State = (States)stateMachine.GetCurrentState().ID });
    }

    public void OnStateExit() {
        GoNextState = false;
    }

    public bool GoNextState { get; private set; } = false;
    public List<Enemy> Targets { get; private set; } = new List<Enemy>();

    public EnemyAttack AttackToBlock;

    public EnemyAttack AttackToAssign;
    public List<UnassignedAttack> UnassignedAttacks = new List<UnassignedAttack>();
    public bool FinishedDamageAssigning = false;

    public enum States { Start, RangedStart, RangedPlay, BlockStart, BlockPlay, AssignStart, AssignDamage, AttackStart, AttackPlay, End}

    State Start = new State((int)States.Start);
    State RangedStart = new State((int)States.RangedStart);
    State RangedPlay = new State((int)States.RangedPlay);
    State BlockStart = new State((int)States.BlockStart);
    State BlockPlay = new State((int)States.BlockPlay);
    State AssignStart = new State((int)States.AssignStart);
    State AssignDamage = new State((int)States.AssignDamage);
    State AttackStart = new State((int)States.AttackStart);
    State AttackPlay = new State((int)States.AttackPlay);
    State End = new State((int)States.End);

    public void StateMachineInit() {
        List<StateTransition> transitions = new List<StateTransition>() {
            new CombatTransition(this, Start, RangedStart, () => GoNextState), // Start
            new CombatTransition(this, RangedStart, RangedPlay, () => GoNextState && Targets.Count > 0), // Choose target(s) to range attack
            new CombatTransition(this, RangedPlay, RangedStart, () => GoNextState), // Play cards to range attack with
            new CombatTransition(this, RangedStart, BlockStart, () => GoNextState && Targets.Count == 0 && Enemies.Count > 0), // End ranged phase (enemies alive)
            new CombatTransition(this, RangedStart, End, () => GoNextState && Targets.Count == 0 && Enemies.Count == 0), // End ranged phase (no enemies alive)
            new CombatTransition(this, BlockStart, BlockPlay, () => GoNextState && AttackToBlock != null), // Choose enemy attack to block
            new CombatTransition(this, BlockPlay, BlockStart, () => GoNextState), // Play cards to block with
            new CombatTransition(this, BlockStart, AssignStart, () => GoNextState && UnassignedAttacks.Count > 0), // End block phase with unblocked enemies
            new CombatTransition(this, BlockStart, AttackStart, () => GoNextState && UnassignedAttacks.Count == 0), // End block phase with all enemies blocked
            new CombatTransition(this, AssignStart, AssignDamage, () => AttackToAssign != null), // Choose an enemy attack to assign damange
            new CombatTransition(this, AssignDamage, AssignDamage, () => !FinishedDamageAssigning), // Choose a player/unit to assign damage to (leftover attack)
            new CombatTransition(this, AssignDamage, AssignStart, () => FinishedDamageAssigning && UnassignedAttacks.Count > 0), // Choose a player/unit to assign damage to (no leftover attack & unassigned enemies left)
            new CombatTransition(this, AssignDamage, AttackStart, () => FinishedDamageAssigning && UnassignedAttacks.Count > 0), // Choose a player/unit to assign damage to (no leftover attack & no unassigned enemies left)
            new CombatTransition(this, AttackStart, AttackPlay, () => GoNextState && Targets.Count > 0), // Choose target(s) to attack
            new CombatTransition(this, AttackPlay, AttackStart, () => GoNextState), // Play cards to attack with
            new CombatTransition(this, AttackStart, End, () => GoNextState && Targets.Count == 0), // End attack phase
        };

        TransitionTable transitionTable = new TransitionTable(Start, transitions);
        stateMachine = new StateMachine(transitionTable);
    }
}

public class CombatAttack {
    public Player Player { get; private set; }
    public List<Enemy> Enemies { get; private set; }

    public List<CombatData> CombatCards { get; private set; }
    public CombatPhases CombatPhase { get; private set; }

    public int TotalArmor { get; private set; } = 0;

    public CombatAttack(Player player, List<Enemy> enemies, List<CombatData> combatCards, CombatPhases combatPhase) {
        Player = player;
        Enemies = enemies;
        CombatCards = combatCards;
        CombatPhase = combatPhase;

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
        if (CombatPhase == CombatPhases.Range) {
            if (combatCard.CombatType == CombatTypes.Range || combatCard.CombatType == CombatTypes.Siege) {
                ReduceArmor(combatCard);
            }
        } else if (CombatPhase == CombatPhases.Attack) {
            ReduceArmor(combatCard);
        } else {
            Debug.Log("Can't deal damage with this card");
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
    public UnassignedAttack UnassignedAttack { get; private set; }
    public List<CombatData> CombatCards { get; private set; }

    public int TotalDamage { get; private set; } = 0;
    public int TotalBlock { get; private set; } = 0;

    private bool combatPrevented = false;

    public CombatBlock(Player player, UnassignedAttack unassignedAttack, List<CombatData> combatCards) {
        Player = player;
        UnassignedAttack = unassignedAttack;
        CombatCards = combatCards;

        // Modify this attack depending on modifiers
        TotalDamage = unassignedAttack.Attack.Damage;

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

public class CombatTransition : StateTransition {
    private Combat combat;

    public CombatTransition(Combat combat, State fromState, State toState, Func<bool> condition) : base(fromState, toState, condition) {
        this.combat = combat;
    }

    public override void OnStateEnter() {
        combat.OnStateEnter();
    }

    public override void OnStateExit() {
        combat.OnStateExit();
    }
}