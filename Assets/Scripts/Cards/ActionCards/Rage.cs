
public  class Rage : ActionCard {
    public Rage(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetCombat(GetPlayer()).PlayCombatCard(new CombatData(2, CombatTypes.Normal, CombatElements.Physical));
                break;
            case 1:
                GetCombat(GetPlayer()).PlayCombatCard(new CombatData(2, CombatTypes.Block, CombatElements.Physical));
                break;
            case 2:
                GetCombat(GetPlayer()).PlayCombatCard(new CombatData(4, CombatTypes.Normal, CombatElements.Physical));
                break;
        }
    }
}