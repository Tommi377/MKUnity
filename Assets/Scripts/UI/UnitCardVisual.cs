using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UnitCardVisual : MonoBehaviour {
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text influenceText;
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text exhaustedText;
    [SerializeField] private Transform abilityContainer;
    [SerializeField] private GameObject abilityTemplate;

    [SerializeField] private Transform woundContainer;
    [SerializeField] private GameObject woundTemplate;

    private UnitCard unitCard;

    private void OnDisable() {
        if (unitCard != null) {
            unitCard.OnUnitExhaustChanged -= UnitCard_OnUnitExhaustChanged;
            unitCard.OnUnitWoundChanged -= UnitCard_OnUnitWoundChanged;
        }
    }

    public void Init(UnitCard unitCard) {
        this.unitCard = unitCard;
        nameText.SetText(unitCard.Name);
        influenceText.SetText(unitCard.Influence.ToString());
        armorText.SetText(unitCard.Armor.ToString());
        foreach (CardChoice choice in unitCard.UnitCardSO.Choices) {
            TMP_Text text = Instantiate(abilityTemplate, abilityContainer).GetComponent<TMP_Text>();
            text.SetText(choice.Description);
            if (choice.ManaTypes.Any()) {
                text.color = Mana.GetColor(choice.ManaTypes[0]);
            }
            text.gameObject.SetActive(true);
        }
        SetExhaustedStatus();

        unitCard.OnUnitExhaustChanged += UnitCard_OnUnitExhaustChanged;
        unitCard.OnUnitWoundChanged += UnitCard_OnUnitWoundChanged;
    }

    public void SetExhaustedStatus() => SetExhaustedStatus(unitCard.Exhausted);
    public void SetExhaustedStatus(bool status) {
        if (unitCard != null) {
            exhaustedText.gameObject.SetActive(status);
        }
    }

    public void SetWounds() {
        foreach (Transform child in woundContainer) {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < unitCard.Wounds; i++) {
            Instantiate(woundTemplate, woundContainer);
        }
    }

    private void UnitCard_OnUnitExhaustChanged(object sender, System.EventArgs e) {
        if (unitCard != null) {
            SetExhaustedStatus(unitCard.Exhausted);
        }
    }

    private void UnitCard_OnUnitWoundChanged(object sender, System.EventArgs e) {
        if (unitCard != null) {
            SetWounds();
        }
    }
}
