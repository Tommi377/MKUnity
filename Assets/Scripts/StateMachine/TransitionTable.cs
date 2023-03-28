using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TransitionTable {
    private State initialState;

    public TransitionTable(State initialState, List<StateTransition> transitions) {
        this.initialState = initialState;

        var fromStates = transitions.GroupBy(transition => transition.FromState);
        foreach (var fromState in fromStates) {
            State state = fromState.Key;

            foreach (StateTransition transition in fromState) {
                state.AddTransition(transition);
            }
        }
    }

    public State GetInitialState() => initialState;
}