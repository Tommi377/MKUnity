
//public class Illusionists : ItemCard {
//    public Illusionists(ItemCardSO UnitCardSO) : base(UnitCardSO) { }

//    public override void Apply(CardChoice choice) {
//        switch (choice.Id) {
//            case 0:
//                GetPlayer().AddInfluence(4);
//                break;
//            case 1:
//                GetCombat(GetPlayer()).PlayBlockCard(0, CombatElements.Physical, (combatBlock) => {
//                    combatBlock.PreventEnemyAttack();
//                    return 0;
//                });
//                break;
//            case 2:
//                GetPlayer().GetInventory().AddCrystal();
//                break;
//        }
//    }
//}
