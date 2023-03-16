using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public  class Rage : ActionCard, ICombatCard {
    public Rage(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public List<ActionChoice> ChoicesCombat() {
        return new List<ActionChoice>() {
            new ActionChoice("Attack 2 (N)", "Attack 2 (N)", false, 0, ActionTypes.Combat),
            new ActionChoice("Block 2 (N)", "Block 2 (N)", false, 1, ActionTypes.Combat),
            new ActionChoice("Attack 4 (S)", "Attack 4 (S)", true, 2, ActionTypes.Combat)
        };
    }

    public override void Apply(ActionChoice choice) {
        base.Apply(choice);
        Player player = GameManager.Instance.CurrentPlayer;
        switch (choice.Id) {
            case 0:
                GetCombat(player).PlayCombatCard(new CombatData(2, CombatTypes.Normal, CombatElements.Normal));
                break;
            case 1:
                GetCombat(player).PlayCombatCard(new CombatData(2, CombatTypes.Block, CombatElements.Normal));
                break;
            case 2:
                GetCombat(player).PlayCombatCard(new CombatData(4, CombatTypes.Normal, CombatElements.Normal));
                break;
        }
    }
}