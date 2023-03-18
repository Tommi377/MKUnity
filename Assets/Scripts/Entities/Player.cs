using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class TargetingCard {
    public TargetTypes TargetType;
    public Card Card;
    public CardChoice Choice;
}

public class Player : Entity {
    [SerializeField] private CardSO woundSO;
    [SerializeField] private CardListSO startingDeckSO;

    public override EntityTypes EntityType { get { return EntityTypes.Player; } }

    /* EVENT DEFINITIONS - START */
    public static event EventHandler<OnPlayerDrawCardArgs> OnPlayerDrawCard;
    public class OnPlayerDrawCardArgs : EventArgs {
        public Player player;
        public Card card;
    }
    public static event EventHandler<OnPlayerDiscardCardArgs> OnPlayerDiscardCard;
    public class OnPlayerDiscardCardArgs : EventArgs {
        public Player player;
        public Card card;
    }
    public static event EventHandler OnShuffleDiscardToDeck;
    public static event EventHandler<OnInventoryUpdateArgs> OnInventoryUpdate;
    public class OnInventoryUpdateArgs : EventArgs {
        public Player player;
        public Inventory inventory;
    }

    public static void ResetStaticData() {
        OnPlayerDrawCard = null;
        OnPlayerDiscardCard = null;
        OnShuffleDiscardToDeck = null;
    }
    /* EVENT DEFINITIONS - END */

    // Actions
    public int Movement { get; private set; } = 0;
    public int Influence { get; private set; } = 0;

    private Inventory inventory = new Inventory();
    private Deck deck;
    private List<Card> hand = new List<Card>();
    private List<Card> discard = new List<Card>();

#nullable enable
    private TargetingCard? targetingCard = null;
#nullable disable

    // Stats
    public int Level { get; private set; } = 1;
    public int Fame { get; private set; } = 0;
    public int Reputation { get; private set; } = 0;
    public int Armor => levelToArmor[Level - 1];
    public int HandLimit => levelToHandLimit[Level - 1];

    private int[] levelThreshold = new int[] { 2, 7, 14, 23, 34, 47, 62, 79, 98, 119 };
    private int[] levelToArmor = new int[] { 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5 };
    private int[] levelToHandLimit = new int[] { 5, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7 };

    // Combat
    // public int Attack = 0;
    // public int IceAttack = 0;
    // public int FireAttack = 0;
    // public int ColdFireAttack = 0;

    // public int RangedAttack = 0;
    // public int IceRangedAttack = 0;
    // public int FireRangedAttack = 0;

    // public int SiegeAttack = 0;
    // public int IceSiegeAttack = 0;
    // public int FireSiegedAttack = 0;

    // public int Block = 0;
    // public int IceBlock = 0;
    // public int FireBlock = 0;

    private void Awake() {
        deck = new Deck(startingDeckSO);
    }

    private void Start() {
        Combat.OnCombatEnd += Combat_OnCombatEnd;

        MouseInput.Instance.OnHexClick += MouseInput_OnHexClick;

        ButtonInput.Instance.OnEndMovementClick += ButtonInput_OnEndMovementClick;
        ButtonInput.Instance.OnDrawStartHandClick += ButtonInput_OnDrawStartHandClick;
        ButtonInput.Instance.OnCardActionClick += ButtonInput_OnCardActionClick;
        ButtonInput.Instance.OnShuffleDiscardClick += ButtonInput_OnShuffleDiscardClick;
        ButtonInput.Instance.OnDrawCardClick += ButtonInput_OnDrawCardClick;
    }

    public Inventory GetInventory() => inventory;
    public int GetDeckCount() => deck.Count;
    public int GetDiscardCount() => discard.Count;
    public bool IsInCombat() => GameManager.Instance.Combat != null && GameManager.Instance.Combat.Player == this;

    public ReadOnlyCollection<Card> DiscardPile => discard.AsReadOnly();

    public bool TryGetCombat(out Combat combat) {
        if (IsInCombat()) {
            combat = GameManager.Instance.Combat;
            return true;
        }
        combat = null;
        return false;
    }

    public void TakeWounds(int amount) {
        Debug.Log("Taking " + amount + " wounds!");
        for (int i = 0; i < amount; i++) {
            Wound wound = Card.GetCardFromSO(woundSO) as Wound;
            AddCardToHand(wound);
        }
    }

    public bool CanPlayCard() {
        return targetingCard == null || targetingCard.TargetType == TargetTypes.Action;
    }

    public void AddMovement(int movement) {
        Movement += movement;
    }

    public void ReduceMovement(int movement) {
        Movement -= movement;
    }

    public void AddInfluence(int influence) {
        Influence += influence;
    }

    public void ReduceInfluence(int influence) {
        Influence -= influence;
    }

    public void GainCrystal(ManaSource.Types type) {
        inventory.AddCrystal(type);
        OnInventoryUpdate?.Invoke(this, new OnInventoryUpdateArgs { player = this, inventory = inventory });
    }

    public void GainMana(ManaSource.Types type) {
        inventory.AddMana(type);
        OnInventoryUpdate?.Invoke(this, new OnInventoryUpdateArgs { player = this, inventory = inventory });
    }

    private bool TryMove(Hex hex) {
        if (HexMap.HexIsNeigbor(Position, hex.Position)) {
            int moveCost = hex.GetMoveCost();
            Debug.Log("Attempting to move to hex with move cost of " + moveCost);
            if (Movement >= moveCost) {
                if (Move(hex)) {
                    ReduceMovement(moveCost);
                    return true;
                }
            } else {
                Debug.Log("Not enough movement");
            }
        } else {
            Debug.Log("Attempting to move into non neighbor tile");
        }
        return false;
    }

