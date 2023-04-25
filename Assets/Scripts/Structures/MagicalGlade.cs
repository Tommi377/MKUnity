using System;
using System.Collections.Generic;
using UnityEngine;

public class MagicalGlade : Structure {
    public override StructureTypes StructureType => StructureTypes.Glade;

    public override List<BaseAction> StartOfTurnActions(Player player) => new List<BaseAction>() {
        new BaseAction(
            "Gain " + (RoundManager.Instance.IsDay() ? "Gold" : "Black") + "\ntoken",
            "Gold on day. Black on night",
            () => player.GetInventory().AddToken(RoundManager.Instance.IsDay() ? Mana.Types.Gold : Mana.Types.Black)
        )
    };

    public override List<BaseAction> EndOfTurnActions(Player player) => new List<BaseAction>() {
        new BaseAction("Heal 1 (hand)", "Heal one wound from hand", () => player.HealWound()),
        new BaseAction("Heal 1 (discrad)", "Heal one wound from discard", () => player.TryRemoveWoundFromDiscard())
    };
}