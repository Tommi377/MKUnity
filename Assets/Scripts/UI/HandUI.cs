using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HandUI : MonoBehaviour {
    [SerializeField] private ExpandingButtonUI buttonUI;
    [SerializeField] private GameObject cardVisualPrefab;

    private Modes mode = Modes.Default;
    public enum Modes {
        Default,
        OnlySuper
    }

#nullable enable
    private CardVisual? selectedCardVisual;
#nullable disable

    private void Start() {
        MouseInput.Instance.OnCardClick += MouseInput_OnCardClick;
        MouseInput.Instance.OnNonCardClick += MouseInput_OnNonCardClick;
        MouseInput.Instance.OnManaSourceClick += MouseInput_OnManaSourceClick;

        Player.OnPlayerDrawCard += Player_OnPlayerDrawCard;
        Player.OnPlayerDiscardCard += Player_OnPlayerDiscardCard;

        EventSignalManager.OnChangeHandUIMode += EventSignalManager_OnChangeHandUIMode;
    }

    private void SelectMode(Modes mode) {
        this.mode = mode;
        DeselectCard();
    }

    private void SelectCard(CardVisual cardVisual) {
        DeselectCard();
        if (!IsCardSelectableInCurrentPhase()) return;

        if (!RoundManager.Instance.CanPlayCard(cardVisual.Card)) return;

        List<ActionChoice> choices = cardVisual.Card.ChoicesDefault(RoundManager.Instance.CurrentAction);
        choices.AddRange(cardVisual.Card.Choices(RoundManager.Instance.CurrentAction));

        if (choices.Count == 0) {
            Debug.Log("Card doesn't have any playable actions");
            return;
        }

        selectedCardVisual = cardVisual;
        selectedCardVisual.Select();
        UpdateChoices();
    }

    private void DeselectCard() {
        if (selectedCardVisual != null) {
            selectedCardVisual.Deselect();
            selectedCardVisual = null;
        }
        buttonUI.ClearButtons();
    }

    private void UpdateChoices() {
        buttonUI.ClearButtons();
        if (selectedCardVisual != null) {
            switch (mode) {
                case Modes.Default: {
                        List<ActionChoice> choices = selectedCardVisual.Card.ChoicesDefault(RoundManager.Instance.CurrentAction);
                        choices.AddRange(selectedCardVisual.Card.Choices(RoundManager.Instance.CurrentAction));

                        foreach (ActionChoice choice in choices) {
                            Button button = buttonUI.AddButton(choice.Name, () => CardActionClick(selectedCardVisual.Card, choice));
                            if (choice.Super && !ManaManager.Instance.SelectedManaUsableWithCard(selectedCardVisual.Card)) button.interactable = false;
                        }
                        break;
                    }
                case Modes.OnlySuper: {
                        List<ActionChoice> choices = selectedCardVisual.Card.Choices(RoundManager.Instance.CurrentAction);

                        foreach (ActionChoice choice in choices) {
                            if (choice.Super) buttonUI.AddButton(choice.Name, () => CardActionClick(selectedCardVisual.Card, choice));
                        }
                        break;
                    }
            }
        }
    }

    private bool IsCardSelectableInCurrentPhase() {
        return RoundManager.Instance.CurrentPhase == TurnPhases.Movement || RoundManager.Instance.CurrentPhase == TurnPhases.Action;
    }

    private void CardActionClick(Card card, ActionChoice choice) {
        ButtonInput.Instance.CardActionClick(card, choice);
        DeselectCard();
    }

    private bool HandContains(CardVisual cardVisual) {
        foreach (Transform child in transform) {
            if (child.GetComponent<CardVisual>() == cardVisual) return true;
        }
        return false;
    }

    private void MouseInput_OnCardClick(object sender, MouseInput.OnCardClickArgs e) {
        if (HandContains(e.cardVisual)) {
            SelectCard(e.cardVisual);
        } else {
            DeselectCard();
        }
    }

    private void MouseInput_OnNonCardClick(object sender, EventArgs e) {
        DeselectCard();
    }

    private void MouseInput_OnManaSourceClick(object sender, MouseInput.OnManaSourceClickArgs e) {
        UpdateChoices();
    }

    private void Player_OnPlayerDrawCard(object sender, Player.OnPlayerDrawCardArgs e) {
        CardVisual cardVisual = Instantiate(cardVisualPrefab, transform).GetComponent<CardVisual>();
        cardVisual.Init(e.card);
    }

    private void Player_OnPlayerDiscardCard(object sender, Player.OnPlayerDiscardCardArgs e) {
        // TODO: Discard the card that was actually discarded (currently only discards the first matching)
        if (selectedCardVisual != null && selectedCardVisual.Card == e.card) {
            Destroy(selectedCardVisual.gameObject);
            selectedCardVisual = null;
            return;
        } else {
            Debug.Log("HandUI discarded card not found");
        }
    }

    private void EventSignalManager_OnChangeHandUIMode(object sender, EventSignalManager.OnChangeHandUIModeArgs e) {
        SelectMode(e.mode);
    }
}
