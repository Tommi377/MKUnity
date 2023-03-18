using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CentralInfoUI : MonoBehaviour {
    public enum States { None, EffectChoice }

    private States state = States.None;

    [SerializeField] private TMP_Text centralInfoText;
    [SerializeField] private ExpandingButtonUI expandingButtonUI;

    private void Awake() {
        Disable();
    }

    private void Start() {
        CardManager.Instance.OnChoiceEffectCard += CardManager_OnChoiceEffectCard;
    }

    private void SetEffectChoices(IChoiceEffect card, CardChoice choice, Action callback) {
        Enable();
        expandingButtonUI.ClearButtons();
        state = States.EffectChoice;

        centralInfoText.SetText(card.GetEffectChoicePrompt(choice));
        foreach ((string text, Action onClick) in card.EffectChoices(choice)) {
            expandingButtonUI.AddButton(text, () => {
                // TODO: Move the onclick logic away from the UI
                onClick();
                callback();
                ButtonInputManager.Instance.ChoiceEffectDoneClick();
                Disable();
            });
        }
    }

    private void Enable() {
        centralInfoText.gameObject.SetActive(true);
        expandingButtonUI.gameObject.SetActive(true);
    }

    private void Disable() {
        centralInfoText.gameObject.SetActive(false);
        expandingButtonUI.gameObject.SetActive(false);
    }

    private void CardManager_OnChoiceEffectCard(object sender, CardManager.OnChoiceEffectCardArgs e) {
        Debug.Log("sgeeges");
        SetEffectChoices(e.Card, e.Choice, e.Callback);
    }
}
