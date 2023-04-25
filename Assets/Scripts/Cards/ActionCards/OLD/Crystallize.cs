using UnityEngine;
using System.Collections.Generic;

public class Crystallize : ActionCard {
    private Mana suppliedMana;

    public Crystallize(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        switch(choice.Id) {
            case 0:
                GetPlayer().AddMana(1);
                break;
            case 1:
                GetPlayer().GetInventory().AddCrystal();
                break;
        }
    }
}
