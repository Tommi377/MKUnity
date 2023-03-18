using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class TargetingCard {
    public TargetTypes TargetType;
    public Card Card;
    public CardChoice Choice;
}

public class PlayCardOptions {
    public bool SkipManaUse = false;
}

public class CardManager : MonoBehaviour {
    public static CardManager Instance;

    public event EventHandler<OnChoiceEffectCardArgs> OnChoiceEffectCard;
    public class OnChoiceEffectCardArgs : EventArgs {
        public CardChoice choice;
        public IChoiceEffect card;
    }

    private bool choiceEffectMidresolve = false;
    private PlayCardOptions defaultPlayCardOptions = new PlayCardOptions();

#nullable enable
    private TargetingCard? targetingCard = null;
#nullable disable

    private Player Player => GameManager.Instance.CurrentPlayer;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogError("More than one instance of a singleton");
        } else {
            Instance = this;
        }
    }

    private void Start() {
        ButtonInputManager.Instance.OnCardActionClick += ButtonInputManager_OnCardActionClick;
        ButtonInputManager.Instance.OnChoiceEffectDoneClick += ButtonInputManager_OnChoiceEffectDoneClick;

        ManaManager.Instance.OnManaSelected += ManaManager_OnManaSelected;
    }

    public bool CanPlay() {
        return !choiceEffectMidresolve && (targetingCard == null || targetingCard.TargetType == TargetTypes.Action);
    }

    public void PlayCard(Card card, CardChoice choice, PlayCardOptions options = null) {
        options ??= defaultPlayCardOptions;

        if (!RoundManager.Instance.CanApplyAction(card, choice, options)) return;

        Debug.Log("Play card " + card.Name + " (" + choice.Name + ")");

        if (!options.SkipManaUse && card is ActionCard && choice.Super) {
            ManaManager.Instance.UseSelectedMana();
        }

        // TODO: Handle back to back targeting (eg concentration to concentartion)
        // Targeting card handling
        if (card is IHasTargeting) {
            IHasTargeting targetingCard = card as IHasTargeting;
            if (targetingCard.HasTarget(choice)) {
                this.targetingCard = new TargetingCard() {
                    TargetType = targetingCard.TargetType,
                    Card = card,
                    Choice = choice
                };

                Player.DiscardCard(card);

                targetingCard.PreTargetSideEffect(choice);
            }
        }

        if (targetingCard == null) {
            ApplyCard(card, choice);
        }
    }

    private void HandleCard(Card card, CardChoice choice) {
        if (targetingCard == null) {
            PlayCard(card, choice);
        } else if (targetingCard.TargetType == TargetTypes.Card) {
            SetTarget(card);
        } else if (targetingCard.TargetType == TargetTypes.Action) {
            SetTarget((card, choice));
        } else {
            Debug.Log("Can't play a card when another card is unresolved!!");
        }
    }

    private void ApplyCard(Card card, CardChoice choice) {
        card.Apply(choice);

        if (card is IChoiceEffect) {
            IChoiceEffect choiceCard = card as IChoiceEffect;
            if (choiceCard.HasChoice(choice)) {
                choiceEffectMidresolve = true;
                OnChoiceEffectCard?.Invoke(this, new OnChoiceEffectCardArgs {
                    choice = choice,
                    card = choiceCard
                });
            }
        }

        Player.DiscardCard(card);
    }

    private bool SetTarget<T>(T target) {
        TargetingCard req = targetingCard;

        if (req == null) {
            Debug.Log("TestCardRequirement: Not card in midresolve");
            return true;
        }

        ITargetingCard<T> targeter = req.Card as ITargetingCard<T>;

        if (targeter == null) {
            Debug.Log("Bad targeting cad");
            return false;
        }

        if (!targeter.ValidTarget(req.Choice, target)) {
            Debug.Log("TestCardRequirement: Requirement not met");
            return false;
        }

        targeter.TargetSideEffect(req.Choice, target);

        targetingCard = null;
        ApplyCard(targeter as Card, req.Choice);
        return true;
    }

    /* ------------------- EVENTS ---------------------- */

    private void ButtonInputManager_OnCardActionClick(object sender, ButtonInputManager.OnCardActionClickArgs e) {
        HandleCard(e.card, e.choice);
    }

    private void ButtonInputManager_OnChoiceEffectDoneClick(object sender, EventArgs e) {
        choiceEffectMidresolve = false;
    }

    private void ManaManager_OnManaSelected(object sender, ManaManager.OnManaSelectedArgs e) {
        if (targetingCard != null && targetingCard.TargetType == TargetTypes.Mana) {
            SetTarget(e.mana);
        }
    }

}
