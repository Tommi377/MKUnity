using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundActionUI : MonoBehaviour {

    [SerializeField] private Transform startPhaseUI;
    [SerializeField] private Transform movePhaseUI;
    [SerializeField] private Transform choosePhaseUI;
    [SerializeField] private Transform combatPhaseUI;
    [SerializeField] private Transform endPhaseUI;

    private void Start() {
        UpdateUI();

        RoundManager.Instance.OnPhaseChange += RoundManager_OnPhaseChange;

        combatPhaseUI.GetComponent<CombatUI>().Initialize();
    }

    private void UpdateUI() {
        foreach (Transform child in transform) {
            child.gameObject.SetActive(false);
        }

        switch (RoundManager.Instance.CurrentPhase) {
            case TurnPhases.Start:
                startPhaseUI.gameObject.SetActive(true);
                break;
            case TurnPhases.Movement:
                movePhaseUI.gameObject.SetActive(true);
                break;
            case TurnPhases.ChooseAction:
                choosePhaseUI.gameObject.SetActive(true);
                break;
            case TurnPhases.Action:
                switch (RoundManager.Instance.CurrentAction) {
                    case ActionTypes.Combat:
                        combatPhaseUI.gameObject.SetActive(true);
                        break;
                }
                break;
            case TurnPhases.End:
                endPhaseUI.gameObject.SetActive(true);
                break;
        }
    }

    private void RoundManager_OnPhaseChange(object sender, RoundManager.OnPhaseChangeArgs e) {
        UpdateUI();
    }
}
