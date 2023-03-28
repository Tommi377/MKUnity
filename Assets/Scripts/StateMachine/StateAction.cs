using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateAction : IStateComponent {
    public abstract void OnTick();

    public virtual void Awake(StateMachine stateMachine) { }
    public virtual void OnStateEnter() { }
    public virtual void OnStateExit() { }
}
