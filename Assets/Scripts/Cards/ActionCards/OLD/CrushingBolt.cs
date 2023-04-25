
public class CrushingBolt : ActionCard {
    public CrushingBolt(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetPlayer().GetInventory().AddCrystal();
                break;
            case 1:
                GetCombat(GetPlayer()).PlayAttackCard(3, CombatTypes.Siege, CombatElements.Physical);
                break;
        }
    }
}
