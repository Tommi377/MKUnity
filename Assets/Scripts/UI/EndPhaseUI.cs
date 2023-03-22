using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndPhaseUI : MonoBehaviour {
    [SerializeField] private ExpandingButtonUI expandingButtonUI;

    private void OnEnable() {
        UpdateUI();
    }

    private void UpdateUI() {
        expandingButtonUI.ClearButtons();
        if (GameManager.Instance == null || GameManager.Instance.CurrentPlayer == null) return;

        foreach (BaseAction action in GameManager.Instance.CurrentPlayer.GetEndOfTurnActions()) {
            expandingButtonUI.AddButton(action.Name, (button) => {
                action.Action();
                button.interactable = false;
            });
        }

        expandingButtonUI.AddButton("End\nTurn", ButtonInputManager.Instance.EndEndPhaseClick);
    }
}
