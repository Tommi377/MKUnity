using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Travel : ActionCard {
    public Travel(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetPlayer().AddMovement(2);
                break;
            case 1:
                GetPlayer().AddMovement(4);
                break;
        }
    }
}
