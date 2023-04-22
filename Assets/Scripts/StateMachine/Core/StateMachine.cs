using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StateMachine {
    private State currentState;

    public StateMachine(State initialState) {
        currentState = initialState;
    }

    public StateMachine(State initialState, List<StateTransition> transitions) {
        currentState = initialState;

        var fromStates = transitions.GroupBy(transition => transition.FromState);
        foreach (var fromState in fromStates) {
            State state = fromState.Key;

            foreach (StateTransition transition in fromState) {
                state.AddTransition(transition);
            }
        }
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

            int loop = 0;
            int maxLoops = 20;
            while (currentState.TrySkipTransition(out State skipState)) {
                currentState = skipState;
                loop++;

                if (loop > maxLoops) {
                    Debug.LogError("StateMachine: exceeded " + maxLoops + " state skips");
                    break;
                }
            }

            currentState.OnStateEnter();
        }
    }
}
