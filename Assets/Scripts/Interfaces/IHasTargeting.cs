
public enum TargetTypes {
    Card,
    Action,
    ManaSource
}

public interface IHasTargeting {
    TargetTypes TargetType { get; }
    bool HasTarget(CardChoice choice);
    void PreTargetSideEffect() { }
}

public interface ITargetingCard<T> : IHasTargeting {
    bool ValidTarget(CardChoice choice, T target);
    void TargetSideEffect(CardChoice choice, T target);
}