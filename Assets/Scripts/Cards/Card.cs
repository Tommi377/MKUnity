using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[Serializable]
public class CardChoice {
    public string Name;
    public int Id;
    public ActionTypes ActionType;
    public bool Super;

    public CardChoice(string name, int id, ActionTypes actionTypes, bool super = false) {
        Name = name;
        Id = id;
        ActionType = actionTypes;
        Super = super;
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
        Item
    }

    public Card(CardSO cardSO) {
        CardSO = cardSO;

        if (cardSO == null) {
            Debug.LogError("CardSO can't be null");
            return;
        }

        if (cardSO.Name.Replace(" ", "") != GetType().Name) {
            Debug.LogError("CardSO name field does not match the class name");
            return;
        }
    }

    public abstract string Name { get; }
    public abstract Types Type { get; }

    public virtual bool CanApply(ActionTypes action, CardChoice cardChoice) {

        if (cardChoice.ActionType == ActionTypes.Special) return true; // Special cards can be played in any time
        if (cardChoice.ActionType == ActionTypes.Heal && !GameManager.Instance.CurrentPlayer.IsInCombat()) return true; // Heal cards can only be played out of combat
        if (GameManager.Instance.Combat != null && GameManager.Instance.Combat.CanApply(cardChoice)) return true; // If combat overrides rules then can played
        if (action == cardChoice.ActionType) return true; // Actions that match the round action can be played

        return false;
    }

    public virtual bool CanPlay(ActionTypes action) => HasPlayableChoices(action);

    public virtual List<CardChoice> GetChoices(ActionTypes actionType) => Choices().Where((choice) => CanApply(actionType, choice)).ToList();

    public virtual List<CardChoice> Choices() => CardSO.Choices;

    public virtual void ApplyChoice(CardChoice choice) => Apply(choice);

    public abstract void Apply(CardChoice choice);

    public override string ToString() => $"{Name} ({Type})";

    public bool HasPlayableChoices(ActionTypes actionType) => GetChoices(actionType).Any();

    protected Combat GetCombat(Player player) {
        if (player.TryGetCombat(out Combat combat)) {
            return combat;
        } else {
            Debug.LogError("Accessing combat while no combat in progress!");
            return null;
        }
    }

    protected Player GetPlayer() => GameManager.Instance.CurrentPlayer;
}
