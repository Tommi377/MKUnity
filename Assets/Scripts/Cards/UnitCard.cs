using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitCard : Card {

    public static event EventHandler<OnUnitExhaustChangedArgs> OnUnitExhaustChanged;
    public class OnUnitExhaustChangedArgs : EventArgs {
        public UnitCard Card;
        public bool Exhausted;
    }

    public override string Name => UnitCardSO.Name; 
    public override Types Type => Types.Unit;
    public int Level => UnitCardSO.Level;
    public int Influence => UnitCardSO.Influence;
    public int Armor => UnitCardSO.Armor;
    public List<StructureTypes> Locations => UnitCardSO.Locations;

    // Variables
    public bool Exhausted { get; private set; } = false;
    public int Wounds { get; private set; } = 0;

    public UnitCardSO UnitCardSO => CardSO as UnitCardSO;

    public UnitCard(UnitCardSO UnitCardSO) : base(UnitCardSO) { }

    public override bool CanApply(ActionTypes action, CardChoice cardChoice) {
        if (!base.CanApply(action, cardChoice)) return false;
        return !Exhausted;
    }

    public override void ApplyChoice(CardChoice choice) {
        Exhausted = true;
        OnUnitExhaustChanged?.Invoke(this, new OnUnitExhaustChangedArgs { Card = this, Exhausted = Exhausted });
        Apply(choice);
    }

    public void Ready() {
        Exhausted = false;
        OnUnitExhaustChanged?.Invoke(this, new OnUnitExhaustChangedArgs { Card = this, Exhausted = Exhausted });
    }

    public void Wound(bool poison) {
        Wounds++;
        if (poison) Wounds++;
    }

    public void Heal() {
        if (Wounds == 0) {
            Debug.Log("Can't heal while unit is full hp");
            return;
        }
        Wounds--;
    }
}