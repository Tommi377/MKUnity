using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundActionUI : MonoBehaviour {

    [SerializeField] private StartPhaseUI startPhaseUI;
    [SerializeField] private MovePhaseUI movePhaseUI;
    [SerializeField] private ChoosePhaseUI choosePhaseUI;
    [SerializeField] private CombatUI combatPhaseUI;
    [SerializeField] private InfluenceUI influencePhaseUI;
    [SerializeField] private EndPhaseUI endPhaseUI;

    private void Start() {
        UpdateUI();

        RoundManager.Instance.OnPhaseChange += RoundManager_OnPhaseChange;

        combatPhaseUI.Initialize();
        influencePhaseUI.Initialize();
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
                    case ActionTypes.Influence:
                        influencePhaseUI.gameObject.SetActive(true);
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
