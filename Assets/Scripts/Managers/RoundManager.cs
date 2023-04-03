using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public enum Time { Day, Night }

public enum ActionTypes {
    None,
    Move,
    Combat,
    Influence,
    Special,
    Action,
    Heal
}

public enum TurnPhases {
    Start,
    Special1,
    Movement,
    ChooseAction,
    Action,
    Special2,
    End
}

public class RoundManager : MonoBehaviour {
    public static RoundManager Instance { get; private set; }

    public enum States { RoundStart, /*RoundCard,*/ TurnStart, TurnChoice, RestChoice, NormalRest, SlowRest, Move, PreAction, Combat, Influence, ActionCard, TurnEnd, RoundEnd }

    /* EVENT DEFINITIONS - START */
    public event EventHandler OnNewRound;
    public event EventHandler OnNewTurn;
    public event EventHandler<OnPhaseChangeArgs> OnPhaseChange;
    public class OnPhaseChangeArgs : EventArgs {
        public TurnPhases phase;
        public ActionTypes actionType;
    }
    /* EVENT DEFINITIONS - END */

    public int Round { get; private set; }
    public int Turn { get; private set; }
    public Time Time { get; private set; }
    public ActionTypes CurrentAction { get; private set; }
    public TurnPhases CurrentPhase { get; private set; }

    private bool endOfRoundFlag = false;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogError("More than one instance of a singleton");
        } else {
            Instance = this;
        }

        Init();
    }

    private void Start() {

        Combat.OnCombatEnd += Combat_OnCombatEnd;

        ButtonInputManager.Instance.OnEndStartPhaseClick += ButtonInput_OnEndStartPhaseClick;
        ButtonInputManager.Instance.OnEndMovementClick += ButtonInput_OnEndMovementClick;
        ButtonInputManager.Instance.OnEndEndPhaseClick += ButtonInput_OnEndEndPhaseClick;
        ButtonInputManager.Instance.OnEndInfluencePhaseClick += ButtonInput_OnEndInfluencePhaseClick;
    }

    // Tests if a card's action is playable with the selected action
    public bool CanApplyAction(Card card, CardChoice actionChoice, PlayCardOptions options) {
        if (actionChoice.Id < 0) return true; // Can play always if using default action

        if (!CanPlayCard(card)) return false; // Can't play if card actions does not include round action

        // Use mana
        if (!options.SkipManaUse && card is ActionCard && actionChoice.ManaTypes.Any()) {
            if (!ManaManager.Instance.SelectedManaUsableWithChoice(actionChoice)) {
                Debug.Log("Selected mana not suitable for casting");
                return false;
            }
        }

        if (!card.CanApply(CurrentAction, actionChoice)) { // Can't play if card specific restriction does not go through
            Debug.Log("Can't apply card effect");
            return false;
        }
        return true;
    }

    // Tests if a card is playable with the selected action
    public bool CanPlayCard(Card card) {
        if (!CardManager.Instance.CanPlay()) {
            Debug.Log("Player can't play card");
            return false;
        }

        if (CurrentPhase != TurnPhases.Movement && CurrentPhase != TurnPhases.Action) {
            Debug.Log("Can't play card in the " + CurrentPhase + " phase!");
            return false;
        }

        return true;
    }

    public bool IsDay() => Time == Time.Day;
    public bool IsNight() => Time == Time.Night;

    private void Init() {
        Turn = 0;
        Round = 0;
        NewRound();
    }

    private void NewRound() {
        Round++;
        Turn = 0;
        Time = Round % 2 == 0 ? Time.Night : Time.Day;

        OnNewRound?.Invoke(this, EventArgs.Empty);

        NewTurn();
    }

    private void NewTurn() {
        Turn++;

        OnNewTurn?.Invoke(this, EventArgs.Empty);

        SetPhaseAndAction(TurnPhases.Start);
    }

    private void TurnEnd() {
        if (endOfRoundFlag) {
            NewRound();
        } else {
            NewTurn();
        }
    }

    public void SetPhaseAndAction(TurnPhases phase, ActionTypes action = ActionTypes.None) {
        if (phase == TurnPhases.Action) {
            if (action == ActionTypes.None) {
                CurrentPhase = TurnPhases.End;
            } else {
                CurrentPhase = TurnPhases.Action;
            }
            CurrentAction = action;
        } else {
            CurrentPhase = phase;
            CurrentAction = action;
        }

        if (CurrentPhase == TurnPhases.End) {
            EndAction();
        }

        OnPhaseChange?.Invoke(this, new OnPhaseChangeArgs { phase = CurrentPhase, actionType = CurrentAction });
    }

    private void EndAction() {
        GameManager.Instance.CurrentPlayer.EndAction();
    }

    /* ------------------- EVENTS ---------------------- */

    private void Combat_OnCombatEnd(object sender, EventArgs e) {
        SetPhaseAndAction(TurnPhases.End);
    }

    private void ButtonInput_OnEndStartPhaseClick(object sender, EventArgs e) {
        SetPhaseAndAction(TurnPhases.Movement, ActionTypes.Move);
    }

    private void ButtonInput_OnEndMovementClick(object sender, System.EventArgs e) {
        SetPhaseAndAction(TurnPhases.ChooseAction);
    }

    private void ButtonInput_OnEndEndPhaseClick(object sender, EventArgs e) {
        TurnEnd();
    }

    private void ButtonInput_OnEndInfluencePhaseClick(object sender, EventArgs e) {
        SetPhaseAndAction(TurnPhases.End);
    }
}

