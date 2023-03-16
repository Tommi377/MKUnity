using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class Swiftness : ActionCard, IMovementCard, ICombatCard {
    public Swiftness(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public List<ActionChoice> ChoicesMove() {
        return new List<ActionChoice>() {
            new ActionChoice("Move 2 (N)", "Move 2 (N)", false, 0, ActionTypes.Move)
        };
    }

    public List<ActionChoice> ChoicesCombat() {
        return new List<ActionChoice>() {
            new ActionChoice("Ranged Attack 3 (S)", "Ranged Attack 3 (S)", true, 1, ActionTypes.Combat)
        };
    }

    public override void Apply(ActionChoice choice) {
        base.Apply(choice);
        Player player = GameManager.Instance.CurrentPlayer;
        switch (choice.Id) {
            case 0:
                player.AddMovement(2);
                break;
            case 1:
                Combat combat = GetCombat(player);
                combat.PlayCombatCard(new CombatData(3, CombatTypes.Range, CombatElements.Normal));
                break;
        }
    }
}