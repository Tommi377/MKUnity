using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ActionTypeRestriction {
    Normal,
    Super,
    Both
}

public abstract class ActionCard : Card {
    public override string Name => ActionCardSO.Name;
    public override Types Type => Types.Action;
    public string DescUp => ActionCardSO.DescUp;
    public string DescDown => ActionCardSO.DescDown;
    public List<Mana.Types> ManaTypes => ActionCardSO.ManaTypes;

    private ActionCardSO ActionCardSO => CardSO as ActionCardSO;

    public ActionCard(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override List<CardChoice> Choices() {
        List<CardChoice> choices = new List<CardChoice>() {
            new CardChoice("Influence 1 (D)", "Influence 1 (D)", -4, ActionTypes.Influence),
            new CardChoice("Block 1 (D)", "Block 1 (D)", -3, ActionTypes.Combat),
            new CardChoice("Attack 1 (D)", "Attack 1 (D)", -2, ActionTypes.Combat),
            new CardChoice("Move 1 (D)", "Move 1 (D)", -1, ActionTypes.Move),
        };

        choices.AddRange(CardSO.Choices);

        return choices;
    }

    public override void ApplyChoice(CardChoice choice) {
        Player player = GameManager.Instance.CurrentPlayer;
        if (choice.Id < 0) {
            switch (choice.Id) {
                case -1:
                    player.AddMovement(1);
                    break;
                case -2:
                    GetCombat(player).PlayCombatCard(new CombatData(1, CombatTypes.Normal, CombatElements.Physical));
                    break;
                case -3:
                    GetCombat(player).PlayCombatCard(new CombatData(1, CombatTypes.Block, CombatElements.Physical));
                    break;
                case -4:
                    player.AddInfluence(1);
                    break;
            }
        } else {
            Apply(choice);
        }
    }
}