using System;
using System.Collections.Generic;
using UnityEngine;

public class MagicalGlade : Structure {
    public override List<BaseAction> StartOfTurnActions(Player player) => new List<BaseAction>() {
        new BaseAction(
            "Gain " + (RoundManager.Instance.IsDay() ? "Gold" : "Black") + "\ntoken",
            "Gold on day. Black on night",
            () => player.GetInventory().AddToken(RoundManager.Instance.IsDay() ? Mana.Types.Gold : Mana.Types.Black)
        )
    };

    public override List<BaseAction> EndOfTurnActions(Player player) => new List<BaseAction>() {
        new BaseAction("Heal 1", "Heal one wound", () => player.HealWounds(1)) // TODO: also allow from discard
    };
}