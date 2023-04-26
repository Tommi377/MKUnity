using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class CombatStateMachine {
    private Combat combat;

    // Flags
    private bool goNextState = false;

    private StateMachine stateMachine;

    public Combat.States GetCurrentState() => (Combat.States)stateMachine.GetCurrentState().ID;

    public CombatStateMachine(Combat combat) {
        this.combat = combat;
        StateMachineInit();
    }

    public void AttemptStateTransfer() {
        goNextState = true;
        stateMachine.Tick();
    }

    private void OnStateEnter() {
        goNextState = false;
        combat.CombatStateEnter();
    }

    private void EnterResult() {
        combat.GenerateCombatResult();
    }

    private void EnterEnd() {
        combat.CombatEnd();
    }

    private State CreateState(Combat.States stateType) => new State((int)stateType, OnStateEnterAction.Create(OnStateEnter));

    private void StateMachineInit() {
        State Start = CreateState(Combat.States.Start);
        State Block = CreateState(Combat.States.Block);
        State Assign = CreateState(Combat.States.Assign);
        State Attack = CreateState(Combat.States.Attack);
        State Result = CreateState(Combat.States.Result);
        State End = CreateState(Combat.States.End);

        Result.AddAction(OnStateEnterAction.Create(EnterResult));
        End.AddAction(OnStateEnterAction.Create(EnterEnd));

        /////////////// TRANSITIONS ///////////////
        Start.To(Block,  () => goNextState); // Start

        // Block
        Block   .To(Assign, () => goNextState && !combat.EveryEnemyDefeated() && combat.HasUnassignedAttacks()); // Assign damage if unblocked
        Block   .To(Attack, () => goNextState && !combat.HasUnassignedAttacks()); // Skip to attack if everything blocked
        Block   .To(Result, () => goNextState && combat.EveryEnemyDefeated()); // Skip to end if all enemies defeated

        Assign  .To(Attack, () => goNextState && !combat.HasUnassignedAttacks());
        Attack  .To(Result, () => goNextState);
        Result  .To(End,    () => goNextState);

        stateMachine = new StateMachine(Start);
    }

    //private void StateMachineInit2() {
    //    State Start = CreateState(Combat.States.Start);
    //    State RangedStart = CreateState(Combat.States.RangedStart);
    //    State RangedPlay = CreateState(Combat.States.RangedPlay);
    //    State BlockStart = CreateState(Combat.States.BlockStart);
    //    State BlockPlay = CreateState(Combat.States.BlockPlay);
    //    State AssignStart = CreateState(Combat.States.AssignStart);
    //    State AssignDamage = CreateState(Combat.States.AssignDamage);
    //    State AttackStart = CreateState(Combat.States.AttackStart);
    //    State AttackPlay = CreateState(Combat.States.AttackPlay);
    //    State Result = CreateState(Combat.States.Result);
    //    State End = CreateState(Combat.States.End);

    //    Start.AddAction(OnStateExitAction.Create(ExitStart));
    //    RangedPlay.AddAction(OnStateExitAction.Create(ExitRangedPlay));
    //    BlockPlay.AddAction(OnStateExitAction.Create(ExitBlockPlay));
    //    AttackPlay.AddAction(OnStateExitAction.Create(ExitAttackPlay));
    //    Result.AddAction(OnStateEnterAction.Create(EnterResult));
    //    End.AddAction(OnStateEnterAction.Create(EnterEnd));

    //    Start       .To(RangedStart,    () => goNextState); // Start
    //    RangedStart .To(RangedPlay,     () => goNextState && Targets.Count > 0); // Choose target(s) to range attack
    //    RangedStart .To(BlockStart,     () => goNextState && Targets.Count == 0); // End ranged phase (enemies alive)
    //    RangedStart .To(Result,         () => false, () => Alive.Count == 0); // End ranged phase (no enemies alive)
    //    RangedPlay  .To(RangedStart,    () => goNextState && combat.Alive.Count > 0); // Play cards to range attack with
    //    RangedPlay  .To(Result,         () => goNextState && combat.Alive.Count == 0); // Play cards to range attack with
    //    BlockStart  .To(BlockPlay,      () => goNextState && Targets.Count == 1 && combat.TargetAttack != null); // Choose enemy attack to block
    //    BlockStart  .To(AssignStart,    () => goNextState && combat.HasUnassignedAttacks() && combat.TargetAttack == null); // End block phase with unblocked enemies
    //    BlockStart  .To(AttackStart,    () => goNextState && ! combat.HasUnassignedAttacks() && combat.TargetAttack == null); // End block phase with all enemies blocked
    //    BlockPlay   .To(BlockStart,     () => goNextState); // Play cards to block with
    //    AssignStart .To(AssignDamage,   () => goNextState && Targets.Count == 1 && combat.TargetAttack != null); // Choose an enemy attack to assign damage
    //    AssignDamage.To(AssignStart,    () => goNextState && combat.DamageToAssign <= 0 && combat.HasUnassignedAttacks()); // Choose a player/unit to assign damage to (no leftover attack & unassigned enemies left)
    //    AssignDamage.To(AttackStart,    () => goNextState && combat.DamageToAssign <= 0 && ! combat.HasUnassignedAttacks()); // Choose a player/unit to assign damage to (no leftover attack & no unassigned enemies left)
    //    AttackStart .To(AttackPlay,     () => goNextState && Targets.Count > 0); // Choose target(s) to 
    //    AttackStart .To(Result,         () => goNextState && Targets.Count == 0); // End attack phase (manually)
    //    AttackStart .To(Result,         () => false, () => Alive.Count == 0); // End attack phase (no enemies alive)
    //    AttackPlay  .To(AttackStart,    () => goNextState && combat.Alive.Count > 0); // Play cards to attack with
    //    AttackPlay  .To(Result,         () => goNextState && combat.Alive.Count == 0); // Play cards to attack with
    //    Result      .To(End,            () => goNextState);

    //    stateMachine = new StateMachine(Start);
    //}
}
