using System;
using System.Collections.Generic;
using UnityEngine;

public class CrystalMine : Structure {
    public Mana.Types ManaType { get; private set; }

    private void Start() {
        ManaType = Mana.GetRandomType();
    }

    public override List<BaseAction> EndOfTurnActions(Player player) => new List<BaseAction>() {
        new BaseAction("Crystal", "Gain a " + ManaType + " crystal", () => player.GetInventory().AddCrystal(ManaType))
    };
}