using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class March : ActionCard {
    public March(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
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