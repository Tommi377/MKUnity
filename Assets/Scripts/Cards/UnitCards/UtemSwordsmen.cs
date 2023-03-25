
public class UtemSwordsmen : UnitCard {
    public UtemSwordsmen(UnitCardSO UnitCardSO) : base(UnitCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetCombat(GetPlayer()).PlayAttackCard(3, CombatTypes.Normal, CombatElements.Physical);
                break;
            case 1:
                GetCombat(GetPlayer()).PlayBlockCard(3, CombatElements.Physical);
                break;
            case 2:
                GetCombat(GetPlayer()).PlayAttackCard(6, CombatTypes.Normal, CombatElements.Physical, (combatAttack) => {
                    WoundUnit();
                    return 0;
                });
                break;
            case 3:
                GetCombat(GetPlayer()).PlayBlockCard(6, CombatElements.Physical, (combatBlock) => {
                    WoundUnit();
                    return 0;
                });
                break;
        }
    }
}
