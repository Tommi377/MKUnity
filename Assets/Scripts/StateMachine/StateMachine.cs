using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine {
    private TransitionTable transitionTable;
    private State currentState;

    public StateMachine(TransitionTable transitionTable) {
        this.transitionTable = transitionTable;
        currentState = transitionTable.GetInitialState();
    }

    public State GetCurrentState() => currentState;

    public void Tick() {
        TryTransition();

        currentState.OnTick();
    }

    private void TryTransition() {
        if (currentState.TryGetTransition(out State transitionState)) {
            currentState.OnStateExit();
            currentState = transitionState;
            currentState.OnStateEnter();
        }
    }
}
