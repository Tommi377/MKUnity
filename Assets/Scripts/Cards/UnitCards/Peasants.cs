using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Peasants : UnitCard {
    public Peasants(UnitCardSO UnitCardSO) : base(UnitCardSO) { }

    public override void Apply(CardChoice choice) {
        switch(choice.Id) {
            case 0:
                GetCombat(GetPlayer()).PlayCombatCard(new CombatData(2, CombatTypes.Normal, CombatElements.Physical));
                break;
            case 1:
                GetCombat(GetPlayer()).PlayCombatCard(new CombatData(2, CombatTypes.Block, CombatElements.Physical));
                break;
            case 2:
                GetPlayer().AddInfluence(2);
                break;
            case 3:
                GetPlayer().AddMovement(2);
                break;
        }
    }
}
