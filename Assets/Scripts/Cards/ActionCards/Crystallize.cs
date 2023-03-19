using System;
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

    public List<(string, Action)> EffectChoices(CardChoice choice) => new List<(string, Action)> {
        ("Red", () => GameManager.Instance.CurrentPlayer.GetInventory().AddCrystal(Mana.Types.Red)),
        ("Green", () => GameManager.Instance.CurrentPlayer.GetInventory().AddCrystal(Mana.Types.Green)),
        ("Blue", () => GameManager.Instance.CurrentPlayer.GetInventory().AddCrystal(Mana.Types.Blue)),
        ("White", () => GameManager.Instance.CurrentPlayer.GetInventory().AddCrystal(Mana.Types.White)),
        ("Gold", () => GameManager.Instance.CurrentPlayer.GetInventory().AddCrystal(Mana.Types.Gold)),
        ("Black", () => GameManager.Instance.CurrentPlayer.GetInventory().AddCrystal(Mana.Types.Black)),
    };
}
