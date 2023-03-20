using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
        base.Apply(choice);
        Player player = GameManager.Instance.CurrentPlayer;

        switch(choice.Id) {
            case 0:
                player.GetInventory().AddCrystal(suppliedMana.Type);
                player.GetInventory().RemoveMana(suppliedMana);
                break;
        }
    }

    public string GetEffectChoicePrompt(CardChoice choice) => "Choose a crystal to gain";

    List<string> IChoiceEffect.EffectChoices(CardChoice choice) => new List<string> { "Red", "Green", "Blue", "White", "Gold", "Black" };

    public void ApplyEffect(int id) {
        Debug.Log("First");
        Inventory inventory = GameManager.Instance.CurrentPlayer.GetInventory();
        switch (id) {
            case 0:
                inventory.AddCrystal(Mana.Types.Red);
                break;
            case 1:
                inventory.AddCrystal(Mana.Types.Green);
                break;
            case 2:
                inventory.AddCrystal(Mana.Types.Blue);
                break;
            case 3:
                inventory.AddCrystal(Mana.Types.White);
                break;
            case 4:
                inventory.AddCrystal(Mana.Types.Gold);
                break;
            case 5:
                inventory.AddCrystal(Mana.Types.Black);
                break;
        }
    }
}
