
public class NorthernMonks : UnitCard {
    public NorthernMonks(UnitCardSO UnitCardSO) : base(UnitCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetCombat(GetPlayer()).PlayAttackCard(3, CombatTypes.Normal, CombatElements.Physical);
                break;
            case 1:
                GetCombat(GetPlayer()).PlayBlockCard(3, CombatElements.Physical);
                break;
            case 2:
                GetCombat(GetPlayer()).PlayAttackCard(4, CombatTypes.Normal, CombatElements.Ice);
                break;
            case 3:
                GetCombat(GetPlayer()).PlayBlockCard(4, CombatElements.Ice);
                break;
        }
    }
}
