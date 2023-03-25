using UnityEngine;
using System.Linq;

public  class ColdToughness : ActionCard {
    public ColdToughness(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetCombat(GetPlayer()).PlayCombatCard(new CombatData(2, CombatTypes.Normal, CombatElements.Physical));
                break;
            case 1:
                GetCombat(GetPlayer()).PlayCombatCard(new CombatData(3, CombatTypes.Block, CombatElements.Ice));
                break;
            case 2:
                GetCombat(GetPlayer()).PlayCombatCard(new CombatData(4, CombatTypes.Block, CombatElements.Physical, (enemies) => {
                    Enemy enemy = enemies[0];
                    int bonusBlock = enemy.Abilities.Count();
                    foreach (EnemyAttack attack in enemy.Attacks) {
                        if (attack.Element == CombatElements.Ice || attack.Element == CombatElements.Fire) bonusBlock += 1;
                        if (attack.Element == CombatElements.ColdFire) bonusBlock += 2;
                    }
                    Debug.Log("Bonus " + bonusBlock);
                    return bonusBlock;

                }));
                break;
        }
    }
}