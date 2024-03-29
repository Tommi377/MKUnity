using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class State {
    public int ID{ get; private set; }
    private List<StateTransition> transitions = new List<StateTransition>();
    private List<StateTransition> skipTransitions = new List<StateTransition>();
    private List<StateAction> actions = new List<StateAction>();

    public State(int id) {
        ID = id;
    }

    public State(int id, StateAction action) {
        ID = id;
        actions.Add(action);
    }

    public State(int id, List<StateAction> actions) {
        ID = id;
        this.actions.AddRange(actions);
    }

    public StateTransition To(State to, Func<bool> condition) {
        StateTransition transition = new StateTransition(this, to, condition);
        transitions.Add(transition);
        return transition;
    }

    public StateTransition To(State to, Func<bool> condition, Func<bool> skipCondition) {
        StateTransition transition = new StateTransition(this, to, condition);
        transitions.Add(transition);
        skipTransitions.Add(new StateTransition(this, to, skipCondition));
        return transition;
    }

    public void AddTransition(StateTransition transition) {
        transitions.Add(transition);
    }

    public void AddAction(StateAction action) {
        actions.Add(action);
    }

    public bool TryGetTransition(out State state) {
        state = null;

        foreach (StateTransition transition in transitions) {
            if (transition.TryGetTransition(out state)) break;
        }

        return state != null;
    }

    public bool TrySkipTransition(out State state) {
        state = null;

        foreach (StateTransition transition in skipTransitions) {
            if (transition.TryGetTransition(out state)) break;
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
