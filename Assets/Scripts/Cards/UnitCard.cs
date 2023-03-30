using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitCard : Card {

    public event EventHandler OnUnitExhaustChanged;
    public event EventHandler OnUnitWoundChanged;

    public override string Name => UnitCardSO.Name; 
    public override Types Type => Types.Unit;
    public int Level => UnitCardSO.Level;
    public int Influence => UnitCardSO.Influence;
    public int Armor => UnitCardSO.Armor;
    public List<StructureTypes> Locations => UnitCardSO.Locations;

    // Variables
    public bool Exhausted { get; private set; } = false;
    public bool Wounded => Wounds > 0;
    public int Wounds { get; private set; } = 0;


    public UnitCardSO UnitCardSO => CardSO as UnitCardSO;

    public UnitCard(UnitCardSO UnitCardSO) : base(UnitCardSO) { }

    public override bool CanApply(ActionTypes action, CardChoice cardChoice) {
        if (!base.CanApply(action, cardChoice)) return false;
        if (Wounds > 0) return false;
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

    public void WoundUnit(bool poison = false) {
        Wounds++;
        if (poison) Wounds++;
        OnUnitWoundChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Heal() {
        if (Wounds == 0) {
            Debug.Log("Can't heal while unit is full hp");
            return;
        }
        Wounds--;
        OnUnitWoundChanged?.Invoke(this, EventArgs.Empty);
    }

    public override string ToString() => Name + " lvl: " + Level;
}