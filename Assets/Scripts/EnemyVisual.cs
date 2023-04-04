using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyVisual : MonoBehaviour {
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text fameText;
    [SerializeField] private Transform resistanceContainer;
    [SerializeField] private Transform attackContainer;
    [SerializeField] private Transform abilityContainer;
    [SerializeField] private GameObject abilityVisualPrefab;

    [SerializeField] private EnemySO enemySO;

    private void Start() {
        if (TryGetComponent(out Enemy enemy)) {
            if (enemy.EnemySO) {
                Init(enemy.EnemySO);
            } else if (enemySO != null) {
                Init(enemySO);
            } else {
                enemy.OnInit += Enemy_OnInit;
            }
        }
    }

    public void Init(EnemySO enemySO) {
        this.enemySO = enemySO;

        gameObject.name = enemySO.Name;
        image.sprite = enemySO.Sprite;
        armorText.SetText(enemySO.Armor.ToString());
        fameText.SetText(enemySO.Fame.ToString());

        foreach (CombatElements resistance in enemySO.Resistances) {
            AbilityVisual resistanceVisual = Instantiate(abilityVisualPrefab, resistanceContainer).GetComponent<AbilityVisual>();
            resistanceVisual.SetText("");
            switch (resistance) {
                case CombatElements.Ice:
                    resistanceVisual.SetBackgroundColor(new Color(0, 1, 1, 1));
                    break;
                case CombatElements.Fire:
                    resistanceVisual.SetBackgroundColor(new Color(1, .5f, 0, 1));
                    break;
            }
        }

        foreach (EnemyAttack attack in enemySO.Attacks) {
            AbilityVisual attackVisual = Instantiate(abilityVisualPrefab, attackContainer).GetComponent<AbilityVisual>();
            attackVisual.SetText(attack.Damage.ToString());
            switch(attack.Element) {
                case CombatElements.Ice:
                    attackVisual.SetBackgroundColor(new Color(0, 1, 1, 1));
                    break;
                case CombatElements.Fire:
                    attackVisual.SetBackgroundColor(new Color(1, .5f, 0, 1));
                    break;
                case CombatElements.ColdFire:
                    attackVisual.SetBackgroundColor(new Color(1, 0, 1, 1));
                    break;
            }
        }


        foreach (EnemyAbilities ability in enemySO.Abilities) {
            AbilityVisual abilityVisual = Instantiate(abilityVisualPrefab, abilityContainer).GetComponent<AbilityVisual>();
            abilityVisual.SetText(ability.ToString()[..2]); // TODO: Make ability visual
        }
    }

    private void Enemy_OnInit(object sender, System.EventArgs e) {
        Init((sender as Enemy).EnemySO);
    }
}
