using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ItemCardVisual : MonoBehaviour {
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private TMP_Text exhaustedText;
    [SerializeField] private Transform abilityContainer;
    [SerializeField] private GameObject abilityTemplate;

    [SerializeField] private Transform locationContainer;
    [SerializeField] private GameObject locationTemplate;

    private ItemCard itemCard;

    private void OnDisable() {
        if (itemCard != null) {
            itemCard.OnUnitExhaustChanged -= UnitCard_OnUnitExhaustChanged;
        }
    }

    public void Init(ItemCard itemCard) {
        this.itemCard = itemCard;
        nameText.SetText(itemCard.Name);
        typeText.SetText(itemCard.UseRate.ToString()[0].ToString());
        costText.SetText(itemCard.Cost.ToString());
        weightText.SetText(itemCard.Weight.ToString());

        // Actions
        foreach (CardChoice choice in itemCard.ItemCardSO.Choices) {
            TMP_Text text = Instantiate(abilityTemplate, abilityContainer).GetComponent<TMP_Text>();
            text.SetText(choice.Name);
            text.gameObject.SetActive(true);
        }

        // Locations
        foreach (StructureTypes location in itemCard.Locations) {
            TokenVisual visual = Instantiate(locationTemplate, locationContainer).GetComponent<TokenVisual>();
            visual.SetText(location.ToString()[0].ToString());
            visual.gameObject.SetActive(true);
        }

        SetExhaustedStatus();

        itemCard.OnUnitExhaustChanged += UnitCard_OnUnitExhaustChanged;
    }

    public void SetExhaustedStatus() => SetExhaustedStatus(itemCard.Exhausted);
    public void SetExhaustedStatus(bool status) {
        if (itemCard != null) {
            exhaustedText.gameObject.SetActive(status);
        }
    }

    private void UnitCard_OnUnitExhaustChanged(object sender, System.EventArgs e) {
        if (itemCard != null) {
            SetExhaustedStatus(itemCard.Exhausted);
        }
    }
}
