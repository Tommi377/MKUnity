using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wound : Card {
    public Wound(CardSO cardSO) : base(cardSO) { }

    public override string Name => "Wound";
    public override Types Type => Types.Wound;

    public override bool CanApply(ActionTypes action, ActionChoice actionChoice) {
        return false;
    }

    public override bool CanPlay(ActionTypes action) {
        return false;
    }

    public override void Apply(ActionChoice choice) { }


    public override List<ActionChoice> ChoicesDefault(ActionTypes type) {
        return new List<ActionChoice>();
    }

    public override List<ActionChoice> Choices(ActionTypes actionType) {
        return new List<ActionChoice>();
    }
}
