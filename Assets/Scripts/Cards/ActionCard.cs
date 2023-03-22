using System.Collections.Generic;
using UnityEngine;

public enum ActionTypeRestriction {
    Normal,
    Super,
    Both
}

public abstract class ActionCard : Card {
    public override string Name => ActionCardSO.Name;
    public override Types Type => Types.Action;
    public string DescUp => ActionCardSO.DescUp;
    public string DescDown => ActionCardSO.DescDown;
    public List<Mana.Types> ManaTypes => ActionCardSO.ManaTypes;

    private ActionCardSO ActionCardSO => CardSO as ActionCardSO;

    public ActionCard(ActionCardSO actionCardSO) : base(actionCardSO) { }
}