using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyButtonVisual : MonoBehaviour {
    [SerializeField] private EnemyVisual enemyVisual;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Transform backgroundHighlight;
    [SerializeField] private Transform backgroundDead;
    [SerializeField] private ExpandingButtonUI buttonContainer;

    public bool Dead { get; private set; } = false;
    public bool Selected { get; private set; } = false;
    public bool Forced { get; private set; } = false;

    public Enemy Enemy { get; private set; }

    public event Action<Enemy, int> OnButtonClick;

    public void Init(Combat.States state, Enemy enemy, bool disableName = false) {
        Enemy = enemy;

        if (!disableName) {
            nameText.SetText(enemy.Name);
            nameText.gameObject.SetActive(true);
        }

        enemyVisual.Init(enemy.EnemySO);
        UpdateUI(state);
    }

    public void UpdateUI(Combat.States state) {
        buttonContainer.ClearButtons();

        if (Dead) {
            return;
        }

        switch (state) {
            case Combat.States.Start:
                Select();
                if (!Forced) {
                    buttonContainer.AddButton("Deselect", (btn) => {
                        ToggleSelect();
                        buttonContainer.SetText(btn, Selected ? "Select" : "Select");
                    });
                }
                break;
            case Combat.States.AttackStart:
            case Combat.States.RangedStart:
                buttonContainer.AddButton("Select", (btn) => {
                    ToggleSelect();
                    buttonContainer.SetText(btn, Selected ? "Deselect" : "Select");
                });
                break;
            case Combat.States.BlockStart:
            case Combat.States.AssignStart:
                if (GameManager.Instance.Combat.UnassignedAttacks.TryGetValue(Enemy, out List<EnemyAttack> attacks)) {
                    for (int i = 0; i < Enemy.Attacks.Count; i++) {
                        int choiceIndex = i;
                        EnemyAttack attack = Enemy.Attacks[i];
                        var options = new ExpandingButtonUI.Options() { Interactable = attacks.Contains(attack) };

                        buttonContainer.AddButton(attack.ToString(), () => OnButtonClick?.Invoke(Enemy, choiceIndex), options);
                    }
                }
                break;
            default:
                break;
        }
    }

    public void ToggleSelect() {
        if (Selected) {
            Deselect();
        } else {
            Select();
        }

        OnButtonClick?.Invoke(Enemy, 0);
    }

    public void SetDead() {
        Dead = true;
        backgroundDead.gameObject.SetActive(true);
    }

    public void SetForced() {
        Forced = true;
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
        OnButtonClick = null;
        Destroy(gameObject);
    }
}
