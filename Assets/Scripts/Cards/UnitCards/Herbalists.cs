
public class Herbalists : ItemCard, ITargetingCard<ItemCard> {
    private ItemCard cachedTarget;

    public Herbalists(ItemCardSO UnitCardSO) : base(UnitCardSO) { }

    public TargetTypes TargetType => TargetTypes.UnitCard;

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetPlayer().HealWounds(2);
                break;
            case 1:
                cachedTarget.Ready();
                break;
            case 2:
                GetPlayer().AddMana(1);
                break;
        }
    }

    public bool HasTarget(CardChoice choice) => choice.Id == 1;
    public bool ValidTarget(CardChoice choice, ItemCard target) => target.Exhausted && target.Level < 3;
    public void PreTargetSideEffect(CardChoice choice) => cachedTarget = null;
    public void TargetSideEffect(CardChoice choice, ItemCard target) => cachedTarget = target;
}
