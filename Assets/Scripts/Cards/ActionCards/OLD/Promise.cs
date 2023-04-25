
public class Promise : ActionCard {
    public Promise(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetPlayer().AddInfluence(2);
                break;
            case 1:
                GetPlayer().AddInfluence(4);
                break;
        }
    }
}