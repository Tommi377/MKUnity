using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoosePhaseUI : MonoBehaviour {
    [SerializeField] private Button selectCombatButton;
    [SerializeField] private Button selectInfluenceButton;
    [SerializeField] private Button selectSkipButton;

    private void Awake() {
        selectCombatButton.onClick.AddListener(() => { ButtonInputManager.Instance.ActionChooseClick(ActionTypes.Combat); });
        selectInfluenceButton.onClick.AddListener(() => { ButtonInputManager.Instance.ActionChooseClick(ActionTypes.Influence); });
        selectSkipButton.onClick.AddListener(() => { ButtonInputManager.Instance.ActionChooseClick(ActionTypes.None); });
    }

    private void OnEnable() {
        if (GameManager.Instance == null || GameManager.Instance.CurrentPlayer == null) return;

        List<ActionTypes> actionTypes = GameManager.Instance.CurrentPlayer.GetPossibleActions();
        selectCombatButton.gameObject.SetActive(actionTypes.Contains(ActionTypes.Combat));
        selectInfluenceButton.gameObject.SetActive(actionTypes.Contains(ActionTypes.Influence));
        selectSkipButton.gameObject.SetActive(actionTypes.Contains(ActionTypes.None));
    }
}
