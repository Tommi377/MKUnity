using System;
using System.Collections.Generic;
using System.Linq;

public class Crystallize : ActionCard, ITargetingCard<(Card, CardChoice)> {

    public Crystallize(ActionCardSO actionCardSO) : base(actionCardSO) { }

    public TargetTypes TargetType => TargetTypes.Action;
    public bool HasTarget(CardChoice choice) => true;

    public bool ValidTarget(CardChoice choice, (Card, CardChoice) target) {
        return true;
    }

    public void PreTargetSideEffect() {}

    public void TargetSideEffect(CardChoice choice, (Card, CardChoice) target) {}

    public override void Apply(CardChoice choice) {}
}
