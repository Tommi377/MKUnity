using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

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
    public static event EventHandler<OnPlayerTrashCardArgs> OnPlayerTrashCard;
    public class OnPlayerTrashCardArgs : EventArgs {
        public Player player;
        public Card card;
    }

    public static void ResetStaticData() {
        OnPlayerDrawCard = null;
        OnPlayerDiscardCard = null;
        OnShuffleDiscardToDeck = null;
        OnPlayerTrashCard = null;
    }
    /* EVENT DEFINITIONS - END */

    // Actions
    public int Movement { get; private set; } = 0;
    public int Influence { get; private set; } = 0;

    private Inventory inventory;
    private Deck deck;
    private List<Card> hand = new List<Card>();
    private List<Card> discard = new List<Card>();

    // Stats
    public int Level { get; private set; } = 1;
    public int Fame { get; private set; } = 0;
    public int Reputation { get; private set; } = 0;
    public int Armor => levelToArmor[Level - 1];
    public int HandLimit => levelToHandLimit[Level - 1];

    private int[] levelThreshold = new int[] { 2, 7, 14, 23, 34, 47, 62, 79, 98, 119 };
    private int[] levelToArmor = new int[] { 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5 };
    private int[] levelToHandLimit = new int[] { 5, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7 };

    private void Awake() {
        inventory = new Inventory(this);
        deck = new Deck(startingDeckSO);
    }

    private void Start() {
        Combat.OnCombatEnd += Combat_OnCombatEnd;

        RoundManager.Instance.OnNewRound += RoundManager_OnNewRound;

        MouseInputManager.Instance.OnHexClick += MouseInput_OnHexClick;

        ButtonInputManager.Instance.OnEndMovementClick += ButtonInput_OnEndMovementClick;
        ButtonInputManager.Instance.OnDrawStartHandClick += ButtonInput_OnDrawStartHandClick;
        ButtonInputManager.Instance.OnShuffleDiscardClick += ButtonInput_OnShuffleDiscardClick;
        ButtonInputManager.Instance.OnDrawCardClick += ButtonInput_OnDrawCardClick;
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

    public void AddReputation(int reputation) {
        Reputation += reputation;
    }

    public void ReduceReputation(int reputation) {
        Reputation -= reputation;
    }

    public void AddFame(int fame) {
        Fame += fame;
    }

    public void ReduceFame(int fame) {
        Fame -= fame;
    }

    public void HealWounds(int count = 1) {
        for (int i = 0; i < count; i++) {
            Card found = hand.Find((card) => card is Wound);
            if (found != null) {
                hand.Remove(found);
                OnPlayerTrashCard?.Invoke(this, new OnPlayerTrashCardArgs { player = this, card = found });
            }
        }
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

    public void DrawCards(int count = 1) {
        for (int i = 0; i < count; i++) {
            if (deck.Empty) {
                Debug.Log("Can't draw from an empty deck");
                return;
            }
            Card card = deck.Draw();
            AddCardToHand(card);
        }
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

    private void DrawToHandLimit() {
        int drawAmount = Mathf.Max(HandLimit - hand.Count, 0);
        DrawCards(drawAmount);
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

        discard.Clear();

        OnShuffleDiscardToDeck?.Invoke(this, EventArgs.Empty);
    }

    private void RoundStartInit() {
        inventory.RemoveAllTokens();
    }

    private void ResetValues() {
        Movement = 0;
        Influence = 0;
    }

    /* ------------------- EVENTS ---------------------- */

    private void Combat_OnCombatEnd(object sender, Combat.OnCombatEndArgs e) {
        GainFame(e.result.TotalFame);
    }

    private void RoundManager_OnNewRound(object sender, EventArgs e) {
        RoundStartInit();
    }

    private void MouseInput_OnHexClick(object sender, MouseInputManager.OnHexClickArgs e) {
        TryMove(e.hex);
    }

    private void ButtonInput_OnEndMovementClick(object sender, EventArgs e) {
        EndMovement();
    }

    private void ButtonInput_OnDrawStartHandClick(object sender, EventArgs e) {
        DrawToHandLimit();
    }

    private void ButtonInput_OnShuffleDiscardClick(object sender, EventArgs e) {
        ShuffleDiscardToDeck();
    }

    private void ButtonInput_OnDrawCardClick(object sender, EventArgs e) {
        DrawCards();
    }
}
