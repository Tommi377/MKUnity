using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundStateMachine {
    // Flags
    private bool goNextState = false;
    private bool automaticTransfer = false;
    private int choiceIndex = -1;

    private StateMachine stateMachine;

    public RoundManager.States GetCurrentState() => (RoundManager.States)stateMachine.GetCurrentState().ID;

    private Player Player => GameManager.Instance.CurrentPlayer;

    public RoundStateMachine() {
        StateMachineInit();
    }

    public void AttemptStateTransfer(int choice = -1) {
        goNextState = true;
        choiceIndex = choice;
        stateMachine.Tick();
    }

    private void OnStateEnter() {
        goNextState = false;
        automaticTransfer = false;
        choiceIndex = -1;
        RoundManager.Instance.RoundStateEnter();
    }

    private State CreateState(RoundManager.States stateType) => new State(
        (int)stateType, new List<StateAction>() {
            OnStateEnterAction.Create(OnStateEnter),
            OnStateExitAction.Create(RoundManager.Instance.RoundStateExit)
        });

    private void StateMachineInit() {
        State RoundStart = CreateState(RoundManager.States.RoundStart);
        State TurnStart = CreateState(RoundManager.States.TurnStart);
        State TurnChoice = CreateState(RoundManager.States.TurnChoice);
        State NormalRest = CreateState(RoundManager.States.NormalRest);
        State SlowRest = CreateState(RoundManager.States.SlowRest);
        State Move = CreateState(RoundManager.States.Move);
        State PreAction = CreateState(RoundManager.States.PreAction);
        State Combat = CreateState(RoundManager.States.Combat);
        State Influence = CreateState(RoundManager.States.Influence);
        State ActionCard = CreateState(RoundManager.States.ActionCard);
        State SiteRewards = CreateState(RoundManager.States.SiteRewards);
        State LevelUp = CreateState(RoundManager.States.LevelUp);
        State Withdraw = CreateState(RoundManager.States.Withdraw);
        State TurnEnd = CreateState(RoundManager.States.TurnEnd);
        State RoundEnd = CreateState(RoundManager.States.RoundEnd);

        RoundStart.AddAction(OnStateExitAction.Create(RoundManager.Instance.RoundStartExit));
        TurnStart.AddAction(OnStateExitAction.Create(RoundManager.Instance.TurnStartExit));

        // Start states
        RoundStart  .To(TurnStart,  () => goNextState); // Start of round (has SoT actions)
        TurnStart   .To(TurnChoice, () => goNextState, () => Player.GetStartOfTurnActions().Count == 0); // Start of turn
        TurnChoice  .To(RoundEnd,   () => goNextState && choiceIndex == 2 && Player.CanEndRound()); // Announce end of round

        // Resting related states
        TurnChoice  .To(NormalRest, () => goNextState && choiceIndex == 1 && Player.CanNormalRest()); // Normal rest
        TurnChoice  .To(SlowRest,   () => goNextState && choiceIndex == 1 && Player.MustSlowRest()); // slow rest
        NormalRest  .To(SiteRewards,() => goNextState); // End of rest
        SlowRest    .To(SiteRewards,() => goNextState); // End of rest

        // Move related states
        TurnChoice  .To(Move,       () => goNextState && choiceIndex == 0 && Player.GetHand().Count > 0, () => !Player.CanNormalRest() && !Player.MustSlowRest() && !Player.CanEndRound()); // Start move phase
        Move        .To(PreAction,  () => goNextState && Player.IsOnSafeHex()); // Move -> Preaction (if on safe hex)
        Move        .To(Combat,     () => goNextState && !Player.IsOnSafeHex()); // Move -> Combat (if on unsafe hex)

        // Action related states
        PreAction   .To(Combat,     () => goNextState && choiceIndex == 0 && GameManager.Instance.GetPossibleActions().Contains(ActionTypes.Combat)); // Choose combat as action
        PreAction   .To(Influence,  () => goNextState && choiceIndex == 1 && GameManager.Instance.GetPossibleActions().Contains(ActionTypes.Influence)); // Choose influence as action
        PreAction   .To(ActionCard, () => goNextState && choiceIndex == 2 && GameManager.Instance.GetPossibleActions().Contains(ActionTypes.Action)); // Choose actioncard as action
        PreAction   .To(SiteRewards,() => goNextState && choiceIndex == 3); // Skip action

        Combat      .To(SiteRewards,() => goNextState && GameManager.Instance.Combat == null);
        Influence   .To(SiteRewards,() => goNextState);
        ActionCard  .To(SiteRewards,() => goNextState);

        // End states
        SiteRewards .To(LevelUp,    () => goNextState, () => true);
        LevelUp     .To(Withdraw,   () => goNextState, () => !Player.HasUnhandledLevelUp());
        Withdraw    .To(TurnEnd,    () => goNextState, () => true);
        TurnEnd     .To(TurnStart,  () => goNextState, () => true);

        RoundEnd    .To(RoundStart, () => goNextState);

        stateMachine = new StateMachine(RoundStart);
    }
}