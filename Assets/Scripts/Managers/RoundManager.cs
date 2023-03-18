using System;
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

    public int Round { get; private set; }
    public Time Time { get; private set; }
    public ActionTypes CurrentAction { get; private set; }
    public TurnPhases CurrentPhase { get; private set; }

    /* EVENT DEFINITIONS - START */
    public event EventHandler OnNewRound;
    public event EventHandler<OnPhaseChangeArgs> OnPhaseChange;
    public class OnPhaseChangeArgs : EventArgs {
        public TurnPhases phase;
        public ActionTypes actionType;
    }
    /* EVENT DEFINITIONS - END */

    private void Awake() {
        InitRound();

        if (Instance != null && Instance != this) {
            Debug.LogError("More than one instance of a singleton");
        } else {
            Instance = this;
        }
    }

    private void Start() {

        Combat.OnCombatEnd += Combat_OnCombatEnd;

        ButtonInputManager.Instance.OnEndStartPhaseClick += ButtonInput_OnEndStartPhaseClick;
        ButtonInputManager.Instance.OnEndMovementClick += ButtonInput_OnEndMovementClick;
        ButtonInputManager.Instance.OnEndEndPhaseClick += ButtonInput_OnEndEndPhaseClick;
    }

    // Tests if a card's action is playable with the selected action
    public bool CanApplyAction(Card card, CardChoice actionChoice) {
        if (actionChoice.Id < 0) return true; // Can play always if using default action

        if (!CanPlayCard(card)) return false; // Can't play if card actions does not include round action

        // Action card specific
        if (card is ActionCard && actionChoice.Super) {
            ActionCard actionCard = card as ActionCard;
            if (!ManaManager.Instance.SelectedManaUsableWithCard(actionCard)) {
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
        if (!GameManager.Instance.CurrentPlayer.CanPlayCard()) {
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

    private void InitRound() {
        Round = 1;
        Time = Time.Day;
        SetPhaseAndAction(TurnPhases.Start);
    }

    private void NewRound() {
        Round++;
        Time = Round % 2 == 0 ? Time.Night : Time.Day;
        SetPhaseAndAction(TurnPhases.Start);

        ManaManager.Instance.RoundStartSetup();

        OnNewRound?.Invoke(this, EventArgs.Empty);
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

        OnPhaseChange?.Invoke(this, new OnPhaseChangeArgs { phase = CurrentPhase, actionType = CurrentAction });
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
        NewRound();
    }
}
