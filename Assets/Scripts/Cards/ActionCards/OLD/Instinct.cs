
public class Instinct : ActionCard {
    public Instinct(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetPlayer().AddMovement(2);
                break;
            case 1:
                GetPlayer().AddInfluence(2);
                break;
            case 2:
                GetCombat(GetPlayer()).PlayCombatCard(new CombatData(2, CombatTypes.Normal, CombatElements.Physical));
                break;
            case 3:
                GetCombat(GetPlayer()).PlayCombatCard(new CombatData(2, CombatTypes.Block, CombatElements.Physical));
                break;
            case 4:
                GetPlayer().AddMovement(4);
                break;
            case 5:
                GetPlayer().AddInfluence(4);
                break;
            case 6:
                GetCombat(GetPlayer()).PlayCombatCard(new CombatData(4, CombatTypes.Normal, CombatElements.Physical));
                break;
            case 7:
                GetCombat(GetPlayer()).PlayCombatCard(new CombatData(4, CombatTypes.Block, CombatElements.Physical));
                break;
        }
    }
}