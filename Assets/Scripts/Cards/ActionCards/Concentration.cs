using System;
using System.Collections.Generic;
using System.Linq;

public class Concentration : ActionCard, ITargetingCard<(Card, CardChoice)> {
    private ActionCard suppliedCard;
    private CardChoice suppliedChoice;

    public Concentration(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public override bool CanApply(ActionTypes action, CardChoice actionChoice) {
        if (!base.CanApply(action, actionChoice)) return false;

        // TODO: Logic to decide whether the player has playable cards in hand
        return actionChoice.Id != 3 ||
            RoundManager.Instance.CurrentAction == ActionTypes.Move ||
            RoundManager.Instance.CurrentAction == ActionTypes.Influence ||
            RoundManager.Instance.CurrentAction == ActionTypes.Combat;
    }

    public TargetTypes TargetType => TargetTypes.Action;
    public bool HasTarget(CardChoice choice) => choice.Id == 3;

    public bool ValidTarget(CardChoice choice, (Card, CardChoice) target) {
        if (target.Item1 is not ActionCard) return false;

        ActionCard actionCard = target.Item1 as ActionCard;
        CardChoice targetChoice = target.Item2;
        ActionTypes actionType = RoundManager.Instance.CurrentAction;

        return targetChoice.Super && actionCard.CanPlay(actionType) && actionCard.CanApply(actionType, targetChoice);
    }

    public void PreTargetSideEffect() {
        EventSignalManager.ChangeHandUIMode(this, HandUI.Modes.OnlySuper);
    }

    public void TargetSideEffect(CardChoice choice, (Card, CardChoice) target) {
        suppliedCard = target.Item1 as ActionCard;
        suppliedChoice = target.Item2;
        GameManager.Instance.CurrentPlayer.DiscardCard(suppliedCard);
        EventSignalManager.ChangeHandUIMode(this, HandUI.Modes.Default);
    }

    public override void Apply(CardChoice choice) {
        base.Apply(choice);
        Player player = GameManager.Instance.CurrentPlayer;

        switch (choice.Id) {
            case 0:
                player.GainMana(ManaSource.Types.Blue);
                break;
            case 1:
                player.GainMana(ManaSource.Types.White);
                break;
            case 2:
                player.GainMana(ManaSource.Types.Red);
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
