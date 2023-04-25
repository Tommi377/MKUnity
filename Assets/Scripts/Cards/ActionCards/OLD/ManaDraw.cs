using System.Collections.Generic;
using UnityEngine;

//public class ManaDraw : ActionCard, ITargetingCard<ManaSource>, IChoiceEffect {
//    public ManaDraw(ActionCardSO actionCardSO) : base(actionCardSO) { }

//    private ManaSource suppliedManaSource;

//    public TargetTypes TargetType => TargetTypes.ManaSource;

//    public override void Apply(CardChoice choice) {
//        switch (choice.Id) {
//            case 0:
//                GetPlayer().AddMana(1);
//                break;
//        }
//    }

//    public bool HasChoice(CardChoice choice) => choice.Id == 1;

//    public void ApplyEffect(CardChoice choice, int id) {
//        Mana.Types manaType = (Mana.Types)id; // Only works because the choices are in order
//        suppliedManaSource.SetManaType(manaType);
//        GameManager.Instance.CurrentPlayer.GetInventory().AddToken(manaType);
//        GameManager.Instance.CurrentPlayer.GetInventory().AddToken(manaType);
//    }

//    public string GetEffectChoicePrompt(CardChoice choice) => "Choose a new mana color to draw 2 mana from";

//    private readonly List<string> effectChoices = new List<string> { "Red", "Green", "Blue", "White", "Gold", "Black" };
//    public List<string> EffectChoices(CardChoice choice) => effectChoices;

//    public bool HasTarget(CardChoice choice) => choice.Id == 1;

//    public void PreTargetSideEffect(CardChoice choice) { }

//    public void TargetSideEffect(CardChoice choice, ManaSource target) => suppliedManaSource = target;

//    public bool ValidTarget(CardChoice choice, ManaSource target) => true;
//}
