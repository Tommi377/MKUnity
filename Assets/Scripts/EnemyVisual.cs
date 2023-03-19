using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Enemy))]
public class EnemyVisual : MonoBehaviour {
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text fameText;
    [SerializeField] private Transform resistanceContainer;
    [SerializeField] private Transform attackContainer;
    [SerializeField] private Transform abilityContainer;
    [SerializeField] private GameObject abilityVisualPrefab;

    private Enemy enemy;

    private void Awake() {
        enemy = GetComponent<Enemy>();
    }

    private void Start() {
        if (enemy.EnemySO) {
            Init();
        } else {
            enemy.OnInit += Enemy_OnInit;
        }
    }

    public void Init() {
        gameObject.name = enemy.Name;
        image.sprite = enemy.sprite;
        armorText.SetText(enemy.Armor.ToString());
        fameText.SetText(enemy.Fame.ToString());

        foreach (CombatElements resistance in enemy.Resistances) {
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

        foreach (EnemyAttack attack in enemy.Attacks) {
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


        foreach (EnemyAbilities ability in enemy.Abilities) {
            AbilityVisual abilityVisual = Instantiate(abilityVisualPrefab, abilityContainer).GetComponent<AbilityVisual>();
            abilityVisual.SetText(ability.ToString()[..2]); // TODO: Make ability visual
        }
    }

    private void Enemy_OnInit(object sender, System.EventArgs e) {
        Init();
    }
}
