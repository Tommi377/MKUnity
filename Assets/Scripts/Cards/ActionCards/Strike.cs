using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Strike : ActionCard {
    public Strike(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetCombat(GetPlayer()).PlayAttackCard(2, CombatElements.Physical);
                break;
            case 1:
                GetCombat(GetPlayer()).PlayBlockCard(2, CombatElements.Physical);
                break;
            case 2:
                GetCombat(GetPlayer()).PlayAttackCard(4, CombatElements.Physical);
                break;
        }
    }
}
