using System.Collections.Generic;
using UnityEngine;

public abstract class UnitCard : Card {
    public override string Name => UnitCardSO.Name;
    public override Types Type => Types.Unit;

    private UnitCardSO UnitCardSO => CardSO as UnitCardSO;

    public UnitCard(ActionCardSO UnitCardSO) : base(UnitCardSO) { }
}