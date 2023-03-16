using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoosePhaseUI : MonoBehaviour {
    [SerializeField] private Button selectCombatButton;
    [SerializeField] private Button selectInfluenceButton;
    [SerializeField] private Button selectSkipButton;

    private void Awake() {
        selectCombatButton.onClick.AddListener(() => { ButtonInput.Instance.ActionChooseClick(ActionTypes.Combat); });
        selectInfluenceButton.onClick.AddListener(() => { ButtonInput.Instance.ActionChooseClick(ActionTypes.Influence); });
        selectSkipButton.onClick.AddListener(() => { ButtonInput.Instance.ActionChooseClick(ActionTypes.None); });
    }
}
