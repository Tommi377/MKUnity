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
        MouseInputManager.Instance.OnCardClick += MouseInput_OnCardClick;
        MouseInputManager.Instance.OnNonCardClick += MouseInput_OnNonCardClick;

        Player.OnPlayerDrawCard += Player_OnPlayerDrawCard;
        Player.OnPlayerDiscardCard += Player_OnPlayerDiscardCard;

        ManaManager.Instance.OnManaSelected += ManaManager_OnManaSelected;
        ManaManager.Instance.OnManaDeselected += ManaManager_OnManaDeselected;

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

        if (!cardVisual.Card.HasPlayableChoices(RoundManager.Instance.CurrentAction)) {
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
                        List<CardChoice> choices = selectedCardVisual.Card.Choices(RoundManager.Instance.CurrentAction);

                        foreach (CardChoice choice in choices) {
                            bool interactable = !choice.Super || ManaManager.Instance.SelectedManaUsableWithCard(selectedCardVisual.Card);
                            Button button = buttonUI.AddButton(choice.Name, () => CardActionClick(selectedCardVisual.Card, choice), interactable);
                        }
                        break;
                    }
                case Modes.OnlySuper: {
                        List<CardChoice> choices = selectedCardVisual.Card.Choices(RoundManager.Instance.CurrentAction);

                        foreach (CardChoice choice in choices) {
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

    private void CardActionClick(Card card, CardChoice choice) {
        ButtonInputManager.Instance.CardActionClick(card, choice);
        DeselectCard();
    }

    private bool HandContains(CardVisual cardVisual) {
        foreach (Transform child in transform) {
            if (child.GetComponent<CardVisual>() == cardVisual) return true;
        }
        return false;
    }

    private void MouseInput_OnCardClick(object sender, MouseInputManager.OnCardClickArgs e) {
        if (HandContains(e.cardVisual)) {
            SelectCard(e.cardVisual);
        } else {
            DeselectCard();
        }
    }

    private void MouseInput_OnNonCardClick(object sender, EventArgs e) {
        DeselectCard();
    }

    private void ManaManager_OnManaSelected(object sender, ManaManager.OnManaSelectedArgs e) {
        UpdateChoices();
    }

    private void ManaManager_OnManaDeselected(object sender, EventArgs e) {
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
