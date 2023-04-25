
public class JackOfAllTrades : ActionCard {
    public JackOfAllTrades(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetPlayer().AddMovement(2);
                break;
            case 1:
                GetPlayer().AddInfluence(2);
                break;
            case 2:
                GetCombat(GetPlayer()).PlayAttackCard(2, CombatTypes.Normal, CombatElements.Physical);
                break;
            case 3:
                GetCombat(GetPlayer()).PlayBlockCard(2, CombatElements.Physical);
                break;
            case 4:
                GetPlayer().AddMovement(4);
                break;
            case 5:
                GetPlayer().AddInfluence(4);
                break;
            case 6:
                GetCombat(GetPlayer()).PlayAttackCard(4, CombatTypes.Normal, CombatElements.Physical);
                break;
            case 7:
                GetCombat(GetPlayer()).PlayBlockCard(4, CombatElements.Physical);
                break;
        }
    }
}