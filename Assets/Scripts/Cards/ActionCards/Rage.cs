using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public  class Rage : ActionCard {
    public Rage(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        base.Apply(choice);
        Player player = GameManager.Instance.CurrentPlayer;
        switch (choice.Id) {
            case 0:
                GetCombat(player).PlayCombatCard(new CombatData(2, CombatTypes.Normal, CombatElements.Physical));
                break;
            case 1:
                GetCombat(player).PlayCombatCard(new CombatData(2, CombatTypes.Block, CombatElements.Physical));
                break;
            case 2:
                GetCombat(player).PlayCombatCard(new CombatData(4, CombatTypes.Normal, CombatElements.Physical));
                break;
        }
    }
}