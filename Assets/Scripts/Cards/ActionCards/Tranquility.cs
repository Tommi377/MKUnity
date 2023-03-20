
public class Tranquility : ActionCard {
    public Tranquility(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        base.Apply(choice);

        Player player = GameManager.Instance.CurrentPlayer;
        switch (choice.Id) {
            case 0:
                player.HealWounds(1);
                break;
            case 1:
                player.DrawCards(1);
                break;
            case 2:
                player.HealWounds(2);
                break;
            case 3:
                player.DrawCards(2);
                break;
        }
    }
}