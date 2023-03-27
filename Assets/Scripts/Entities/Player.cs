using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BaseAction {
    public string Name;
    public string Description;
    public Action Action;
    public BaseAction(string name, string description, Action action) {
        Name = name;
        Description = description;
        Action = action;
    }
}

public struct LevelStats {
    public int Armor;
    public int HandLimit;
    public int UnitLimit;
    public LevelStats(int armor, int handLimit, int unitLimit) {
        Armor = armor;
        HandLimit = handLimit;
        UnitLimit = unitLimit;
    }
}

[RequireComponent(typeof(Deck))]
[RequireComponent(typeof(Inventory))]
public class Player : Entity {
    [SerializeField] private CardSO woundSO;

    public override EntityTypes EntityType { get { return EntityTypes.Player; } }

    /* EVENT DEFINITIONS - START */
    public static event EventHandler OnShuffleDiscardToDeck;
    public static event EventHandler<CardEventArgs> OnPlayerDrawCard;
    public static event EventHandler<CardEventArgs> OnPlayerDiscardCard;
    public static event EventHandler<CardEventArgs> OnPlayerTrashCard;
    public class CardEventArgs : EventArgs {
        public Player Player;
        public Card Card;
    }


    public static event EventHandler<IntEventArgs> OnPlayerInfluenceUpdate;
    public class IntEventArgs : EventArgs {
        public Player Player;
        public int Value;
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

    // Components
    private Inventory inventory;
    private Deck deck;

    private List<Card> hand = new List<Card>();
    private List<Card> discard = new List<Card>();
    private List<UnitCard> units = new List<UnitCard>();

    // Stats
    public int Level { get; private set; } = 1;
    public int Fame { get; private set; } = 0;
    public int Reputation { get; private set; } = 0;
    public int ReputationBonus => reputationBonuses[Math.Min(Math.Max(Reputation + 7, 0), reputationBonuses.Length - 1)];
    public int Armor => levelStats[Level / 2].Armor;
    public int HandLimit => levelStats[Level / 2].HandLimit;
    public int UnitLimit => levelStats[Level / 2].UnitLimit;

    private readonly int[] reputationBonuses = new int[] { -99, -5, -3, -2, -1, -1, 0, 0, 0, 1, 1, 2, 2, 3, 3 };
    private readonly int[] levelThresholds = new int[] { 2, 7, 14, 23, 34, 47, 62, 79, 98, 119 };

    private readonly List<LevelStats> levelStats = new List<LevelStats>() {
        new LevelStats(2, 5, 1),
        new LevelStats(3, 5, 2),
        new LevelStats(3, 6, 3),
        new LevelStats(4, 6, 4),
        new LevelStats(4, 7, 5),
        new LevelStats(5, 7, 6),
    };

    public Inventory GetInventory() => inventory;
    public Deck GetDeck() => deck;
    public List<Card> GetHand() => hand;
    public List<UnitCard> GetUnits() => units;

    // Modifier functions
    List<Func<Hex, int, int>> MoveModifiers = new List<Func<Hex, int, int>>();

    public int GetDeckCount() => deck.Count;
    public int GetDiscardCount() => discard.Count;
    public bool IsInCombat() => GameManager.Instance.Combat != null && GameManager.Instance.Combat.Player == this;

    public ReadOnlyCollection<Card> DiscardPile => discard.AsReadOnly();

    private void Awake() {
        inventory = GetComponent<Inventory>();
        deck = GetComponent<Deck>();
    }

    private void Start() {
        TurnStartInit();

        Combat.OnCombatEnd += Combat_OnCombatEnd;

        RoundManager.Instance.OnNewRound += RoundManager_OnNewRound;
        RoundManager.Instance.OnNewTurn += RoundManager_OnNewTurn;
        RoundManager.Instance.OnPhaseChange += RoundManager_OnPhaseChange;

        MouseInputManager.Instance.OnHexClick += MouseInput_OnHexClick;

        ButtonInputManager.Instance.OnShuffleDiscardClick += ButtonInputManager_OnShuffleDiscardClick;
        ButtonInputManager.Instance.OnDrawCardClick += ButtonInputManager_OnDrawCardClick;
        ButtonInputManager.Instance.OnInfluenceChoiceClick += ButtonInputManager_OnInfluenceChoiceClick;
        ButtonInputManager.Instance.OnRecruitUnitClick += ButtonInputManager_OnRecruitUnitClick;
    }

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
            Wound wound = woundSO.CreateInstance() as Wound;
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
        OnPlayerInfluenceUpdate?.Invoke(this, new IntEventArgs { Player = this, Value = Influence });
    }

