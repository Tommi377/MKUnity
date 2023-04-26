
public class SurpriseAttack : ActionCard {
    public SurpriseAttack(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetPlayer().AddMovement(2);
                break;
            case 1:
                GetCombat(GetPlayer()).PlayAttackCard(1, CombatElements.Physical, true);
                break;
            case 2:
                GetCombat(GetPlayer()).PlayAttackCard(3, CombatElements.Physical, true);
                break;
        }
    }
}