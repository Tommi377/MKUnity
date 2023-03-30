using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HandUI : MonoBehaviour {
    [SerializeField] private Transform cardContainer;
    [SerializeField] private ExpandingButtonUI buttonUI;
    [SerializeField] private GameObject cardVisualPrefab;
    [SerializeField] private Button changeStateButton;
    [SerializeField] private TMP_Text changeStateButtonText;

    private State state = State.Hand;
    private enum State { Hand, Unit }

    private SelectionMode mode = SelectionMode.Default;
    public enum SelectionMode {
        Default,
        OnlySuper
    }

#nullable enable
    private CardVisual? selectedCardVisual;
#nullable disable

    private void Awake() {
        changeStateButton.onClick.AddListener(()  => SetState(state == State.Hand ? State.Unit : State.Hand));
    }

    private void Start() {
        MouseInputManager.Instance.OnCardClick += MouseInput_OnCardClick;
        MouseInputManager.Instance.OnNonCardClick += MouseInput_OnNonCardClick;

        Player.OnPlayerDrawCard += Player_OnPlayerDrawCard;
        Player.OnPlayerDiscardCard += Player_OnPlayerDiscardCard;
        Player.OnPlayerTrashCard += Player_OnPlayerTrashCard;
        Player.OnPlayerDisbandUnit += Player_OnPlayerDisbandUnit;

        ManaManager.Instance.OnManaSelected += ManaManager_OnManaSelected;
        ManaManager.Instance.OnManaDeselected += ManaManager_OnManaDeselected;

        RoundManager.Instance.OnPhaseChange += RoundManager_OnPhaseChange;

        UnitManager.Instance.OnUnitRecruit += UnitManager_OnUnitRecruit;

        EventSignalManager.OnChangeHandUIMode += EventSignalManager_OnChangeHandUIMode;
    }

    private void SetState(State state) {
        DeselectCard();

        this.state = state;
        changeStateButtonText.SetText(state == State.Hand ? "Units" : "Hand");
        UpdateHand();
    }

    private void UpdateHand() {
        ClearHand();
        switch (state) {
            case State.Hand:
                GameManager.Instance.CurrentPlayer.GetHand().ForEach((card) => AddCard(card));
                break;
            case State.Unit:
                GameManager.Instance.CurrentPlayer.GetUnits().ForEach((card) => AddCard(card));
                break;
        }
    }

    private void ClearHand() {
        foreach (Transform child in cardContainer) {
            Destroy(child.gameObject);
        }
    }

    private void SelectMode(SelectionMode mode) {
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
                case SelectionMode.Default: {
                        List<CardChoice> choices = selectedCardVisual.Card.GetChoices(RoundManager.Instance.CurrentAction);

                        foreach (CardChoice choice in choices) {
                            bool interactable = !choice.ManaTypes.Any() || ManaManager.Instance.SelectedManaUsableWithChoice(choice);
                            Color? manaColor = choice.ManaTypes.Any() ? Mana.GetColor(choice.ManaTypes[0]) : null;
                            Button button = buttonUI.AddButton(
                                choice.Name, () => CardActionClick(selectedCardVisual.Card, choice),
                                new ExpandingButtonUI.Options() { Interactable = interactable, BackgroundColor = manaColor }
                            );
                        }
                        break;
                    }
                case SelectionMode.OnlySuper: {
                        List<CardChoice> choices = selectedCardVisual.Card.GetChoices(RoundManager.Instance.CurrentAction);
                        foreach (CardChoice choice in choices) {
                            Color? manaColor = choice.ManaTypes.Any() ? Mana.GetColor(choice.ManaTypes[0]) : null;
                            if (choice.ManaTypes.Any()) buttonUI.AddButton(
                                choice.Name, () => CardActionClick(selectedCardVisual.Card, choice),
                                new ExpandingButtonUI.Options() { BackgroundColor = manaColor }
                            );
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
        foreach (Transform child in cardContainer) {
            if (child.GetComponent<CardVisual>() == cardVisual) return true;
        }
        return false;
    }

    private void AddCard(Card card) {
        CardVisual cardVisual = Instantiate(cardVisualPrefab, cardContainer).GetComponent<CardVisual>();
        cardVisual.Init(card);
    }

    private void RemoveCard(Card card) {
        foreach (Transform child in cardContainer) {
            if (child.GetComponent<CardVisual>().Card == card) {
                Destroy(child.gameObject);
                return;
            }
        }
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

    private void RoundManager_OnPhaseChange(object sender, RoundManager.OnPhaseChangeArgs e) {
        DeselectCard();
    }

    private void UnitManager_OnUnitRecruit(object sender, UnitManager.OnUnitRecruitArgs e) {
        UpdateHand();
    }

    private void Player_OnPlayerDrawCard(object sender, Player.CardEventArgs e) {
        if (state == State.Hand) {
            AddCard(e.Card);
        }
    }

    private void Player_OnPlayerDiscardCard(object sender, Player.CardEventArgs e) {
        // TODO: Discard the card that was actually discarded (currently only discards the first matching)
        if (selectedCardVisual != null && selectedCardVisual.Card == e.Card) {
            Destroy(selectedCardVisual.gameObject);
            selectedCardVisual = null;
            return;
        } else {
            Debug.Log("HandUI discarded card not found");
            RemoveCard(e.Card);
        }
    }

    private void Player_OnPlayerTrashCard(object sender, Player.CardEventArgs e) {
        RemoveCard(e.Card);
    }

    private void Player_OnPlayerDisbandUnit(object sender, Player.CardEventArgs e) {
        RemoveCard(e.Card);
    }

    private void EventSignalManager_OnChangeHandUIMode(object sender, EventSignalManager.OnChangeHandUIModeArgs e) {
        SelectMode(e.mode);
    }
}
