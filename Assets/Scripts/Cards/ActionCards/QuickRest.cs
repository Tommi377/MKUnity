
public class QuickRest : ActionCard {
    public QuickRest(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetPlayer().AddHeal(1);
                break;
            case 1:
                GetPlayer().DrawCards(2);
                break;
            case 2:
                GetPlayer().AddHeal(2);
                break;
            case 3:
                GetPlayer().DrawCards(3);
                break;
        }
    }
}