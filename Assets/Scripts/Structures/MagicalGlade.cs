using System;
using System.Collections.Generic;
using UnityEngine;

public class MagicalGlade : Structure {
    public override StructureTypes StructureType => StructureTypes.Glade;

    public override List<BaseAction> StartOfTurnActions(Player player) => new List<BaseAction>() {
        new BaseAction(
            "Channel 1",
            "Channel 1",
            () => player.AddMana(1)
        )
    };

    public override List<BaseAction> EndOfTurnActions(Player player) => new List<BaseAction>() {
        new BaseAction("Heal 1 (hand)", "Heal one wound from hand", () => player.HealWounds()),
        new BaseAction("Heal 1 (discrad)", "Heal one wound from discard", () => player.TryRemoveWoundFromDiscard())
    };
}