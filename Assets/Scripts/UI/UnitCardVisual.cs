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

    private UnitCard unitCard;

    private void OnEnable() {
        UnitCard.OnUnitExhaustChanged += UnitCard_OnUnitExhaustChanged;
    }

    private void OnDisable() {
        UnitCard.OnUnitExhaustChanged -= UnitCard_OnUnitExhaustChanged;
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
    }

    public void SetExhaustedStatus() => SetExhaustedStatus(unitCard.Exhausted);
    public void SetExhaustedStatus(bool status) {
        if (unitCard != null) {
            exhaustedText.gameObject.SetActive(status);
        }
    }

    private void UnitCard_OnUnitExhaustChanged(object sender, UnitCard.OnUnitExhaustChangedArgs e) {
        if (unitCard != null && e.Card == unitCard) {
            SetExhaustedStatus(e.Exhausted);
        }
    }
}
