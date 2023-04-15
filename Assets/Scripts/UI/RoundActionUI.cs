using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoundActionUI : MonoBehaviour {
    [SerializeField] private ExpandingButtonUI buttonContainer;
    [SerializeField] private TMP_Text roundText;

    [SerializeField] private SupplyUI supplyUI;
    [SerializeField] private CombatUI combatPhaseUI;
    [SerializeField] private InfluenceUI influencePhaseUI;

    private Player Player => GameManager.Instance.CurrentPlayer;

    private void Start() {
        UpdateUI();

        RoundManager.Instance.OnRoundStateEnter += RoundManager_OnRoundStateEnter;
    }

    private void UpdateUI() {
        RoundManager.States state = RoundManager.Instance.GetCurrentState();

        buttonContainer.ClearButtons();
        roundText.SetText(GetText(state));

        combatPhaseUI.gameObject.SetActive(false);
        influencePhaseUI.gameObject.SetActive(false);

        switch (state) {
            case RoundManager.States.Combat:
                combatPhaseUI.gameObject.SetActive(true);
                break;
            case RoundManager.States.Influence:
                influencePhaseUI.gameObject.SetActive(true);
                break;
            case RoundManager.States.LevelUp:
                supplyUI.OpenSpecial(state);
                break;
            default:
                buttonContainer.gameObject.SetActive(true);
                roundText.gameObject.SetActive(true);

                UpdateButtons(state);
                break;
        }
    }

    private string GetText(RoundManager.States state) {
        switch (state) {
            case RoundManager.States.RoundStart: return "Start of Round";
            case RoundManager.States.TurnStart: return "Start of Turn";
            case RoundManager.States.TurnChoice: return "Choose whether to rest";
            case RoundManager.States.NormalRest: return "Normal rest";
            case RoundManager.States.SlowRest: return "Slow rest";
            case RoundManager.States.Move: return "Move phase";
            case RoundManager.States.PreAction: return "Choose action";
            case RoundManager.States.Influence: return "Influence phase";
            case RoundManager.States.Combat: return "Combat phase";
            case RoundManager.States.TurnEnd: return "End of Turn";
            case RoundManager.States.RoundEnd: return "End of Round";
            default: return "";
        }
    }

    private void UpdateButtons(RoundManager.States state) {
        switch (state) {
            case RoundManager.States.RoundStart:
                buttonContainer.AddButton("Start\nRound", () => RoundAction.RoundNextStateClick(this));
                break;
            case RoundManager.States.TurnStart:
                SetTurnStartUI();
                break;
            case RoundManager.States.TurnChoice:
                buttonContainer.AddButton("Start\nAction\nPhase", () => RoundAction.RoundNextStateClick(this, 0), new ExpandingButtonUI.Options { Interactable = !Player.MustSlowRest() });
                buttonContainer.AddButton("Rest", () => RoundAction.RoundNextStateClick(this, 1), new ExpandingButtonUI.Options { Interactable = Player.CanNormalRest() || Player.MustSlowRest() });
                buttonContainer.AddButton("End\nRound", () => RoundAction.RoundNextStateClick(this, 2), new ExpandingButtonUI.Options { Interactable = Player.CanEndRound() });
                break;
            case RoundManager.States.NormalRest:
                break;
            case RoundManager.States.SlowRest:
                break;
            case RoundManager.States.Move:
                buttonContainer.AddButton("End\nMovement", () => RoundAction.RoundNextStateClick(this));
                break;
            case RoundManager.States.PreAction:
                SetPreActionUI();
                break;
            case RoundManager.States.ActionCard:
                break;
            case RoundManager.States.TurnEnd:
                buttonContainer.AddButton("End\nTurn", () => RoundAction.RoundNextStateClick(this));
                break;
            case RoundManager.States.RoundEnd:
                buttonContainer.AddButton("End\nRound", () => RoundAction.RoundNextStateClick(this));
                break;
        }
    }

    private void SetTurnStartUI() {
        foreach (BaseAction action in GameManager.Instance.CurrentPlayer.GetStartOfTurnActions()) {
            buttonContainer.AddButton(action.Name, (button) => {
                action.Action();
                button.interactable = false;
            });
        }

        buttonContainer.AddButton("Start\nTurn", () =>  RoundAction.RoundNextStateClick(this));
    }

    private void SetPreActionUI() {
        List<ActionTypes> actionTypes = GameManager.Instance.GetPossibleActions(); 
        if (actionTypes.Contains(ActionTypes.Combat)) buttonContainer.AddButton("Combat", () => RoundAction.RoundNextStateClick(this, 0));
        if (actionTypes.Contains(ActionTypes.Influence)) buttonContainer.AddButton("Influence", () => RoundAction.RoundNextStateClick(this, 1));
        if (actionTypes.Contains(ActionTypes.Action)) buttonContainer.AddButton("Card Action", () => RoundAction.RoundNextStateClick(this, 2));
        buttonContainer.AddButton("Skip", () => RoundAction.RoundNextStateClick(this, 3));
    }

    private void RoundManager_OnRoundStateEnter(object sender, RoundManager.RoundStateArgs e) {
        UpdateUI();
    }
}

public static class RoundAction {
    public static event EventHandler<OnRoundNextStateClickArgs> OnRoundNextStateClick;
    public class OnRoundNextStateClickArgs : EventArgs {
        public int choiceId;
    }

    public static void ResetStaticData() {
        OnRoundNextStateClick = null;
    }

    public static void RoundNextStateClick(object sender, int choiceId = -1) {
        OnRoundNextStateClick?.Invoke(sender, new OnRoundNextStateClickArgs { choiceId = choiceId });
    }
}