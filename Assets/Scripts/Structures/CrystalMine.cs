using System;
using System.Collections.Generic;
using UnityEngine;

public class CrystalMine : Structure {
    [SerializeField] Renderer[] crystalRenderers;

    public override StructureTypes StructureType => StructureTypes.CrystalMine;

    private void Awake() {
        foreach (var renderer in crystalRenderers) {
            renderer.material.color = Color.yellow;
        }
    }

    public override List<BaseAction> EndOfTurnActions(Player player) => new List<BaseAction>() {
        new BaseAction("Crystal", "Gain a mana crystal", () => player.GetInventory().AddCrystal())
    };
}