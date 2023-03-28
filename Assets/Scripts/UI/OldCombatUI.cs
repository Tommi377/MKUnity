using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OldCombatUI : MonoBehaviour {
    [SerializeField] private ExpandingButtonUI buttonContainer;
    [SerializeField] private ExpandingButtonUI subPanelUI;
    [SerializeField] private TMP_Text combatPhaseText;

    private State state;
    public enum State {
        SelectEnemies,
        Attack,
        Block,
        AssignDamage
    }

    private List<Enemy> chosenEnemies = new List<Enemy>();

    public void Initialize() {
        Combat.OnCombatPhaseChange += Combat_OnCombatPhaseChange;
        Combat.OnCombatAttack += Combat_OnCombatAttack;
        Combat.OnCombatBlock += Combat_OnCombatBlock;
        Combat.OnCombatAssign += Combat_OnCombatAssign;
    }

    private void UpdateUI(Combat combat, State state) {
        this.state = state;
        UpdateUI(combat);
    }

    private void UpdateUI(Combat combat) {
        //buttonContainer.ClearButtons();

        //if (combat == null) {
        //    Debug.LogError("Updating CombatUI without combat");
        //    return;
        //}

        //switch (state) {
        //    case State.SelectEnemies:
        //        string endPhaseText = GameManager.Instance.Combat.CombatPhase == CombatPhases.Attack ? "End\nCombat\n" : "To the\nBlock\nPhase";

        //        buttonContainer.AddButton("Finish\nSelecting\nEnemies", () => UpdateUI(combat, State.Attack));
        //        buttonContainer.AddButton(endPhaseText, () => NextPhaseClick());

        //        subPanelUI.ClearButtons();
        //        foreach (Enemy enemy in combat.Enemies) {
        //            subPanelUI.AddButton(enemy.name, () => ToggleEnemy(combat, enemy));
        //        }
        //        break;
        //    case State.Attack:
        //        buttonContainer.AddButton("Attack!", () => {
        //            Hide();
        //            ButtonInputManager.Instance.CombatEnemyChooseClick(chosenEnemies);
        //            chosenEnemies.Clear();
        //        });
        //        break;
        //    case State.Block:
        //        foreach (Enemy enemy in combat.UnassignedAttacks) {
        //            buttonContainer.AddButton(enemy.name, () => {
        //                Hide();
        //                BlockEnemyClick(enemy);
        //            });
        //        }
        //        buttonContainer.AddButton("To the\nDamage\nPhase", () => NextPhaseClick());
        //        break;
        //    case State.AssignDamage:
        //        if (combat.UnassignedAttacks.Count == 0) {
        //            buttonContainer.AddButton("To the\nAttack\nPhase", () => NextPhaseClick());
        //        } else {
        //            foreach (Enemy enemy in combat.UnassignedAttacks) {
        //                buttonContainer.AddButton(enemy.name, () => {
        //                    Hide();
        //                    AssignEnemyDamageClick(enemy);
        //                });
        //            }
        //        }
        //        break;
        //}
        //Show();
    }

    private void UpdateCombatPhaseText(CombatPhases combatPhase) {
        combatPhaseText.SetText(combatPhase + " phase");
    }

    private void ChangeCombatPhase(Combat combat, CombatPhases combatPhase) {
        UpdateCombatPhaseText(combatPhase);

        switch(combatPhase) {
            case CombatPhases.Range:
            case CombatPhases.Attack:
                UpdateUI(combat, State.SelectEnemies);
                break;
            case CombatPhases.Block:
                UpdateUI(combat, State.Block);
                break;
            case CombatPhases.AssignDamage:
                UpdateUI(combat, State.AssignDamage);
                break;
        }

    }

    private void ToggleEnemy(Combat combat, Enemy enemy) {
        if (!combat.Enemies.Contains(enemy)) {
            Debug.Log("Enemy is not in combat");
        }

        if (chosenEnemies.Contains(enemy)) {
            chosenEnemies.Remove(enemy);
        } else {
            chosenEnemies.Add(enemy);
        }
        Debug.Log("Targeted enemy count: " + chosenEnemies.Count);
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void NextPhaseClick() {
        ButtonInputManager.Instance.CombatNextPhaseClick();
    }

    private void BlockEnemyClick(Enemy enemy) {
        ButtonInputManager.Instance.CombatBlockChooseClick(enemy);
    }

    private void AssignEnemyDamageClick(Enemy enemy) {
        ButtonInputManager.Instance.AssignEnemyDamageClick(enemy);
    }

    private void Combat_OnCombatPhaseChange(object sender, Combat.OnCombatPhaseChangeArgs e) {
        ChangeCombatPhase(e.combat, e.phase);
    }

    private void Combat_OnCombatAttack(object sender, System.EventArgs e) {
        Combat combat = sender as Combat;
        ChangeCombatPhase(combat, CombatPhases.Attack);
    }

    private void Combat_OnCombatBlock(object sender, System.EventArgs e) {
        Combat combat = sender as Combat;
        ChangeCombatPhase(combat, CombatPhases.Block);
    }

    private void Combat_OnCombatAssign(object sender, System.EventArgs e) {
        Combat combat = sender as Combat;
        ChangeCombatPhase(combat, CombatPhases.AssignDamage);
    }
}
