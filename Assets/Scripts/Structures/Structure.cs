using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureAction {
    public string Name;
    public string Description;
    public Action<Player> Apply;

    public StructureAction(string name, string description, Action<Player> apply) {
        Name = name;
        Description = description;
        Apply = apply;
    }
}

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
    public virtual void StartOfTurn(Player player) { }
    public virtual void EndOfTurn(Player player) { }
    public virtual List<StructureAction> PreTurnChoices(Player player) => new List<StructureAction>();

    public virtual bool CanInfluence(Player player) => false;
    public virtual bool CanFight(Player player) => false;

    public virtual List<InfluenceAction> InfluenceChoices(Player player) => new List<InfluenceAction>();

}