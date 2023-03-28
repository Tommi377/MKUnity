using System;

public abstract class StateCondition : IStateComponent {
    public bool IsMet() => Statement();

    protected abstract bool Statement();

    public virtual void Awake(StateMachine stateMachine) { }
    public virtual void OnStateEnter() { }
    public virtual void OnStateExit() { }
}

public class VoidStateCondition : StateCondition {
    private Func<bool> condition;

    public VoidStateCondition(Func<bool> condition) {
        this.condition = condition;
    }

    protected override bool Statement() => condition();
}
