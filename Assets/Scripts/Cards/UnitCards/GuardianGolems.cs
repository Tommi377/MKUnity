
public class GuardianGolems : ItemCard {
    public GuardianGolems(ItemCardSO UnitCardSO) : base(UnitCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetCombat(GetPlayer()).PlayAttackCard(2, CombatTypes.Normal, CombatElements.Physical);
                break;
            case 1:
                GetCombat(GetPlayer()).PlayBlockCard(2, CombatElements.Physical);
                break;
            case 2:
                GetCombat(GetPlayer()).PlayBlockCard(4, CombatElements.Fire);
                break;
            case 3:
                GetCombat(GetPlayer()).PlayBlockCard(4, CombatElements.Ice);
                break;
        }
    }
}
