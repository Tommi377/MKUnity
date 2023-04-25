
public class UtemGuardsmen : ItemCard {
    public UtemGuardsmen(ItemCardSO UnitCardSO) : base(UnitCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetCombat(GetPlayer()).PlayAttackCard(2, CombatTypes.Normal, CombatElements.Physical);
                break;
            case 1:
                GetCombat(GetPlayer()).PlayBlockCard(4, CombatElements.Physical, (combatBlock) => {
                    if (combatBlock.Enemy.Abilities.Contains(EnemyAbilities.Swift)) return 4;
                    return 0;
                });
                break;
        }
    }
}
