using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State {
    public int ID{ get; private set; }
    private List<StateTransition> transitions;
    private List<StateAction> actions;

    public State(int id) {
        ID = id;
    }

    public void AddTransition(StateTransition transition) {
        transitions.Add(transition);
    }

    public bool TryGetTransition(out State state) {
        state = null;

        foreach (StateTransition transition in transitions) {
            transition.TryGetTransition(out state);
            break;
        }

        return state != null;
    }

    public void OnTick() {
        foreach (StateAction action in actions) {
            action.OnTick();
        }
    }

    public void OnStateEnter() {
        foreach (IStateComponent transition in transitions) {
            transition.OnStateEnter();
        }
        foreach (IStateComponent action in actions) {
            action.OnStateEnter();
        }
    }

    public void OnStateExit() {
        foreach (IStateComponent transition in transitions) {
            transition.OnStateExit();
        }
        foreach (IStateComponent action in actions) {
            action.OnStateExit();
        }
    }
}
