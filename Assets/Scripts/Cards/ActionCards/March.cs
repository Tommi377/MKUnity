using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class March : ActionCard, IMovementCard {
    public March(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public List<ActionChoice> ChoicesMove() {
        return new List<ActionChoice>() {
            new ActionChoice("Move 2 (N)", "Move 2 (N)", false, 0, ActionTypes.Move),
            new ActionChoice("Move 4 (S)", "Move 4 (S)", true, 1, ActionTypes.Move)
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
                player.AddMovement(4);
                break;
        }
    }
}