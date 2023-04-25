
public class Swiftness : ActionCard {
    public Swiftness(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetPlayer().AddMovement(2);
                break;
            case 1:
                Combat combat = GetCombat(GetPlayer());
                combat.PlayCombatCard(new CombatData(3, CombatTypes.Range, CombatElements.Physical));
                break;
        }
    }
}