    private void EndMovement() {
        // Find possible actions for action phase
        if (!TryGetHex(out Hex currentHex)) {
            Debug.Log("Hex doesn't exist");
            return;
        }

        // TODO: restrict actions depending on whether they are executable
        List<ActionTypes> actions = new List<ActionTypes>();
        switch (currentHex.StructureType) {
            case HexStructureTypes.MagicalGlade:
                // TODO: add influence if demons in recruit row
                break;
            case HexStructureTypes.Village:
            case HexStructureTypes.MageTower:
            case HexStructureTypes.Monastery:
            case HexStructureTypes.Keep:
                if (currentHex.Entities.Any(e => e.IsAggressive())) {
                    actions.Add(ActionTypes.Combat);
                } else {
                    actions.Add(ActionTypes.Influence);
                }
                break;
            case HexStructureTypes.AncientRuins:
                // TODO: add ancient ruins stuff
                break;
            default:
                break;
        }
        ResetValues();
    }

    public void EndAction() {
        ResetValues();
    }

    private void LevelUp() {
        Level += 1;
    }

    private void GainFame(int amount) {
        // TODO: account for multiple level ups at once
        Fame += amount;
        if (levelThreshold[Level - 1] < Fame) {
            LevelUp();
        }
    }


    private void HandleCard(Card card, CardChoice choice) {
        if (targetingCard == null) {
            PlayCard(card, choice);
        } else if (targetingCard.TargetType == TargetTypes.Card) {
            SetTarget(card);
        } else if (targetingCard.TargetType == TargetTypes.Action) {
            SetTarget((card, choice));
        }
    }

    private void PlayCard(Card card, CardChoice choice) {
        if (targetingCard != null) {
            Debug.Log("Can't play a card when another card is unresolved!!");
            return;
        }

        if (!RoundManager.Instance.CanApplyAction(card, choice)) return;

        Debug.Log("Play card " + card.Name + " (" + choice.Name + ")");

        if (card is IHasTargeting) {
            IHasTargeting targetingCard = card as IHasTargeting;
            if (targetingCard.HasTarget(choice)) {
                this.targetingCard = new TargetingCard() {
                    TargetType = targetingCard.TargetType,
                    Card = card,
                    Choice = choice
                };
                DiscardCard(card);
                targetingCard.PreTargetSideEffect();
            }
        } 
        
        if (targetingCard == null) {
            ApplyCard(card, choice);
        }
    }

    private void ApplyCard(Card card, CardChoice choice) {
        card.Apply(choice);

        // TODO: Ponder whether its good to roll manasource here
        if (card is ActionCard && choice.Super) {
            ManaManager.Instance.UseMana();
        }

        DiscardCard(card);
    }

    private bool SetTarget<T>(T target) {
        TargetingCard req = targetingCard;
        ITargetingCard<T> targeter = req.Card as ITargetingCard<T>;

        if (req == null) {
            Debug.Log("TestCardRequirement: Not card in midresolve");
            return true;
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

    private void DrawToHandLimit() {
        int drawAmount = Mathf.Max(HandLimit - hand.Count, 0);
        DrawCards(drawAmount);
    }

    private void DrawCards(int count) {
        for (int i = 0; i < count; i++) {
            DrawCard();
        }
    }

    private void DrawCard() {
        if (deck.Empty) {
            Debug.Log("Can't draw from an empty deck");
            return;
        }
        Card card = deck.Draw();
        AddCardToHand(card);
    }

    private void AddCardToHand(Card card) {
        hand.Add(card);
        OnPlayerDrawCard?.Invoke(this, new OnPlayerDrawCardArgs { player = this, card = card });
    }

    public void DiscardCard(Card card) {
        Debug.Log("Discarded: " + card);
        discard.Add(card);
        hand.Remove(card);
        OnPlayerDiscardCard?.Invoke(this, new OnPlayerDiscardCardArgs { player = this, card = card });
    }

    private void ShuffleDiscardToDeck() {
        deck.Add(discard);
        deck.Shuffle();
        Debug.Log(deck.Count);
        discard.Clear();
        Debug.Log(deck.Count);

        OnShuffleDiscardToDeck?.Invoke(this, EventArgs.Empty);
    }

    private void ResetValues() {
        Movement = 0;
        Influence = 0;
        //Attack = 0;
        //IceAttack = 0;
        //FireAttack = 0;
        //ColdFireAttack = 0;
        //RangedAttack = 0;
        //IceRangedAttack = 0;
        //FireRangedAttack = 0;
        //SiegeAttack = 0;
        //IceSiegeAttack = 0;
        //FireSiegedAttack = 0;
        //Block = 0;
        //IceBlock = 0;
        //FireBlock = 0;
    }

    /* ------------------- EVENTS ---------------------- */

    private void Combat_OnCombatEnd(object sender, Combat.OnCombatEndArgs e) {
        GainFame(e.result.TotalFame);
    }

    private void MouseInput_OnHexClick(object sender, MouseInput.OnHexClickArgs e) {
        TryMove(e.hex);
    }

    private void ButtonInput_OnEndMovementClick(object sender, EventArgs e) {
        EndMovement();
    }

    private void ButtonInput_OnDrawStartHandClick(object sender, EventArgs e) {
        DrawToHandLimit();
    }

    private void ButtonInput_OnCardActionClick(object sender, ButtonInput.OnCardActionClickArgs e) {
        HandleCard(e.card, e.choice);
    }

    private void ButtonInput_OnShuffleDiscardClick(object sender, EventArgs e) {
        ShuffleDiscardToDeck();
    }

    private void ButtonInput_OnDrawCardClick(object sender, EventArgs e) {
        DrawCard();
    }
}
