﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class CrystalMine : Structure {
    [SerializeField] Renderer[] crystalRenderers;

    private Mana.Types manaType;

    private void Awake() {
        manaType = Mana.GetRandomType();
        foreach (var renderer in crystalRenderers) {
            renderer.material.color = Mana.GetColor(manaType);
        }
    }

    public override List<BaseAction> EndOfTurnActions(Player player) => new List<BaseAction>() {
        new BaseAction("Crystal", "Gain a " + manaType + " crystal", () => player.GetInventory().AddCrystal(manaType))
    };
}