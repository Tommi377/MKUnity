using System.Linq;
using UnityEngine;

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

    public TargetTypes TargetType => TargetTypes.ActionCardChoice;
    public bool HasTarget(CardChoice choice) => choice.Id == 3;

    public bool ValidTarget(CardChoice choice, (Card, CardChoice) target) {
        if (target.Item1 is not ActionCard) return false;

        ActionCard actionCard = target.Item1 as ActionCard;
        CardChoice targetChoice = target.Item2;
        ActionTypes actionType = RoundManager.Instance.CurrentAction;

        return targetChoice.ManaTypes.Any() && actionCard.HasPlayableChoices(actionType) && actionCard.CanApply(actionType, targetChoice);
    }

    public void PreTargetSideEffect(CardChoice choice) {
        EventSignalManager.ChangeHandUIMode(this, HandUI.SelectionMode.OnlySuper);
    }

    public void TargetSideEffect(CardChoice choice, (Card, CardChoice) target) {
        suppliedCard = target.Item1 as ActionCard;
        suppliedChoice = target.Item2;
        GameManager.Instance.CurrentPlayer.DiscardCard(suppliedCard);
        EventSignalManager.ChangeHandUIMode(this, HandUI.SelectionMode.Default);
    }

    public override void Apply(CardChoice choice) {
        switch (choice.Id) {
            case 0:
                GetPlayer().GetInventory().AddToken(Mana.Types.Blue);
                break;
            case 1:
                GetPlayer().GetInventory().AddToken(Mana.Types.White);
                break;
            case 2:
                GetPlayer().GetInventory().AddToken(Mana.Types.Red);
                break;
            case 3:
                int initMovement = GetPlayer().Movement;
                int initInfluence = GetPlayer().Influence;
                int combatCardCount = GetPlayer().IsInCombat() ? GetCombat(GetPlayer()).CombatCards.Count() : 0;

                CardManager.Instance.AddUnresolvedCard(this, () => {
                    switch (RoundManager.Instance.CurrentAction) {
                        case ActionTypes.Move:
                            if (GetPlayer().Movement > initMovement) {
                                GetPlayer().AddMovement(2);
                            }
                            break;
                        case ActionTypes.Influence:
                            if (GetPlayer().Influence > initInfluence) {
                                GetPlayer().AddInfluence(2);
                            }
                            break;
                        case ActionTypes.Combat:
                            Combat combat = GetCombat(GetPlayer());
                            if (combat.CombatCards.Count() > combatCardCount) {
                                CombatData combatCard = combat.CombatCards.Last();

                                combat.PlayCombatCard(new CombatData(2, combatCard.CombatType, combatCard.CombatElement));
                            }
                            break;
                    }
                });

                CardManager.Instance.PlayCard(suppliedCard, suppliedChoice, new PlayCardOptions() { SkipManaUse = true });
                suppliedCard = null;
                suppliedChoice = null;
                break;
        }
    }
}
