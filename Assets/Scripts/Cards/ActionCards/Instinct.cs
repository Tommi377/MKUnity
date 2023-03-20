
public class Instinct : ActionCard {
    public Instinct(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        base.Apply(choice);

        Player player = GameManager.Instance.CurrentPlayer;
        switch (choice.Id) {
            case 0:
                player.AddMovement(2);
                break;
            case 1:
                player.AddInfluence(2);
                break;
            case 2:
                GetCombat(player).PlayCombatCard(new CombatData(2, CombatTypes.Normal, CombatElements.Physical));
                break;
            case 3:
                GetCombat(player).PlayCombatCard(new CombatData(2, CombatTypes.Block, CombatElements.Physical));
                break;
            case 4:
                player.AddMovement(4);
                break;
            case 5:
                player.AddInfluence(4);
                break;
            case 6:
                GetCombat(player).PlayCombatCard(new CombatData(4, CombatTypes.Normal, CombatElements.Physical));
                break;
            case 7:
                GetCombat(player).PlayCombatCard(new CombatData(4, CombatTypes.Block, CombatElements.Physical));
                break;
        }
    }
}