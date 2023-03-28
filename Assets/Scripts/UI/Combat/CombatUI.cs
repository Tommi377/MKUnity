using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CombatUI : MonoBehaviour {
    [SerializeField] private Transform enemyContainer;
    [SerializeField] private ExpandingButtonUI bottomRightButtonContainer;

    [SerializeField] private Transform aliveTab;
    [SerializeField] private Transform deadTab;
    [SerializeField] private Transform aliveContainer;
    [SerializeField] private Transform deadContainer;
    [SerializeField] private Transform resultTab;

    [SerializeField] private GameObject enemyButtonVisualTemplate;
    [SerializeField] private GameObject enemySmallVisualTemplate;

    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text subheaderText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text resultDetailText;

    private Combat combat;
    private CombatResult result;
    private Combat.States state;
    private List<EnemyButtonVisual> enemyVisuals = new List<EnemyButtonVisual>();

    private void Awake() {
        gameObject.SetActive(false);
    }

    private void OnEnable() {
        combat = GameManager.Instance.Combat;
        SetState(combat.GetCurrentState());

        Combat.OnCombatStateEnter += Combat_OnCombatStateEnter;
        Combat.OnGenerateCombatResult += Combat_OnGenerateCombatResult;
    }

    private void OnDisable() {
        Combat.OnCombatStateEnter -= Combat_OnCombatStateEnter;
        Combat.OnGenerateCombatResult -= Combat_OnGenerateCombatResult;

        combat = null;
        result = null;

        enemyVisuals.ForEach(visual => visual.DestroySelf());
        enemyVisuals.Clear();

        resultTab.gameObject.SetActive(false);
        aliveTab.gameObject.SetActive(false);
        deadTab.gameObject.SetActive(false);

        foreach (Transform child in aliveContainer) Destroy(child.gameObject);
        foreach (Transform child in deadContainer) Destroy(child.gameObject);
    }

    private void SetState(Combat.States state) {
        this.state = state;

        if (enemyVisuals.Count == 0) {
            DrawEnemies();
        }
        
        UpdateEnemies();
        UpdateUI();
    }

    private void UpdateUI() {
        if (combat == null) return;

        gameObject.SetActive(true);

        bottomRightButtonContainer.ClearButtons();

        switch (state) {
            case Combat.States.Start:
                headerText.SetText("Start of Combat");
                subheaderText.SetText("These are your enemies:");

                bottomRightButtonContainer.AddButton("Start\nCombat", () => CombatAction.CombatNextStateClick(this));
                break;
            case Combat.States.RangedStart:
                headerText.SetText("Ranged Phase");
                subheaderText.SetText("Select enemies to target:");


                var options = new ExpandingButtonUI.Options() { Interactable = GetTargets().Count > 0 };
                string nextPhaseText = combat.EveryEnemyDefeated() ? "End\nCombat" : "To\nBlock\nPhase";

                bottomRightButtonContainer.AddButton("Attack\nEnemies", () => {
                    SelectTargets();
                    CombatAction.CombatNextStateClick(this);
                }, options);
                bottomRightButtonContainer.AddButton(nextPhaseText, () => CombatAction.CombatNextStateClick(this));
                break;
            case Combat.States.RangedPlay:
                subheaderText.SetText("Play ranged cards to damage");

                bottomRightButtonContainer.AddButton("Finish\nPlaying\nCards", () => CombatAction.CombatNextStateClick(this));
                break;
            case Combat.States.BlockStart:
                break;
            case Combat.States.BlockPlay:
                break;
            case Combat.States.AssignStart:
                break;
            case Combat.States.AssignDamage:
                break;
            case Combat.States.AttackStart:
                break;
            case Combat.States.AttackPlay:
                break;
            case Combat.States.Result:
                headerText.SetText("End of Combat");
                subheaderText.SetText("Combat details:");
                enemyContainer.gameObject.SetActive(false);

                if (aliveTab.gameObject.activeSelf == false && combat.Alive.Any()) {
                    aliveTab.gameObject.SetActive(true);
                    foreach (Enemy enemy in combat.Alive) {
                        EnemyVisual enemyVisual = Instantiate(enemySmallVisualTemplate, aliveContainer).GetComponent<EnemyVisual>();
                        enemyVisual.Init(enemy.EnemySO);
                        enemyVisual.gameObject.SetActive(true);
                    }
                }

                if (aliveTab.gameObject.activeSelf == false && combat.Defeated.Any()) {
                    deadTab.gameObject.SetActive(true);
                    foreach (Enemy enemy in combat.Defeated) {
                        EnemyVisual enemyVisual = Instantiate(enemySmallVisualTemplate, deadContainer).GetComponent<EnemyVisual>();
                        enemyVisual.Init(enemy.EnemySO);
                        enemyVisual.gameObject.SetActive(true);

                        var highlight = enemyVisual.GetComponent<ToggleableHighlightVisual>();
                        highlight.Select();
                    }
                }

                bottomRightButtonContainer.AddButton("Proceed", () => CombatAction.CombatNextStateClick(this));
                break;
        }
    }

    private void DrawResult(CombatResult result) {
        resultTab.gameObject.SetActive(true);
        string resultTextString = result.Alive.Any() ? "Ran Away" : "Combat Won";
        string detailTextString = "Fame Gained: " + result.Fame;
        detailTextString += "\nCurrent Fame: " + (result.Fame + GameManager.Instance.CurrentPlayer.Fame);
        if (GameManager.Instance.CurrentPlayer.CanLevelUp(result.Fame)) {
            detailTextString += "\nCurrent Level: " + (GameManager.Instance.CurrentPlayer.Level + 1) + " *LEVEL UP*";
        } else {
            detailTextString += "\nCurrent Level: " + GameManager.Instance.CurrentPlayer.Level;
        }
        detailTextString += "\n\nReputation Change: " + result.Reputation;
        detailTextString += "\nCurrent Rep Bonus: " + GameManager.Instance.CurrentPlayer.GetReputationBonus(result.Reputation);

        resultText.SetText(resultTextString);
        resultDetailText.SetText(detailTextString);
    }

    private void DrawEnemies() {
        foreach (Enemy enemy in combat.Enemies) {
            EnemyButtonVisual visual = Instantiate(enemyButtonVisualTemplate, enemyContainer).GetComponent<EnemyButtonVisual>();
            visual.Init(state, enemy);
            visual.gameObject.SetActive(true);

            enemyVisuals.Add(visual);

            visual.OnButtonClick += EnemyButtonVisual_OnButtonClick;
        }
    }

    private void UpdateEnemies() {
        if (state == Combat.States.Result) return;

        foreach(EnemyButtonVisual visual in enemyVisuals) {
            if (!visual.Dead && combat.Defeated.Contains(visual.Enemy)) {
                visual.SetDead();
            }

            visual.Deselect();
            visual.UpdateUI(state);
        }
    }

    private void SelectTargets() {
        CombatAction.TargetsSelectedClick(this, GetTargets());
    }

    private List<Enemy> GetTargets() {
        var targets = new List<Enemy>();
        foreach (EnemyButtonVisual visual in enemyVisuals) {
            if (visual.Selected) {
                targets.Add(visual.Enemy);
            }
        }
        return targets;
    }

    private void EnemyButtonVisual_OnButtonClick(int choiceId) {
        UpdateUI();
    }

    private void Combat_OnCombatStateEnter(object sender, Combat.OnCombatStateEnterArgs e) {
        SetState(e.State);
    }

    private void Combat_OnGenerateCombatResult(object sender, Combat.OnCombatResultArgs e) {
        DrawResult(e.result);
    }
}

public static class CombatAction {
    public static event EventHandler OnCombatNextStateClick;
    public static event EventHandler<OnTargetsSelectedClickArgs> OnTargetsSelectedClick;
    public class OnTargetsSelectedClickArgs : EventArgs {
        public List<Enemy> Targets;
    }

    public static void CombatNextStateClick(object sender) {
        OnCombatNextStateClick?.Invoke(sender, EventArgs.Empty);
    }

    public static void TargetsSelectedClick(object sender, List<Enemy> targets) {
        OnTargetsSelectedClick?.Invoke(sender, new OnTargetsSelectedClickArgs { Targets = targets });
    }
}