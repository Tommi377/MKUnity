using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyButtonVisual : MonoBehaviour {
    [SerializeField] private EnemyVisual enemyVisual;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Transform backgroundHighlight;
    [SerializeField] private Transform backgroundDead;
    [SerializeField] private ExpandingButtonUI buttonContainer;

    public bool Dead { get; private set; } = false;
    public bool Selected { get; private set; } = false;

    public Enemy Enemy { get; private set; }

    public event Action<int> OnButtonClick;

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
            case Combat.States.AttackStart:
            case Combat.States.RangedStart:
                buttonContainer.AddButton("Select", (btn) => {
                    ToggleSelect();
                    buttonContainer.SetText(btn, Selected ? "Deselect" : "Select");
                });
                break;
            case Combat.States.BlockStart:
            case Combat.States.AssignStart:
                for (int i = 0; i < Enemy.Attacks.Count; i++) {
                    int choiceIndex = i;
                    EnemyAttack attack = Enemy.Attacks[i];
                    buttonContainer.AddButton(attack.ToString(), () => {
                        OnButtonClick?.Invoke(choiceIndex);
                    });
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

        OnButtonClick?.Invoke(0);
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
        OnButtonClick = null;
        Destroy(gameObject);
    }
}