public class RoundStateMachine {
    // Flags
    private bool goNextState = false;
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
        choiceIndex = -1;
}

    private State CreateState(RoundManager.States stateType) => new State((int)stateType, OnStateEnterAction.Create(OnStateEnter));

    // StartOfRound, StartOfTurn, TurnChoice, RestChoice, NormalRest, SlowRest, Move, ActionChoice, Combat, Influence, ActionCard, EndOfTurn, EndOfRound
    private void StateMachineInit() {
        State RoundStart    = CreateState(RoundManager.States.RoundStart);
        State TurnStart     = CreateState(RoundManager.States.TurnStart);
        State TurnChoice    = CreateState(RoundManager.States.TurnChoice);
        State NormalRest    = CreateState(RoundManager.States.NormalRest);
        State SlowRest      = CreateState(RoundManager.States.SlowRest);
        State Move          = CreateState(RoundManager.States.Move);
        State PreAction     = CreateState(RoundManager.States.PreAction);
        State Combat        = CreateState(RoundManager.States.Combat);
        State Influence     = CreateState(RoundManager.States.Influence);
        State ActionCard    = CreateState(RoundManager.States.ActionCard);
        State TurnEnd       = CreateState(RoundManager.States.TurnEnd);
        State RoundEnd      = CreateState(RoundManager.States.RoundEnd);


        List<StateTransition> transitions = new List<StateTransition>() {
            // Start states
            RoundStart  .To(TurnStart,  () => goNextState), // Start of round
            TurnStart   .To(TurnChoice, () => goNextState), // Start of turn
            TurnChoice  .To(RoundEnd,   () => goNextState && Player.GetDeckCount() == 0), // Announce end of round

            // Resting related states
            TurnChoice  .To(NormalRest, () => goNextState && Player.GetDeckCount() > 0 && choiceIndex == 1 && Player.GetWoundCount() > 0 && Player.GetWoundCount() < Player.HandLimit), // Normal rest
            TurnChoice  .To(SlowRest,   () => goNextState && Player.GetDeckCount() > 0 && choiceIndex == 1 && Player.GetWoundCount() == Player.HandLimit), // slow rest
            NormalRest  .To(TurnEnd,    () => goNextState), // End of rest
            SlowRest    .To(TurnEnd,    () => goNextState), // End of rest

            // Move related states
            TurnChoice  .To(Move,       () => goNextState && Player.GetDeckCount() > 0 && choiceIndex == 0), // Start move phase
            Move        .To(PreAction,  () => goNextState && Player.IsOnSafeHex()), // Move -> Preaction (if on safe hex)
            Move        .To(Combat,     () => goNextState && !Player.IsOnSafeHex()), // Move -> Combat (if on unsafe hex)

            // Action related states
            PreAction   .To(Combat,     () => goNextState && choiceIndex == 0 && GameManager.Instance.GetPossibleActions().Contains(ActionTypes.Combat)), // Choose combat as action
            PreAction   .To(Influence,  () => goNextState && choiceIndex == 1 && GameManager.Instance.GetPossibleActions().Contains(ActionTypes.Influence)), // Choose influence as action
            PreAction   .To(ActionCard, () => goNextState && choiceIndex == 2 && GameManager.Instance.GetPossibleActions().Contains(ActionTypes.Action)), // Choose actioncard as action

            Combat      .To(TurnEnd,    () => goNextState && GameManager.Instance.Combat == null),
            Influence   .To(TurnEnd,    () => goNextState),
            ActionCard  .To(TurnEnd,    () => goNextState),

            // End states
            TurnEnd     .To(TurnStart,  () => goNextState),
            RoundEnd    .To(RoundStart, () => goNextState),
        };

        stateMachine = new StateMachine(RoundStart, transitions);
    }
}