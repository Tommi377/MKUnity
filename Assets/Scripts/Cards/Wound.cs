using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wound : Card {
    public Wound(CardSO cardSO) : base(cardSO) { }

    public override string Name => "Wound";
    public override Types Type => Types.Wound;

    public override bool CanApply(ActionTypes action, CardChoice actionChoice) {
        return false;
    }

    public override bool CanPlay(ActionTypes action) {
        return false;
    }

    public override void Apply(CardChoice choice) { }

    public override List<CardChoice> Choices(ActionTypes actionType) {
        return new List<CardChoice>();
    }
}
