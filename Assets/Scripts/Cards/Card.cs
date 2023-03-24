using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


[Serializable]
public class CardChoice {
    public string Name;
    public string Description;
    public int Id;
    public ActionTypes ActionType;
    public List<Mana.Types> ManaTypes;

    public CardChoice(string name, string description, int id, ActionTypes actionTypes, List<Mana.Types> manaTypes = null) {
        Name = name;
        Description = description;
        Id = id;
        ActionType = actionTypes;
        ManaTypes = manaTypes ?? new List<Mana.Types>();
    }

    public override string ToString() => $"Choice: {Name} ({Id}, {ActionType})";
}

public abstract class Card {
    public CardSO CardSO { get; private set; }

    public enum Types {
        Action,
        Wound,
        Spell,
        Artifact,
        Unit
    }

    public Card(CardSO cardSO) {
        CardSO = cardSO;
    }

    public abstract string Name { get; }
    public abstract Types Type { get; }

    public virtual bool CanApply(ActionTypes action, CardChoice cardChoice) {
        if (cardChoice.ActionType == ActionTypes.Special) return true; // Special cards can be played in any time
        if (cardChoice.ActionType == ActionTypes.Heal && !GameManager.Instance.CurrentPlayer.IsInCombat()) return true; // Heal cards can only be played out of combat
        if (action == cardChoice.ActionType) return true; // Actions that match the round action can be played

        return false;
    }

    public virtual bool CanPlay(ActionTypes action) => HasPlayableChoices(action);

    public virtual List<CardChoice> Choices(ActionTypes actionType) => CardSO.Choices.Where((choice) => CanApply(actionType, choice)).ToList();
    public abstract void Apply(CardChoice choice);

    public override string ToString() => $"{Name} ({Type})";

    public bool HasPlayableChoices(ActionTypes actionType) => Choices(actionType).Any();

    protected Combat GetCombat(Player player) {
        if (player.TryGetCombat(out Combat combat)) {
            return combat;
        } else {
            Debug.LogError("Accessing combat while no combat in progress!");
            return null;
        }
    }

    protected Player GetPlayer() => GameManager.Instance.CurrentPlayer;

    public static List<Card> GetCardsFromSO(IEnumerable<CardSO> cardSOs) {
        List<Card> cards = new List<Card>();
        foreach (CardSO cardSO in cardSOs) {
            Card card = GetCardFromSO(cardSO);
            if (card != null) cards.Add(card);
        }
        return cards;
    }

    public static Card GetCardFromSO(CardSO cardSO) {
        switch (cardSO.Type) {
            case Types.Action:
                ActionCardSO actionCardSO = cardSO as ActionCardSO;

                Type type = System.Type.GetType(cardSO.Name.Replace(" ", ""));
                return (ActionCard)Activator.CreateInstance(type, new object[] { actionCardSO });
            case Types.Wound:
                return new Wound(cardSO);

        }
        Debug.Log("Card matching SO not found");
        return null;
    }
}
