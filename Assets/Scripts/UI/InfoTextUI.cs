using TMPro;
using UnityEngine;

public class InfoTextUI : MonoBehaviour {
    [SerializeField] private TMP_Text phaseInfoText;
    [SerializeField] private TMP_Text debugPlayerInfoText;

    private CombatPhases? combatPhase = null;

    private void Start() {
        RoundManager.Instance.OnPhaseChange += RoundManager_OnPhaseChange;

        Combat.OnCombatPhaseChange += Combat_OnCombatPhaseChange;
        Combat.OnCombatEnd += Combat_OnCombatEnd;

        updateActionText();
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

    private void updateActionText() {
        TurnPhases phase = RoundManager.Instance.CurrentPhase;
        ActionTypes action = RoundManager.Instance.CurrentAction;
        string text = "Current phase:\n" + phase;

        text += "\nCurrent action:\n" + action;

        if (combatPhase != null) {
            text += "\nCurrent action:\n" + combatPhase;
        }

        phaseInfoText.SetText(text);
    }

    private void RoundManager_OnPhaseChange(object sender, RoundManager.OnPhaseChangeArgs e) {
        updateActionText();
    }

    private void Combat_OnCombatPhaseChange(object sender, Combat.OnCombatPhaseChangeArgs e) {
        combatPhase = e.phase;
        updateActionText();
    }

    private void Combat_OnCombatEnd(object sender, Combat.OnCombatEndArgs e) {
        combatPhase = null;
        updateActionText();
    }
}
