
public class Tranquility : ActionCard {
    public Tranquility(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetPlayer().HealWounds(1);
                break;
            case 1:
                GetPlayer().DrawCards(1);
                break;
            case 2:
                GetPlayer().HealWounds(2);
                break;
            case 3:
                GetPlayer().DrawCards(2);
                break;
        }
    }
}