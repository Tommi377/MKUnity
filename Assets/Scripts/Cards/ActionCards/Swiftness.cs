using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class Swiftness : ActionCard {
    public Swiftness(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        base.Apply(choice);
        Player player = GameManager.Instance.CurrentPlayer;
        switch (choice.Id) {
            case 0:
                player.AddMovement(2);
                break;
            case 1:
                Combat combat = GetCombat(player);
                combat.PlayCombatCard(new CombatData(3, CombatTypes.Range, CombatElements.Normal));
                break;
        }
    }
}