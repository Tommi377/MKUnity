using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoodShoes : ItemCard {
    public GoodShoes(ItemCardSO UnitCardSO) : base(UnitCardSO) { }

    private List<HexTypes> discounted = new List<HexTypes> { HexTypes.Forest, HexTypes.Hills, HexTypes.Swamp };

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetPlayer().AddMovement(2);
                GetPlayer().AddModifierFunction<Func<Hex, int, int>>((Hex hex, int cost) => hex.HexType != HexTypes.Plains ? cost - 1 : cost);
                break;
            case 1:
                GetCombat(GetPlayer()).PlayBlockCard(3, CombatElements.Physical);
                break;
        }
    }
}
