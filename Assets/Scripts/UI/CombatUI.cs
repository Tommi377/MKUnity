using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CombatUI : MonoBehaviour {
    [SerializeField] private Transform enemyContainer;
    [SerializeField] private ExpandingButtonUI middleButtonContainer;
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
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text resultDetailText;

    private Combat combat;
    private Combat.States state;
    private List<EnemyButtonVisual> enemyVisuals = new List<EnemyButtonVisual>();

    private EnemyButtonVisual selectedEnemy;

    //private void Awake() {
    //    gameObject.SetActive(false);
    //}

    private void OnEnable() {
        combat = GameManager.Instance?.Combat;
        if (combat == null) {
            Debug.LogError("Can't enable combat UI without combat");
            return;
        }

        ResetUI();

        SetState(combat.GetCurrentState());

        Combat.OnCombatStateEnter += Combat_OnCombatStateEnter;
        Combat.OnGenerateCombatResult += Combat_OnGenerateCombatResult;
        Combat.OnCombatDamageAssign += Combat_OnCombatDamageAssign;

        if (CardManager.Instance) {
            CardManager.Instance.OnPlayCard += CardManager_OnPlayCard;
        }
    }

    private void OnDisable() {
        Combat.OnCombatStateEnter -= Combat_OnCombatStateEnter;
        Combat.OnGenerateCombatResult -= Combat_OnGenerateCombatResult;
        Combat.OnCombatDamageAssign -= Combat_OnCombatDamageAssign;

        if (CardManager.Instance) {
            CardManager.Instance.OnPlayCard -= CardManager_OnPlayCard;
        }

        combat = null;
    }

    private void ResetUI() {
        enemyVisuals.ForEach(visual => visual.DestroySelf());
        enemyVisuals.Clear();

        resultTab.gameObject.SetActive(false);
        aliveTab.gameObject.SetActive(false);
        deadTab.gameObject.SetActive(false);
        resultTab.gameObject.SetActive(false);

        foreach (Transform child in aliveContainer) Destroy(child.gameObject);
        foreach (Transform child in deadContainer) Destroy(child.gameObject);
    }

    private void SetState(Combat.States state) {
        this.state = state;

        DrawEnemies();
        UpdateUI();
        //UpdateInfoText();
    }

    private void UpdateUI() {
        if (combat == null) return;

        gameObject.SetActive(true);
        enemyContainer.gameObject.SetActive(true);

        middleButtonContainer.ClearButtons();
        bottomRightButtonContainer.ClearButtons();

        switch (state) {
            case Combat.States.Start:
                string proceedButtonString = "Start\nCombat";
                string subheaderString = "These are your enemies:";

                headerText.SetText("Start of Combat");
                subheaderText.SetText(subheaderString);

                bottomRightButtonContainer.AddButton(proceedButtonString, () => CombatAction.CombatNextStateClick(this));
                break;
            case Combat.States.Block:
                headerText.SetText("Block Phase");
                subheaderText.SetText("Play blocks or fast attacks:");

                string blockButtonText = combat.EveryEnemyDefeated() ? "End\nCombat" : combat.HasUnassignedAttacks() ? "To\nDamage\nPhase" : "To\nAttack\nPhase";
                bottomRightButtonContainer.AddButton(blockButtonText, () => CombatAction.CombatNextStateClick(this));
                break;
            case Combat.States.Assign:
                headerText.SetText("Damage Phase");
                subheaderText.SetText("Select attack to receive");

                string assignText = "To\nAttack\nPhase";
                var options = new ExpandingButtonUI.Options { Interactable = !combat.HasUnassignedAttacks() };
                bottomRightButtonContainer.AddButton(assignText, () => CombatAction.CombatNextStateClick(this), options);
                break;
            case Combat.States.Attack:
                headerText.SetText("Block Phase");
                subheaderText.SetText("Play attacks:");

                bottomRightButtonContainer.AddButton("End\nCombat", () => CombatAction.CombatNextStateClick(this));
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

                if (deadTab.gameObject.activeSelf == false && combat.Defeated.Any()) {
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
        string resultTextString = result.Alive.Any() || result.Defeated.Count == 0 ? "Ran Away" : "Combat Won";
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

    private void ResetEnemies() {
        enemyVisuals.Clear();
        aliveTab.gameObject.SetActive(false);
        deadTab.gameObject.SetActive(false);

        foreach (Transform child in enemyContainer) {
            Destroy(child.gameObject);
        }
        foreach (Transform child in aliveContainer) {
            Destroy(child.gameObject);
        }
        foreach (Transform child in deadContainer) {
            Destroy(child.gameObject);
        }
    }

    private void DrawEnemies() {
        ResetEnemies();

        if (combat.Defeated.Count > 0) {
            deadTab.gameObject.SetActive(true);
            foreach (Enemy enemy in combat.Defeated) {
                EnemyVisual enemyVisual = EnemySmallVisualInstantiate(enemy, deadContainer);

                var deadlight = enemyVisual.GetComponent<ToggleableHighlightVisual>();
                deadlight.Select();
            }
        }

        foreach (Enemy enemy in combat.Alive) {
            EnemyButtonVisualInstantiate(enemy, enemyContainer);
        }

        //switch (state) {
        //    case Combat.States.AttackPlay:
        //    case Combat.States.RangedPlay:
        //    case Combat.States.BlockPlay:
        //    case Combat.States.AssignDamage:
        //        List<Enemy> nonTargets = combat.Alive.Where(e => !combat.Targets.Contains(e)).ToList();
        //        if (nonTargets.Count > 0) {
        //            aliveTab.gameObject.SetActive(true);
        //            foreach (Enemy enemy in nonTargets) {
        //                EnemySmallVisualInstantiate(enemy, aliveContainer);
        //            }
        //        }

        //        foreach (Enemy enemy in combat.Targets) {
        //            EnemyButtonVisualInstantiate(enemy, enemyContainer);
        //        }
        //        break;
        //    default:
        //        foreach (Enemy enemy in combat.Alive) {
        //            EnemyButtonVisualInstantiate(enemy, enemyContainer);
        //        }
        //        break;
        //}
    }

    private EnemyVisual EnemySmallVisualInstantiate(Enemy enemy, Transform container) {
        EnemyVisual enemyVisual = Instantiate(enemySmallVisualTemplate, container).GetComponent<EnemyVisual>();
        enemyVisual.Init(enemy.EnemySO);
        enemyVisual.gameObject.SetActive(true);

        return enemyVisual;
    }

    private EnemyButtonVisual EnemyButtonVisualInstantiate(Enemy enemy, Transform container) {
        EnemyButtonVisual enemyVisual = Instantiate(enemyButtonVisualTemplate, container).GetComponent<EnemyButtonVisual>();
        enemyVisual.Init(state, enemy);
        enemyVisual.gameObject.SetActive(true);
        enemyVisual.OnEnemyButtonClick += EnemyButtonVisual_OnButtonClick;

        enemyVisuals.Add(enemyVisual);

        return enemyVisual;
    }

    //private void UpdateInfoText() {
    //    switch (state) {
    //        case Combat.States.AttackPlay:
    //        case Combat.States.RangedPlay:
    //            string attackText = "Enemy armor: " + combat.CalculateEnemyArmor();
    //            attackText += "\nCurrent attack: " + combat.CalculateDamage(state == Combat.States.RangedPlay);
    //            infoText.SetText(attackText);
    //            break;
    //        case Combat.States.BlockPlay:
    //            string blockText = "Blocking: " + combat.CalculateEnemyAttack();
    //            blockText += "\nCurrent block: " + combat.CalculateBlock();
    //            infoText.SetText(blockText);
    //            break;
    //        case Combat.States.AssignDamage:
    //            infoText.SetText("Damage Remaining: " + combat.DamageToAssign);
    //            break;
    //        default:
    //            infoText.SetText("");
    //            break;
    //    }
    //}

    //private void SelectTargets() {
    //    CombatAction.TargetsSelectedClick(this, GetTargets());
    //}

    private List<Enemy> GetTargets() {
        var targets = new List<Enemy>();
        foreach (EnemyButtonVisual visual in enemyVisuals) {
            if (visual.Selected) {
                targets.Add(visual.Enemy);
            }
        }
        return targets;
    }

    private void EnemyButtonVisual_OnButtonClick(EnemyButtonVisual visual, Enemy enemy, EnemyAttack attack) {
        if (selectedEnemy != null) {
            selectedEnemy.Deselect();
        }

        selectedEnemy = visual;
        selectedEnemy.Select();

        CombatAction.CombatSelectTargetClick(this, enemy, attack);

        //switch (state) {
        //    case Combat.States.BlockStart:
        //    case Combat.States.AssignStart:
        //        CombatAction.AttackSelectedClick(this, enemy, attack);
        //        CombatAction.CombatNextStateClick(this);
        //        break;
        //    default:
        //        UpdateUI();
        //        break;
        //}
    }

    private void Combat_OnCombatStateEnter(object sender, Combat.OnCombatStateEnterArgs e) {
        SetState(e.State);
    }

    private void Combat_OnGenerateCombatResult(object sender, Combat.OnCombatResultArgs e) {
        DrawResult(e.result);
    }

    private void Combat_OnCombatDamageAssign(object sender, EventArgs e) {
        DrawEnemies();
        UpdateUI();
    }

    private void CardManager_OnPlayCard(object sender, CardManager.OnPlayCardArgs e) {
        //UpdateInfoText();
    }
}

public static class CombatAction {
    public static event EventHandler OnCombatNextStateClick;

    public static event EventHandler<OnCombatSelectTargetClickArgs> OnCombatSelectTargetClick;
    public class OnCombatSelectTargetClickArgs : EventArgs {
        public Enemy Enemy;
        public EnemyAttack Attack;
    }

    public static void ResetStaticData() {
        OnCombatNextStateClick = null;
        OnCombatSelectTargetClick = null;
    }

    public static void CombatNextStateClick(object sender) {
        OnCombatNextStateClick?.Invoke(sender, EventArgs.Empty);
    }

    public static void CombatSelectTargetClick(object sender, Enemy enemy, EnemyAttack attack) {
        OnCombatSelectTargetClick?.Invoke(sender, new OnCombatSelectTargetClickArgs { Enemy = enemy, Attack = attack });
    }
}