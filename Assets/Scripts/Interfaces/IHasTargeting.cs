
public enum TargetTypes {
    AnyCard,
    ActionCardChoice,
    UnitCard,
    AnyMana,
    ManaSource,
}

public interface IHasTargeting {
    TargetTypes TargetType { get; }
    bool HasTarget(CardChoice choice);
    void PreTargetSideEffect(CardChoice choice);
}

public interface ITargetingCard<T> : IHasTargeting {
    bool ValidTarget(CardChoice choice, T target);
    void TargetSideEffect(CardChoice choice, T target);
}