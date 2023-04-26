using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwissKnife : ItemCard {
    public SwissKnife(ItemCardSO UnitCardSO) : base(UnitCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetPlayer().AddMovement(1);
                break;
            case 1:
                GetCombat(GetPlayer()).PlayAttackCard(1, CombatElements.Physical);
                break;
            case 2:
                GetCombat(GetPlayer()).PlayBlockCard(1, CombatElements.Physical);
                break;
            case 3:
                GetPlayer().AddInfluence(1);
                break;
        }
    }
}
