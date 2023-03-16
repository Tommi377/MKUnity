using System;
using System.Collections.Generic;
using System.Linq;

public class Crystallize : ActionCard, ISpecialCard, ITargetingCard<(Card, ActionChoice)> {
    private ActionCard suppliedCard;
    private ActionChoice suppliedChoice;

    public List<ActionChoice> ChoicesSpecial() {
        return new List<ActionChoice>() {
            new ActionChoice("Gain blue (N)", "Gain blue (N)", false, 0, ActionTypes.Special),
            new ActionChoice("Gain white (N)", "Gain white (N)", false, 1, ActionTypes.Special),
            new ActionChoice("Gain red (N)", "Gain red (N)", false, 2, ActionTypes.Special),
            new ActionChoice("Buff card +2 (S)", "Buff card +2 (S)", true, 3, ActionTypes.Special),
        };
    }

    public Crystallize(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override bool CanApply(ActionTypes action, ActionChoice actionChoice) {
        if (!base.CanApply(action, actionChoice)) return false;

        return actionChoice.Id != 3 ||
            RoundManager.Instance.CurrentAction == ActionTypes.Move ||
            RoundManager.Instance.CurrentAction == ActionTypes.Influence ||
            RoundManager.Instance.CurrentAction == ActionTypes.Combat;
    }

    public TargetTypes TargetType => TargetTypes.Action;
    public bool HasTarget(ActionChoice choice) => choice.Id == 3;

    public bool ValidTarget(ActionChoice choice, (Card, ActionChoice) target) {
        if (target.Item1 is not ActionCard) return false;

        ActionCard actionCard = target.Item1 as ActionCard;
        ActionChoice targetChoice = target.Item2;
        ActionTypes actionType = RoundManager.Instance.CurrentAction;

        return targetChoice.Super && actionCard.CanPlay(actionType) && actionCard.CanApply(actionType, targetChoice);
    }

    public void PreTargetSideEffect() {
        EventSignalManager.ChangeHandUIMode(this, HandUI.Modes.OnlySuper);
    }

    public void TargetSideEffect(ActionChoice choice, (Card, ActionChoice) target) {
        suppliedCard = target.Item1 as ActionCard;
        suppliedChoice = target.Item2;
        GameManager.Instance.CurrentPlayer.DiscardCard(suppliedCard);
        EventSignalManager.ChangeHandUIMode(this, HandUI.Modes.Default);
    }

    public override void Apply(ActionChoice choice) {
        base.Apply(choice);
        Player player = GameManager.Instance.CurrentPlayer;

        // TODO: Give mana tokens
        // TODO: Make the target card forced to be super
        switch (choice.Id) {
            case 0:
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                int initMovement = player.Movement;
                int initInfluence = player.Influence;
                int combatCardCount = player.IsInCombat() ? GetCombat(player).CombatCards.Count() : 0;

                suppliedCard.Apply(suppliedChoice);

                switch (RoundManager.Instance.CurrentAction) {
                    case ActionTypes.Move:
                        if (player.Movement > initMovement) {
                            player.AddMovement(2);
                        }
                        break;
                    case ActionTypes.Influence:
                        if (player.Influence > initInfluence) {
                            player.AddInfluence(2);
                        }
                        break;
                    case ActionTypes.Combat:
                        Combat combat = GetCombat(player);
                        if (combat.CombatCards.Count() > combatCardCount) {
                            CombatData combatCard = combat.CombatCards.Last();

                            combat.PlayCombatCard(new CombatData(2, combatCard.CombatType, combatCard.CombatElement));
                        }
                        break;
                }
                break;
        }
    }
}
