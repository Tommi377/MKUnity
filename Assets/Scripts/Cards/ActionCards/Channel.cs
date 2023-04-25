using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Channel : ActionCard {
    public Channel(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetPlayer().AddMana(1);
                break;
            case 1:
                GetPlayer().GetInventory().AddCrystal();
                break;
        }
    }
}
