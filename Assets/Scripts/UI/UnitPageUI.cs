using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPageUI : MonoBehaviour {
    [SerializeField] private Transform unitCardContainer;
    [SerializeField] private GameObject unitCardVisualPrefab;

    private void OnEnable() {
        UpdateUI();
    }

    public void UpdateUI() {
        ResetUI();
        foreach (UnitCard unit in UnitManager.Instance.UnitOffer) {
            CardVisual unitCardVisual = Instantiate(unitCardVisualPrefab, unitCardContainer).GetComponent<CardVisual>();
            unitCardVisual.Init(unit);
        }
    }

    private void ResetUI() {
        foreach (Transform child in unitCardContainer) {
            Destroy(child.gameObject);
        }
    }
}
