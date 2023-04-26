
public class RedCapeMonks : ItemCard {
    public RedCapeMonks(ItemCardSO UnitCardSO) : base(UnitCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetCombat(GetPlayer()).PlayAttackCard(3, CombatElements.Physical);
                break;
            case 1:
                GetCombat(GetPlayer()).PlayBlockCard(3, CombatElements.Physical);
                break;
            case 2:
                GetCombat(GetPlayer()).PlayAttackCard(4, CombatElements.Fire);
                break;
            case 3:
                GetCombat(GetPlayer()).PlayBlockCard(4, CombatElements.Fire);
                break;
        }
    }
}
