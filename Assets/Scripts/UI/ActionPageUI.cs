using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ActionPageUI : MonoBehaviour {
    [SerializeField] private Transform actionCardContainer;
    [SerializeField] private GameObject cardTemplate;

    private bool levelUpMode = false;

    private void OnEnable() {
        UpdateUI();

        SupplyManager.Instance.OnSupplyUpdate += SupplyManager_OnSupplyUpdate;
        RoundManager.Instance.OnRoundStateExit += RoundManager_OnRoundStateExit;
    }

    private void OnDisable() {
        SupplyManager.Instance.OnSupplyUpdate -= SupplyManager_OnSupplyUpdate;
        RoundManager.Instance.OnRoundStateExit -= RoundManager_OnRoundStateExit;
    }

    public void UpdateUI() {
        ResetUI();
        Debug.Log("Level up " + levelUpMode);
        for (int i = 0; i < SupplyManager.Instance.AdvancedActionOffer.Count; i++) {
            int index = i;
            Card advancedAction = SupplyManager.Instance.AdvancedActionOffer[i];

            GameObject go = Instantiate(cardTemplate, actionCardContainer);
            CardVisual advancedActionVisual = go.GetComponentInChildren<CardVisual>();
            advancedActionVisual.Init(advancedAction);

            if (levelUpMode) {
                Button button = go.GetComponentInChildren<Button>(true);
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SupplyAction.AdvancedActionChoose(this, index));
                button.gameObject.SetActive(true);
            }

            go.SetActive(true);
        }
    }

    public void LevelUpMode(bool mode) {
        levelUpMode = mode;
        UpdateUI();
    }

    private void ResetUI() {
        foreach (Transform child in actionCardContainer) {
            Destroy(child.gameObject);
        }
    }

    private void SupplyManager_OnSupplyUpdate(object sender, System.EventArgs e) {
        UpdateUI();
    }

    private void RoundManager_OnRoundStateExit(object sender, RoundManager.RoundStateArgs e) {
        if (e.State == RoundManager.States.LevelUp) {
            LevelUpMode(false);
        }
    }
}
