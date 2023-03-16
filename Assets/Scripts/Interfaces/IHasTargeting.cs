
public enum TargetTypes {
    Card,
    Action,
    ManaSource
}

public interface IHasTargeting {
    TargetTypes TargetType { get; }
    bool HasTarget(ActionChoice choice);
    void PreTargetSideEffect() { }
}

public interface ITargetingCard<T> : IHasTargeting {
    bool ValidTarget(ActionChoice choice, T target);
    void TargetSideEffect(ActionChoice choice, T target);
}