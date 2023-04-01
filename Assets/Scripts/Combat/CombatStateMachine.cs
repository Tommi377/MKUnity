using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class CombatStateMachine {
    private Combat combat;

    // Flags
    private bool goNextState = false;
    private bool transferIfNoEnemies = false;

    private StateMachine stateMachine;

    public Combat.States GetCurrentState() => (Combat.States)stateMachine.GetCurrentState().ID;

    private List<Enemy> Targets => combat.Targets;
    private ReadOnlyCollection<Enemy> Alive => combat.Alive;

    public CombatStateMachine(Combat combat) {
        this.combat = combat;
        StateMachineInit();
    }

    public void AttemptStateTransfer() {
        goNextState = true;
        stateMachine.Tick();
    }

    private void TransferIfNoEnemies() {
        transferIfNoEnemies = true;
        stateMachine.Tick();
    }

    private void OnStateEnter() {
        goNextState = false;
        transferIfNoEnemies = false;
        combat.CombatStateEnter();
    }

    private void ExitStart() {
        combat.RemoveTargetEnemies();
    }

    private void ExitRangedPlay() {
        combat.AttackEnemies(true);
    }

    private void ExitAttackPlay() {
        combat.AttackEnemies(false);
    }

    private void ExitBlockPlay() {
        combat.BlockEnemyAttack();
    }

    private void EnterResult() {
        combat.GenerateCombatResult();
    }

    private void EnterEnd() {
        combat.CombatEnd();
    }

    private State CreateState(Combat.States stateType, bool noEnemiesCheck = false) {
        State state = new State((int)stateType, OnStateEnterAction.Create(OnStateEnter));
        if (noEnemiesCheck) {
            state.AddAction(OnStateEnterAction.Create(TransferIfNoEnemies));
        }
        return state;
    }

    private void StateMachineInit() {
        State Start = CreateState(Combat.States.Start);
        State RangedStart = CreateState(Combat.States.RangedStart, true);
        State RangedPlay = CreateState(Combat.States.RangedPlay);
        State BlockStart = CreateState(Combat.States.BlockStart);
        State BlockPlay = CreateState(Combat.States.BlockPlay);
        State AssignStart = CreateState(Combat.States.AssignStart);
        State AssignDamage = CreateState(Combat.States.AssignDamage);
        State AttackStart = CreateState(Combat.States.AttackStart, true);
        State AttackPlay = CreateState(Combat.States.AttackPlay);
        State Result = CreateState(Combat.States.Result);
        State End = CreateState(Combat.States.End);

        Start.AddAction(OnStateExitAction.Create(ExitStart));
        RangedPlay.AddAction(OnStateExitAction.Create(ExitRangedPlay));
        BlockPlay.AddAction(OnStateExitAction.Create(ExitBlockPlay));
        AttackPlay.AddAction(OnStateExitAction.Create(ExitAttackPlay));
        Result.AddAction(OnStateEnterAction.Create(EnterResult));
        End.AddAction(OnStateEnterAction.Create(EnterEnd));

        List<StateTransition> transitions = new List<StateTransition>() {
            new StateTransition(Start, RangedStart, () => goNextState), // Start
            new StateTransition(RangedStart, RangedPlay, () => goNextState && Targets.Count > 0), // Choose target(s) to range attack
            new StateTransition(RangedPlay, RangedStart, () => goNextState && combat.Alive.Count > 0), // Play cards to range attack with
            new StateTransition(RangedPlay, Result, () => goNextState && combat.Alive.Count == 0), // Play cards to range attack with
            new StateTransition(RangedStart, BlockStart, () => goNextState && Targets.Count == 0), // End ranged phase (enemies alive)
            new StateTransition(RangedStart, Result, () => transferIfNoEnemies && Alive.Count == 0), // End ranged phase (no enemies alive)
            new StateTransition(BlockStart, BlockPlay, () => goNextState && Targets.Count == 1 && combat.AttackToHandle != null), // Choose enemy attack to block
            new StateTransition(BlockPlay, BlockStart, () => goNextState), // Play cards to block with
            new StateTransition(BlockStart, AssignStart, () => goNextState && combat.HasUnassignedAttacks() && combat.AttackToHandle == null), // End block phase with unblocked enemies
            new StateTransition(BlockStart, AttackStart, () => goNextState && !combat.HasUnassignedAttacks() && combat.AttackToHandle == null), // End block phase with all enemies blocked
            new StateTransition(AssignStart, AssignDamage, () => goNextState && Targets.Count == 1 && combat.AttackToHandle != null), // Choose an enemy attack to assign damange
            new StateTransition(AssignDamage, AssignStart, () => goNextState && combat.DamageToAssign <= 0 && combat.HasUnassignedAttacks()), // Choose a player/unit to assign damage to (no leftover attack & unassigned enemies left)
            new StateTransition(AssignDamage, AttackStart, () => goNextState && combat.DamageToAssign <= 0 && !combat.HasUnassignedAttacks()), // Choose a player/unit to assign damage to (no leftover attack & no unassigned enemies left)
            new StateTransition(AttackStart, AttackPlay, () => goNextState && Targets.Count > 0), // Choose target(s) to attack
            new StateTransition(AttackStart, Result, () => transferIfNoEnemies && Alive.Count == 0), // End attack phase
            new StateTransition(AttackPlay, AttackStart, () => goNextState && combat.Alive.Count > 0), // Play cards to attack with
            new StateTransition(AttackPlay, Result, () => goNextState && combat.Alive.Count == 0), // Play cards to attack with
            new StateTransition(Result, End, () => goNextState), // End attack phase
        };

        stateMachine = new StateMachine(Start, transitions);
    }
}
