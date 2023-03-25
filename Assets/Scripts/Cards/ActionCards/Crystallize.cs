using UnityEngine;
using System.Collections.Generic;

public class Crystallize : ActionCard, ITargetingCard<Mana>, IChoiceEffect {
    private Mana suppliedMana;

    public Crystallize(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public TargetTypes TargetType => TargetTypes.Mana;
    public bool HasTarget(CardChoice choice) => choice.Id == 0;
    public bool HasChoice(CardChoice choice) => choice.Id == 1;

    public bool ValidTarget(CardChoice choice, Mana target) => target.Type != Mana.Types.Gold && target.Type != Mana.Types.Black;

    public void PreTargetSideEffect(CardChoice choice) {}

    public void TargetSideEffect(CardChoice choice, Mana target) {
        suppliedMana = target;
    }

    public override void Apply(CardChoice choice) {
        switch(choice.Id) {
            case 0:
                GetPlayer().GetInventory().AddCrystal(suppliedMana.Type);
                GetPlayer().GetInventory().RemoveMana(suppliedMana);
                break;
        }
    }

    public string GetEffectChoicePrompt(CardChoice choice) => "Choose a crystal to gain";

    private readonly List<string> effectChoices = new List<string> { "Red", "Green", "Blue", "White", "Gold", "Black" };
    public List<string> EffectChoices(CardChoice choice) => effectChoices;

    public void ApplyEffect(int id) {
        Inventory inventory = GetPlayer().GetInventory();
        Mana.Types manaType = (Mana.Types)id; // Only works because the choices are in order
        inventory.AddCrystal(manaType);
    }
}
