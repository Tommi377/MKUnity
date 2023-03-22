using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfluenceAction {
    public string Name;
    public string Description;
    public int Cost;
    public Action<Player> Apply;

    public InfluenceAction(string name, string description, int cost, Action<Player> apply) {
        Name = name;
        Description = description;
        Cost = cost;
        Apply = apply;
    }
}

public abstract class Structure : MonoBehaviour {
    private static readonly List<BaseAction> emptyList = new List<BaseAction>();

    public virtual List<BaseAction> StartOfTurnActions(Player player) => emptyList;
    public virtual List<BaseAction> EndOfTurnActions(Player player) => emptyList;

    public virtual bool CanInfluence(Player player) => false;
    public virtual bool CanFight(Player player) => false;

    public virtual List<InfluenceAction> InfluenceChoices(Player player) => new List<InfluenceAction>();

}