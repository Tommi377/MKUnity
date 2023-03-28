using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateAction : IStateComponent {
    public virtual void OnTick() { }
    public virtual void Awake(StateMachine stateMachine) { }
    public virtual void OnStateEnter() { }
    public virtual void OnStateExit() { }
}

public class OnStateEnterAction : StateAction {
    private Action action;

    public OnStateEnterAction(Action action) {
        this.action = action;
    }

    public static OnStateEnterAction Create(Action action) => new OnStateEnterAction(action);

    public override void OnStateEnter() => action();
}

public class OnStateExitAction : StateAction {
    private Action action;

    public OnStateExitAction(Action action) {
        this.action = action;
    }

    public static OnStateExitAction Create(Action action) => new OnStateExitAction(action);

    public override void OnStateExit() => action();
}