using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemCard : Card {

    public event EventHandler OnUnitExhaustChanged;

    public override string Name => ItemCardSO.Name; 
    public override Types Type => Types.Item;
    public int Level => ItemCardSO.Tier;
    public int Cost => ItemCardSO.Cost;
    public int Weight => ItemCardSO.Weight;
    public List<StructureTypes> Locations => ItemCardSO.Locations;
    public ItemUseRate UseRate => ItemCardSO.UseRate;

    // Variables
    public bool Exhausted { get; private set; } = false;


    public ItemCardSO ItemCardSO => CardSO as ItemCardSO;

    public ItemCard(ItemCardSO UnitCardSO) : base(UnitCardSO) { }

    public override bool CanApply(ActionTypes action, CardChoice cardChoice) {
        if (!base.CanApply(action, cardChoice)) return false;
        return !Exhausted;
    }

    public override void ApplyChoice(CardChoice choice) {
        Exhausted = true;
        OnUnitExhaustChanged?.Invoke(this, EventArgs.Empty);
        Apply(choice);
    }

    public void Ready() {
        Exhausted = false;
        OnUnitExhaustChanged?.Invoke(this, EventArgs.Empty);
    }

    public override string ToString() => Name + " lvl: " + Level;
}

public enum ItemUseRate {
    Day, Turn, Passive
}