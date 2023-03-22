using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public Player CurrentPlayer { get; private set; }
    public Combat Combat { get; private set; }

    public bool DoneInitializing { get; private set; } = false;

    /* EVENT DEFINITIONS - START */
    public event EventHandler OnGameManagerDoneInitializing;
    public event EventHandler<OnCurrentPlayerChangedArgs> OnCurrentPlayerChanged;
    public class OnCurrentPlayerChangedArgs : EventArgs {
        public Player player;
    }
    /* EVENT DEFINITIONS - END */

    private void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogError("More than one instance of a singleton");
        } else {
            Instance = this;
        }
    }

    private void Start() {
        SetCurrentPlayer(GameObject.FindWithTag("Player").GetComponent<Player>()); // TODO: temp

        Combat.OnCombatEnd += Combat_OnCombatEnd;

        // Button click events
        ButtonInputManager.Instance.OnActionChooseClick += ButtonInput_OnActionChooseClick;

        SetDoneInitializing();
    }

    private void SetDoneInitializing() {
        DoneInitializing = true;
        OnGameManagerDoneInitializing?.Invoke(this, EventArgs.Empty);
        OnGameManagerDoneInitializing = null;
    }

    private void SetCurrentPlayer(Player player) {
        CurrentPlayer = player;
        OnCurrentPlayerChanged?.Invoke(this, new OnCurrentPlayerChangedArgs { player = player });
    }

    private Hex GetHexWithCurrentPlayer() {
        if (HexMap.Instance.TryGetHex(CurrentPlayer.Position, out Hex currentHex)) {
            return currentHex;
        } else {
            Debug.Log("Something went horribly wrong");
            return null;
        }
    }

    private void ChooseAction(ActionTypes action) {
        ActionTypes actionFinal = action;
        switch(action) {
            case ActionTypes.Combat:
                // TODO: optionally choose neighboring rampaging enemies
                Hex hex = GetHexWithCurrentPlayer();
                List<Enemy> enemies = hex.GetEnemies();
                foreach (Hex neighbor in HexMap.Instance.GetNeighbors(hex.Position)) {
                    enemies.AddRange(neighbor.GetEnemies());
                }
                Debug.Log("Enemies: " + enemies.Count);
                if (enemies.Count > 0) {
                    Combat = new Combat(CurrentPlayer, enemies);
                    Combat.Init();
                } else {
                    Debug.Log("Can't start combat with no enemies");
                    actionFinal = ActionTypes.None;
                }
                break;
        }

        Debug.Log("Choose Action:  " + actionFinal);
        RoundManager.Instance.SetPhaseAndAction(TurnPhases.Action, actionFinal);
    }

    private void EndOfCombat(CombatResult result) {
        Combat.Dispose();
        Combat = null;
        foreach (Enemy enemy in result.Defeated) {
            enemy.DestroySelf();
        }
    }

    /* ------------------- EVENTS ---------------------- */

    private void Combat_OnCombatEnd(object sender, Combat.OnCombatEndArgs e) {
        EndOfCombat(e.result);
    }

    private void ButtonInput_OnActionChooseClick(object sender, ButtonInputManager.OnActionChooseClickArgs e) {
        ChooseAction(e.actionType);
    }
}
