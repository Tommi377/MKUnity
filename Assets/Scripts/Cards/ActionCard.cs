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
    public List<ManaSource.Types> ManaTypes => ActionCardSO.ManaTypes;
    public List<ActionTypes> ActionTypeList => ActionCardSO.ActionTypeList;

    private ActionCardSO ActionCardSO => CardSO as ActionCardSO;

    public ActionCard(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override bool CanPlay(ActionTypes action) {
        return ActionTypeList.Contains(action);
    }

    public override bool CanApply(ActionTypes action, CardChoice cardChoice) {
        if (cardChoice.ActionType == ActionTypes.Special) return true; // Special cards can be played in any time
        if (cardChoice.ActionType == ActionTypes.Heal && !GameManager.Instance.CurrentPlayer.IsInCombat()) return true; // Heal cards can only be played out of combat
        if (action == cardChoice.ActionType) return true; // Actions that match the round action can be played

        return false;
    }
}
