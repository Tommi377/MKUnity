using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Village : Structure {
    private List<Player> plundered = new List<Player>();

    private void Start() {
        RoundManager.Instance.OnNewTurn += RoundManager_OnNewTurn;
    }

    public override List<BaseAction> StartOfTurnActions(Player player) => VillageActionList(player);
    // public override List<BaseAction> EndOfTurnActions(Player player) => VillageActionList(player);/

    public override bool CanInfluence(Player player) => true;

    public override List<InfluenceAction> InfluenceChoices(Player player) => new List<InfluenceAction> {
        new InfluenceAction("Heal 1", "Heal 1 wound from your hand", 3, (p) => p.HealWounds(1)),
        // TODO: Add recruitment
    };

    private List<BaseAction> VillageActionList(Player player) {
        if (plundered.Contains(player)) return new List<BaseAction>();
        return new List<BaseAction>() { new BaseAction("Plunder", "Burn the village for 2 cards", () => PlunderVillage(player)) };
    }

    private void PlunderVillage(Player player) {
        player.ReduceReputation(1);
        player.DrawCards(2);
        plundered.Add(player);
    }

    private void RoundManager_OnNewTurn(object sender, EventArgs e) {
        plundered.Clear();
    }
}