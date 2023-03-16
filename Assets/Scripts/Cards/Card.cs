using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class ActionChoice {
    public string Name;
    public string Description;
    public bool Super;
    public int Id;
    public ActionTypes ActionType;

    public ActionChoice(string name, string description, bool super, int id, ActionTypes actionTypes) {
        Name = name;
        Description = description;
        Super = super;
        Id = id;
        ActionType = actionTypes;
    }
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
    public abstract bool CanPlay(ActionTypes action);
    public abstract bool CanApply(ActionTypes action, ActionChoice actionChoice);
    public abstract List<ActionChoice> Choices(ActionTypes actionType);

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


    public virtual List<ActionChoice> ChoicesDefault(ActionTypes type) {
        List<ActionChoice> choices = new List<ActionChoice>();

        switch(type) {
            case ActionTypes.Move:
                choices.Add(new ActionChoice("Move 1 (D)", "Move 1 (D)", false, -1, ActionTypes.Move));
                break;
            case ActionTypes.Combat:
                choices.Add(new ActionChoice("Attack 1 (D)", "Attack 1 (D)", false, -2, ActionTypes.Combat));
                choices.Add(new ActionChoice("Block 1 (D)", "Block 1 (D)", false, -3, ActionTypes.Combat));
                break;
        };

        if (this is ISpecialCard) {
            choices.AddRange((this as ISpecialCard).ChoicesSpecial());
        }

        if (this is IHealCard && !GameManager.Instance.CurrentPlayer.IsInCombat()) {
            choices.AddRange((this as ISpecialCard).ChoicesSpecial());
        }

        return choices;
    }

    public virtual void Apply(ActionChoice choice) {
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
