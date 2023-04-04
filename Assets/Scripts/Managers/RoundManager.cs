using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoundManager : MonoBehaviour {
    public static RoundManager Instance { get; private set; }

    public enum States { RoundStart, /*RoundCard,*/ TurnStart, TurnChoice, NormalRest, SlowRest, Move, PreAction, Combat, Influence, ActionCard, TurnEnd, RoundEnd }

    /* EVENT DEFINITIONS - START */
    public event EventHandler OnNewRound;
    public event EventHandler OnNewTurn;

    public event EventHandler<RoundStateArgs> OnRoundStateEnter;
    public event EventHandler<RoundStateArgs> OnRoundStateExit;
    public class RoundStateArgs : EventArgs {
        public States State;
    }
    /* EVENT DEFINITIONS - END */

    public int Round { get; private set; }
    public int Turn { get; private set; }
    public Time Time { get; private set; }

    public States GetCurrentState() => stateMachine.GetCurrentState();

    public ActionTypes GetCurrentAction() {
        if (GetCurrentState() == States.Move) return ActionTypes.Move;
        if (GetCurrentState() == States.Combat) return ActionTypes.Combat;
        if (GetCurrentState() == States.Influence) return ActionTypes.Influence;
        if (GetCurrentState() == States.ActionCard) return ActionTypes.Action;
        return ActionTypes.None;
    }

    private RoundStateMachine stateMachine;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogError("More than one instance of a singleton");
        } else {
            Instance = this;
        }

        stateMachine = new RoundStateMachine();

        Init();
    }

    private void Start() {
        RoundAction.OnRoundNextStateClick += RoundAction_OnRoundNextStateClick;
    }

    // Tests if a card's action is playable with the selected action
    public bool CanApplyAction(Card card, CardChoice actionChoice, PlayCardOptions options) {
        if (actionChoice.Id < 0) return true; // Can play always if using default action

        if (!StateAllowsCardPlay(card)) return false; // Can't play if card actions does not include round action

        // Use mana
        if (!options.SkipManaUse && card is ActionCard && actionChoice.ManaTypes.Any()) {
            if (!ManaManager.Instance.SelectedManaUsableWithChoice(actionChoice)) {
                Debug.Log("Selected mana not suitable for casting");
                return false;
            }
        }

        if (!card.CanApply(GetCurrentAction(), actionChoice)) { // Can't play if card specific restriction does not go through
            Debug.Log("Can't apply card effect");
            return false;
        }
        return true;
    }

    // Tests if a card is playable with the selected action
    public bool StateAllowsCardPlay(Card card) {
        if (!CardManager.Instance.CanPlay()) {
            Debug.Log("Player can't play card");
            return false;
        }

        States state = GetCurrentState();

        if (
            state == States.RoundStart &&
            state == States.RoundEnd
        ) {
            Debug.Log("Can't play card in the " + state + " state!");
            return false;
        }

        return true;
    }

    public bool IsDay() => Time == Time.Day;
    public bool IsNight() => Time == Time.Night;

    private void Init() {
        Turn = 0;
        Round = 0;
        // NewRound();
    }

    public void NewRound() {
        Round++;
        Turn = 0;
        Time = Round % 2 == 0 ? Time.Night : Time.Day;

        Debug.Log("New round: " + Round);
        OnNewRound?.Invoke(this, EventArgs.Empty);

        // NewTurn();
    }

    public void NewTurn() {
        Turn++;

        Debug.Log("New turn: " + Turn);
        OnNewTurn?.Invoke(this, EventArgs.Empty);

        // SetPhaseAndAction(TurnPhases.Start);
    }

    public void AttemptStateTransfer(int choiceId = -1) {
        States prev = GetCurrentState();

        stateMachine.AttemptStateTransfer(choiceId);

        Debug.Log("Round statemachine: Old state: " + prev + ". New state: " + GetCurrentState());
    }

    /* STATE MACHINE DEFINITIONS - START */
    public void RoundStateEnter() {
        OnRoundStateEnter?.Invoke(this, new RoundStateArgs { State = GetCurrentState() });
    }
    public void RoundStateExit() {
        OnRoundStateExit?.Invoke(this, new RoundStateArgs { State = GetCurrentState() });
    }

    public void RoundStartExit() {
        NewRound();
    }

    public void TurnStartExit() {
        NewTurn();
    }
    /* STATE MACHINE DEFINITIONS - END */

    /* ------------------- EVENTS ---------------------- */

    private void RoundAction_OnRoundNextStateClick(object sender, RoundAction.OnRoundNextStateClickArgs e) {
        AttemptStateTransfer(e.choiceId);
    }
}

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