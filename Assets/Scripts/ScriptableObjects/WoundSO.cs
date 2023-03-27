using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "Cards/Wound")]
public class WoundSO : CardSO {
    public override Card.Types Type => Card.Types.Wound;
    public override Card CreateInstance() => new Wound(this);
}
