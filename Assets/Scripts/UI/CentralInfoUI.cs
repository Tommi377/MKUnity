using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CentralInfoUI : MonoBehaviour {

    [SerializeField] private TMP_Text centralInfoText;
    [SerializeField] private ExpandingButtonUI expandingButtonUI;

    private void Awake() {
        Disable();
    }

    private void Start() {
        CardManager.Instance.OnStartTargeting += CardManager_OnPlayTargetCard;
        CardManager.Instance.OnEndTargeting += CardManager_OnEndTargeting;
        CardManager.Instance.OnChoiceEffectCard += CardManager_OnChoiceEffectCard;
    }

    private void SetTargetText(IHasTargeting card) {
        Enable();
        expandingButtonUI.ClearButtons();

        centralInfoText.SetText("Choose a " + card.TargetType + " as a target");
    }

    private void SetEffectChoices(IChoiceEffect card, CardChoice choice) {
        Enable();
        expandingButtonUI.ClearButtons();

        centralInfoText.SetText(card.GetEffectChoicePrompt(choice));
        List<string> choiceTexts = card.EffectChoices(choice);
        for (int i = 0; i < choiceTexts.Count; i++) {
            int choiceIndex = i;
            expandingButtonUI.AddButton(choiceTexts[i], () => {
                ButtonInputManager.Instance.ChoiceEffectDoneClick(choiceIndex);
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

    private void CardManager_OnPlayTargetCard(object sender, CardManager.OnStartTargetingArgs e) {
        SetTargetText(e.Card);
    }

    private void CardManager_OnEndTargeting(object sender, EventArgs e) {
        Disable();
    }

    private void CardManager_OnChoiceEffectCard(object sender, CardManager.OnChoiceEffectCardArgs e) {
        SetEffectChoices(e.Card, e.Choice);
    }
}
