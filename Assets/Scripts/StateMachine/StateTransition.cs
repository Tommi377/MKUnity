using System;
using System.Collections.Generic;
using System.Linq;

public class StateTransition : IStateComponent {
    public State FromState;
    public State ToState;
    private Func<bool> condition; // [ [cond1 && cond2] || [cond3] ]

    public StateTransition(State fromState, State toState, Func<bool> condition) {
        FromState = fromState;
        ToState = toState;
        this.condition = condition;
    }

    public bool TryGetTransition(out State state) {
        state = CanTransition() ? ToState : null;
        return state != null;
    }

    private bool CanTransition() => condition();

    public virtual void Awake(StateMachine stateMachine) { }
    public virtual void OnStateEnter() { }
    public virtual void OnStateExit() { }
}
