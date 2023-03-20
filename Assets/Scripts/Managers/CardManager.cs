using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class TargetingCard {
    public TargetTypes TargetType;
    public Card Card;
    public CardChoice Choice;
}

public class PlayCardOptions {
    public bool SkipResolveCallbacks = false;
    public bool SkipManaUse = false;
    public Action ApplyCallback = null;
}

public class UnresolvedCard {
    public Card Card;
    public Action Callback;
    public UnresolvedCard(Card card, Action callback) {
        Card = card;
        Callback = callback;
    }
}

public class CardManager : MonoBehaviour {
    public static CardManager Instance;

    /* EVENT DEFINITIONS - START */
    public event EventHandler<OnChoiceEffectCardArgs> OnChoiceEffectCard;
    public class OnChoiceEffectCardArgs : EventArgs {
        public CardChoice Choice;
        public IChoiceEffect Card;
    }
    public event EventHandler OnEndTargeting;
    public event EventHandler<OnStartTargetingArgs> OnStartTargeting;
    public class OnStartTargetingArgs : EventArgs {
        public IHasTargeting Card;
    }
    /* EVENT DEFINITIONS - END */

    private PlayCardOptions defaultPlayCardOptions = new PlayCardOptions();

    private Stack<UnresolvedCard> unresolvedCards = new Stack<UnresolvedCard>();

    private TargetingCard targetingCard = null;
    private Card choiceCard = null;

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
        ManaManager.Instance.OnManaSourceSelected += ManaManager_OnManaSourceSelected;
    }

    public bool CanPlay() {
        return choiceCard == null && (targetingCard == null || targetingCard.TargetType == TargetTypes.Action);
    }

    public void HandleCard(Card card, CardChoice choice) {
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

    public void PlayCard(Card card, CardChoice choice, PlayCardOptions options = null) {
        options ??= defaultPlayCardOptions;

        if (!RoundManager.Instance.CanApplyAction(card, choice, options)) return;

        Debug.Log("Play card " + card.Name + " (" + choice.Name + ")");

        if (!options.SkipManaUse && card is ActionCard && choice.Super) {
            ManaManager.Instance.UseSelectedMana();
        }

        // Targeting card handling
        if (card is IHasTargeting) {
            IHasTargeting targetingCard = card as IHasTargeting;
            if (targetingCard.HasTarget(choice)) {
                this.targetingCard = new TargetingCard() {
                    TargetType = targetingCard.TargetType,
                    Card = card,
                    Choice = choice,
                };
                Player.DiscardCard(card);
                targetingCard.PreTargetSideEffect(choice);

                OnStartTargeting?.Invoke(this, new OnStartTargetingArgs { Card = targetingCard });
                return;
            }
        } 
        
        ApplyCard(card, choice, options);
    }

    public void AddUnresolvedCard(Card card, Action callback) {
        unresolvedCards.Push(new UnresolvedCard(card, callback));
    }

    private void ApplyCard(Card card, CardChoice choice, PlayCardOptions options) {
        card.Apply(choice);

        if (card is IChoiceEffect) {
            IChoiceEffect choiceEffectCard = card as IChoiceEffect;
            if (choiceEffectCard.HasChoice(choice)) {
                this.choiceCard = card;
                OnChoiceEffectCard?.Invoke(this, new OnChoiceEffectCardArgs {
                    Choice = choice,
                    Card = choiceEffectCard,
                });
            }
        } else if (unresolvedCards.Any() && unresolvedCards.Peek().Card != card) {
            ExecuteUnresolvedCards();
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

        OnEndTargeting?.Invoke(this, EventArgs.Empty);

        targetingCard = null;
        ApplyCard(targeter as Card, req.Choice, defaultPlayCardOptions);
        return true;
    }

    private void ChooseChoiceEffect(int choiceId) {
        Debug.Log("Choice " + choiceId + " chosen!");
        (choiceCard as IChoiceEffect).ApplyEffect(choiceId);
        ExecuteUnresolvedCards();
        choiceCard = null;
    }

    private void ExecuteUnresolvedCards() {
        while (unresolvedCards.Any()) {
            unresolvedCards.Pop().Callback();
        }
    }

    /* ------------------- EVENTS ---------------------- */

    private void ButtonInputManager_OnCardActionClick(object sender, ButtonInputManager.OnCardActionClickArgs e) {
        HandleCard(e.card, e.choice);
    }

    private void ButtonInputManager_OnChoiceEffectDoneClick(object sender, ButtonInputManager.OnChoiceEffectDoneClickArgs e) {
        ChooseChoiceEffect(e.choiceId);
    }

    private void ManaManager_OnManaSelected(object sender, ManaManager.OnManaSelectedArgs e) {
        if (targetingCard != null && targetingCard.TargetType == TargetTypes.Mana) {
            SetTarget(e.mana);
        }
    }

    private void ManaManager_OnManaSourceSelected(object sender, ManaManager.OnManaSourceSelectedArgs e) {
        if (targetingCard != null && targetingCard.TargetType == TargetTypes.ManaSource) {
            SetTarget(e.manaSource);
        }
    }

}
