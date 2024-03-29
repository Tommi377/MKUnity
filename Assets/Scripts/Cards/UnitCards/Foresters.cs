using System;
using System.Collections.Generic;

public class Foresters : ItemCard {
    public Foresters(ItemCardSO UnitCardSO) : base(UnitCardSO) { }

    private List<HexTypes> discounted = new List<HexTypes> { HexTypes.Forest, HexTypes.Hills, HexTypes.Swamp };

    public override void Apply(CardChoice choice) {
        switch(choice.Id) {
            case 0:
                GetPlayer().AddMovement(2);
                GetPlayer().AddModifierFunction<Func<Hex, int, int>>((Hex hex, int cost) => discounted.Contains(hex.HexType) ? cost - 1 : cost);
                break;
            case 1:
                GetCombat(GetPlayer()).PlayBlockCard(3, CombatElements.Physical);
                break;
        }
    }
}
