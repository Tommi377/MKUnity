
public class UtemCrossbowmen : ItemCard {
    public UtemCrossbowmen(ItemCardSO UnitCardSO) : base(UnitCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetCombat(GetPlayer()).PlayAttackCard(3, CombatElements.Physical);
                break;
            case 1:
                GetCombat(GetPlayer()).PlayBlockCard(3, CombatElements.Physical);
                break;
            case 2:
                GetCombat(GetPlayer()).PlayAttackCard(2, CombatElements.Physical);
                break;
        }
    }
}
