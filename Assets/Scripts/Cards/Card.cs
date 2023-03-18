using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[Serializable]
public class CardChoice {
    public string Name;
    public string Description;
    public bool Super;
    public int Id;
    public ActionTypes ActionType;

    public CardChoice(string name, string description, bool super, int id, ActionTypes actionTypes) {
        Name = name;
        Description = description;
        Super = super;
        Id = id;
        ActionType = actionTypes;
    }

    public override string ToString() => $"Choice: {Name} ({Id}, {ActionType})";
}

public abstract class Card {
    public CardSO CardSO { get; private set; }

    public enum Types {
        Action,
        Wound,
        Spell,
        Artifact
    }

    public Card(CardSO cardSO) {
        CardSO = cardSO;
    }

    public abstract string Name { get; }
    public abstract Types Type { get; }
    public abstract bool CanApply(ActionTypes action, CardChoice actionChoice);

    public virtual bool CanPlay(ActionTypes action) => HasPlayableChoices(action);

    public override string ToString() => $"{Name} ({Type})";

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

                Type type = System.Type.GetType(cardSO.Name);
                return (ActionCard)Activator.CreateInstance(type, new object[] { actionCardSO });
            case Types.Wound:
                return new Wound(cardSO);

        }
        Debug.Log("Card matching SO not found");
        return null;
    }

    public virtual List<CardChoice> Choices(ActionTypes actionType) {
        List<CardChoice> choices = new List<CardChoice>() {
            new CardChoice("Influence 1 (D)", "Influence 1 (D)", false, -4, ActionTypes.Influence),
            new CardChoice("Block 1 (D)", "Block 1 (D)", false, -3, ActionTypes.Combat),
            new CardChoice("Attack 1 (D)", "Attack 1 (D)", false, -2, ActionTypes.Combat),
            new CardChoice("Move 1 (D)", "Move 1 (D)", false, -1, ActionTypes.Move),
        };

        choices.AddRange(CardSO.Choices);

        return choices.Where((choice) => CanApply(actionType, choice)).ToList();
    }

    public bool HasPlayableChoices(ActionTypes actionType) => Choices(actionType).Any();

    public virtual void Apply(CardChoice choice) {
        Player player = GameManager.Instance.CurrentPlayer;
        switch (choice.Id) {
            case -1:
                player.AddMovement(1);
                break;
            case -2:
                GetCombat(player).PlayCombatCard(new CombatData(1, CombatTypes.Normal, CombatElements.Normal));
                break;
            case -3:
                GetCombat(player).PlayCombatCard(new CombatData(1, CombatTypes.Block, CombatElements.Normal));
                break;
            case -4:
                player.AddInfluence(1);
                break;
        }
    }

    protected Combat GetCombat(Player player) {
        if (player.TryGetCombat(out Combat combat)) {
            return combat;
        } else {
            Debug.LogError("Playing combat card while not in combat!");
            return null;
        }
    }
}
