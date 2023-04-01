using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

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

    public List<ActionTypes> GetPossibleActions() {
        List<ActionTypes> actions = new List<ActionTypes>();
        Hex currentHex = CurrentPlayer.GetHex();

        if (currentHex.Entities.Any(e => e.IsAggressive())) {
            // Ended movement on a nonsafe tile
            actions.Add(ActionTypes.Combat);
        } else {
            // Structure actions
            if (CanInfluence()) {
                actions.Add(ActionTypes.Influence);
            }

            // Check if combat possible with rampaging enemies
            List<Enemy> rampaging = new List<Enemy>();
            foreach (Hex neighbor in HexMap.Instance.GetNeighbors(CurrentPlayer.Position)) {
                foreach (Enemy enemy in neighbor.GetEnemies()) {
                    if (enemy.Rampaging) rampaging.Add(enemy);
                }
            }
            if (rampaging.Any()) actions.Add(ActionTypes.Combat);

            // TODO: Add card action turn if applicable

            actions.Add(ActionTypes.None); // End turn without doing actions
        }

        return actions;
    }

    private bool CanInfluence() {
        Hex currentHex = CurrentPlayer.GetHex();
        if (currentHex.ContainsStructure() && currentHex.Structure.CanInfluence(CurrentPlayer)) return true;
        if (UnitManager.Instance.GetUnitOfferForStructure(currentHex.StructureType).Any()) return true;
        return false;
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
                List<Enemy> forced = new List<Enemy>(enemies);

                foreach (Hex neighbor in HexMap.Instance.GetNeighbors(hex.Position)) {
                    enemies.AddRange(neighbor.GetEnemies());
                }

                Debug.Log("Enemies: " + enemies.Count);
                if (enemies.Count > 0) {
                    Combat = new Combat(CurrentPlayer, enemies, forced);
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

    private void Combat_OnCombatEnd(object sender, Combat.OnCombatResultArgs e) {
        EndOfCombat(e.result);
    }

    private void ButtonInput_OnActionChooseClick(object sender, ButtonInputManager.OnActionChooseClickArgs e) {
        ChooseAction(e.actionType);
    }
}
