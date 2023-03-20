
public class Promise : ActionCard {
    public Promise(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        base.Apply(choice);

        Player player = GameManager.Instance.CurrentPlayer;
        switch (choice.Id) {
            case 0:
                player.AddInfluence(2);
                break;
            case 1:
                player.AddInfluence(4);
                break;
        }
    }
}