    public void ReduceInfluence(int influence) {
        Influence -= influence;
        OnPlayerInfluenceUpdate?.Invoke(this, new IntEventArgs { Player = this, Value = Influence });
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
                OnPlayerTrashCard?.Invoke(this, new CardEventArgs { Player = this, Card = found });
            }
        }
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
    
    public List<BaseAction> GetStartOfTurnActions() {
        List <BaseAction> actions = new List<BaseAction>();

        if (TryGetHex(out Hex hex) && hex.ContainsStructure()) {
            actions.AddRange(hex.Structure.StartOfTurnActions(this));
        }
        return actions;
    }

    public List<BaseAction> GetEndOfTurnActions() {
        List<BaseAction> actions = new List<BaseAction>();

        if (TryGetHex(out Hex hex) && hex.ContainsStructure()) {
            actions.AddRange(hex.Structure.EndOfTurnActions(this));
        }

        return actions;
    }

    private bool TryMove(Hex hex) {
        if (HexMap.HexIsNeigbor(Position, hex.Position)) {
            int moveCost = hex.GetMoveCost();
            if (moveCost < 0) {
                Debug.Log("Inaccessable tile");
                return false;
            }

            foreach (Func<Hex, int, int> modFunc in MoveModifiers) {
                moveCost = modFunc(hex, moveCost);
            }

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

    public void EndAction() {
        ResetValues();
        if (!GetHex().IsSafeTile()) {
            // Force player to retreat
            // Then execute the end of turn
        }
    }

    public void AddModifierFunction<T>(T func) {
        if (func is Func<Hex, int, int>) {
            MoveModifiers.Add(func as Func<Hex, int, int>);
        } else {
            Debug.Log("No modifier type: " + typeof(T).Name);
        }
    }

    private void LevelUp() {
        Level += 1;
    }

    private void GainFame(int amount) {
        // TODO: account for multiple level ups at once
        Fame += amount;
        if (levelThresholds[Level - 1] < Fame) {
            LevelUp();
        }
    }

    private void DrawToHandLimit() {
        int drawAmount = Mathf.Max(HandLimit - hand.Count, 0);
        DrawCards(drawAmount);
    }

    private void AddCardToHand(Card card) {
        hand.Add(card);
        OnPlayerDrawCard?.Invoke(this, new CardEventArgs { Player = this, Card = card });
    }

    public void DiscardCard(Card card) {
        Debug.Log("Discarded: " + card);
        discard.Add(card);
        hand.Remove(card);
        OnPlayerDiscardCard?.Invoke(this, new CardEventArgs { Player = this, Card = card });
    }

    private void ShuffleDiscardToDeck() {
        deck.Add(discard);
        deck.Shuffle();

        discard.Clear();

        OnShuffleDiscardToDeck?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyInfluenceAction(InfluenceAction action) {
        int trueCost = action.Cost - ReputationBonus;
        if (Influence < trueCost) { 
            Debug.Log("Not enough influence for action");
            return;
        }
        ReduceInfluence(trueCost);
        action.Apply(this);
    }

    private void RecruitUnit(UnitCard unitCard) {
        int trueCost = unitCard.Influence - ReputationBonus;
        if (Influence < trueCost) {
            Debug.Log("Not enough influence for recruit");
            return;
        }
        ReduceInfluence(trueCost);
        // TODO: make replace unit if at cap
        units.Add(unitCard);
        UnitManager.Instance.RecruitUnit(unitCard);
    }

    private void RoundStartInit() {
        ShuffleDiscardToDeck();
        foreach (UnitCard unitCard in units) {
            unitCard.Ready();
        }
    }

    private void TurnStartInit() {
        inventory.RemoveAllTokens();
        DrawToHandLimit();
    }

    private void ResetValues() {
        Movement = 0;
        Influence = 0;

        MoveModifiers.Clear();
    }

    /* ------------------- EVENTS ---------------------- */

    private void Combat_OnCombatEnd(object sender, Combat.OnCombatEndArgs e) {
        GainFame(e.result.TotalFame);
    }

    private void RoundManager_OnPhaseChange(object sender, RoundManager.OnPhaseChangeArgs e) {
        ResetValues();
    }

    private void RoundManager_OnNewRound(object sender, EventArgs e) {
        RoundStartInit();
    }

    private void RoundManager_OnNewTurn(object sender, EventArgs e) {
        TurnStartInit();
    }

    private void MouseInput_OnHexClick(object sender, MouseInputManager.OnHexClickArgs e) {
        TryMove(e.hex);
    }

    private void ButtonInputManager_OnShuffleDiscardClick(object sender, EventArgs e) {
        ShuffleDiscardToDeck();
    }

    private void ButtonInputManager_OnDrawCardClick(object sender, EventArgs e) {
        DrawCards();
    }

    private void ButtonInputManager_OnInfluenceChoiceClick(object sender, ButtonInputManager.OnInfluenceChoiceClickArgs e) {
        ApplyInfluenceAction(e.influenceAction);
    }

    private void ButtonInputManager_OnRecruitUnitClick(object sender, ButtonInputManager.OnRecruitUnitClickArgs e) {
        RecruitUnit(e.unitCard);
    }
}
