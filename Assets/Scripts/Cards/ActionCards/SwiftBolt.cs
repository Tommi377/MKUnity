
public class SwiftBolt : ActionCard {
    public SwiftBolt(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetPlayer().GetInventory().AddCrystal(Mana.Types.White);
                break;
            case 1:
                GetCombat(GetPlayer()).PlayAttackCard(4, CombatTypes.Range, CombatElements.Physical);
                break;
        }
    }
}
