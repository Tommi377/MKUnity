using TMPro;
using UnityEngine;

public class InfoTextUI : MonoBehaviour {
    [SerializeField] private TMP_Text phaseInfoText;
    [SerializeField] private TMP_Text debugPlayerInfoText;

    private Combat.States? combatState = null;

    private void Start() {
        RoundManager.Instance.OnRoundStateEnter += Instance_OnRoundStateEnter;

        Combat.OnCombatStateEnter += Combat_OnCombatStateEnter;
        Combat.OnCombatEnd += Combat_OnCombatEnd;

        UpdateActionText();
    }

    private void Update() {
        string debugText = "";
        debugText += $"Move: {GameManager.Instance.CurrentPlayer.Movement}\n";
        debugText += $"Influence: {GameManager.Instance.CurrentPlayer.Influence}\n\n";
        debugText += $"Round: {RoundManager.Instance.Round}\n";
        debugText += $"Turn: {RoundManager.Instance.Turn}\n";
        debugText += $"Time: {RoundManager.Instance.Time}\n";

        debugPlayerInfoText.SetText(debugText);
    }

    private void UpdateActionText() {
        string text = "Current state:\n" + RoundManager.Instance.GetCurrentState();

        text += "\nCurrent action:\n" + RoundManager.Instance.GetCurrentAction();

        if (combatState != null) {
            text += "\nCurrent action:\n" + combatState;
        }

        phaseInfoText.SetText(text);
    }

    private void Instance_OnRoundStateEnter(object sender, RoundManager.RoundStateArgs e) {
        UpdateActionText();
    }

    private void Combat_OnCombatStateEnter(object sender, Combat.OnCombatStateEnterArgs e) {
        combatState = e.State;
        UpdateActionText();
    }

    private void Combat_OnCombatEnd(object sender, Combat.OnCombatResultArgs e) {
        combatState = null;
        UpdateActionText();
    }
}
