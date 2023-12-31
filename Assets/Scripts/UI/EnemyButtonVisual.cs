using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyButtonVisual : MonoBehaviour {
    [SerializeField] private EnemyVisual enemyVisual;
    [SerializeField] private EnemyVisual summonedEnemy;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Transform backgroundHighlight;
    [SerializeField] private Transform backgroundDead;
    [SerializeField] private ExpandingButtonUI buttonContainer;

    public bool Dead { get; private set; } = false;
    public bool Selected { get; private set; } = false;

    public Enemy Enemy { get; private set; }

    private Combat Combat => GameManager.Instance.Combat;

    public event Action<EnemyButtonVisual, Enemy, EnemyAttack> OnEnemyButtonClick;

    public void Init(Combat.States state, Enemy enemy, bool disableName = false) {
        if (Combat == null) {
            Debug.LogError("Can't instantiate EnemyButtonVisual without active combat");
            return;
        }

        Enemy = enemy;

        if (!disableName) {
            nameText.SetText(enemy.Name);
            nameText.gameObject.SetActive(true);
        }

        enemyVisual.Init(enemy.EnemySO);
        UpdateUI(state);

        if (Combat.SummonedEnemies.ContainsKey(Enemy)) {
            DrawSummonToken(state);
        }
    }

    public void ToggleSelect() {
        //if (Selected) {
        //    Deselect();
        //} else {
        //    Select();
        //}

        OnEnemyButtonClick?.Invoke(this, Enemy, null);
    }

    public void SetDead() {
        Dead = true;
        backgroundDead.gameObject.SetActive(true);
    }

    public void Select() {
        Selected = true;
        backgroundHighlight.gameObject.SetActive(true);
    }

    public void Deselect() {
        Selected = false;
        backgroundHighlight.gameObject.SetActive(false);
    }

    public void DestroySelf() {
        OnEnemyButtonClick = null;
        Destroy(gameObject);
    }

    private void UpdateUI(Combat.States state) {
        buttonContainer.ClearButtons();

        if (Dead) {
            return;
        }

        switch (state) {
            case Combat.States.Attack:
                buttonContainer.AddButton("Select", (btn) => {
                    ToggleSelect();
                    buttonContainer.SetText(btn, "Select");
                });
                break;
            case Combat.States.Block:
            case Combat.States.Assign:
                List<EnemyAttack> attacks = Combat.SummonedEnemies.ContainsKey(Enemy) ? Combat.SummonedEnemies[Enemy].Attacks : Enemy.Attacks;
                if (Combat.UnassignedAttacks.TryGetValue(Enemy, out Dictionary<EnemyAttack, float> unassigned)) {
                    for (int i = 0; i < attacks.Count; i++) {
                        int choiceIndex = i;
                        EnemyAttack attack = attacks[i];
                        var options = new ExpandingButtonUI.Options() { Interactable = unassigned.ContainsKey(attack) };

                        buttonContainer.AddButton(attack.ToString(), () => OnEnemyButtonClick?.Invoke(this, Enemy, attack), options);
                    }
                } else {
                    attacks.ForEach(attack => {
                        var options = new ExpandingButtonUI.Options() { Interactable = false };
                        buttonContainer.AddButton(attack.ToString(), () => { }, options);
                    });
                }
                break;
            default:
                break;
        }
    }

    private void DrawSummonToken(Combat.States state) {
        summonedEnemy.gameObject.SetActive(false);

        if (
            state == Combat.States.Block ||
            state == Combat.States.Assign
        ) {
            summonedEnemy.gameObject.SetActive(true);

            EnemySO summoned = Combat.SummonedEnemies[Enemy];
            summonedEnemy.Init(summoned);
        }
    }
}
