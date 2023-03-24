using System;
using System.Collections.Generic;
using UnityEngine;

public class Foresters : UnitCard {
    public Foresters(UnitCardSO UnitCardSO) : base(UnitCardSO) { }

    private List<HexTypes> discounted = new List<HexTypes> { HexTypes.Forest, HexTypes.Hills, HexTypes.Swamp };

    public override void Apply(CardChoice choice) {
        base.Apply(choice);

        switch(choice.Id) {
            case 0:
                GetPlayer().AddMovement(2);
                GetPlayer().AddModifierFunction<Func<Hex, int, int>>((Hex hex, int cost) => discounted.Contains(hex.HexType) ? cost - 1 : cost);
                break;
            case 1:
                GetCombat(GetPlayer()).PlayCombatCard(new CombatData(3, CombatTypes.Block, CombatElements.Physical));
                break;
        }
    }
}
