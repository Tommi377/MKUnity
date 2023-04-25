using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainmailArmor : ItemCard {
    public ChainmailArmor(ItemCardSO UnitCardSO) : base(UnitCardSO) { }
    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetCombat(GetPlayer()).PlayBlockCard(1, CombatElements.Physical);
                break;
        }
    }
}
