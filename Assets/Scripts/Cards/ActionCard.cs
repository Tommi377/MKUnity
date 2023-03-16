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

    public override bool CanApply(ActionTypes action, ActionChoice actionChoice) {
        // Test whether action type can be applied with normal/super
        int index = ActionTypeList.IndexOf(action);


        if (index >= 0 && CanDoAction(actionChoice, ActionCardSO.ActionTypeRestrictionList[index])) return true;

        int specialIndex = ActionTypeList.IndexOf(ActionTypes.Special);
        if (specialIndex >= 0 && CanDoAction(actionChoice, ActionCardSO.ActionTypeRestrictionList[specialIndex])) return true;

        int healIndex = ActionTypeList.IndexOf(ActionTypes.Heal);
        if (
            healIndex >= 0 && GameManager.Instance.CurrentPlayer.IsInCombat() &&
            CanDoAction(actionChoice, ActionCardSO.ActionTypeRestrictionList[healIndex])
        ) return true;

        Debug.Log("The card is not playable at all in the first place");
        return false;
    }

    public override List<ActionChoice> Choices(ActionTypes actionType) {
        if (actionType == ActionTypes.Move && this is IMovementCard) {
            IMovementCard card = (IMovementCard)this;
            return card.ChoicesMove();
        } else if (actionType == ActionTypes.Combat && this is ICombatCard) {
            ICombatCard card = (ICombatCard)this;
            return card.ChoicesCombat();
        }

        return new List<ActionChoice>();
    }

    private bool CanDoAction(ActionChoice actionChoice, ActionTypeRestriction restriction) {
        if (restriction == ActionTypeRestriction.Both) return true;

        return actionChoice.Super && restriction == ActionTypeRestriction.Super ||
            !actionChoice.Super && restriction == ActionTypeRestriction.Normal;
    }
}
