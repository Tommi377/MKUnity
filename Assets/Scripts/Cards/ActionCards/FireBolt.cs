
public class FireBolt : ActionCard {
    public FireBolt(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetPlayer().GetInventory().AddCrystal(Mana.Types.Red);
                break;
            case 1:
                GetCombat(GetPlayer()).PlayAttackCard(3, CombatTypes.Range, CombatElements.Fire);
                break;
        }
    }
}
