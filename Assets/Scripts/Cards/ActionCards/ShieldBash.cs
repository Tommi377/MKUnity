using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldBash : ActionCard {
    public ShieldBash(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetCombat(GetPlayer()).PlayAttackCard(2, CombatElements.Physical);
                break;
            case 1:
                GetCombat(GetPlayer()).PlayBlockCard(2, CombatElements.Physical);
                break;
            case 2:
                GetCombat(GetPlayer()).PlayBlockCard(5, CombatElements.Physical);
                break;
        }
    }
}
