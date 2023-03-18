using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryUI : MonoBehaviour {
    [SerializeField] private GameObject crystalCounterPrefab;
    [SerializeField] private GameObject manaTokenPrefab;

    [SerializeField] private Transform crystalContainer;
    [SerializeField] private Transform manaContainer;

    private void Start() {
        if (GameManager.Instance.DoneInitializing) {
            Init();
        } else {
            GameManager.Instance.OnGameManagerDoneInitializing += GameManager_OnGameManagerDoneInitializing;
        }

        Player.OnInventoryUpdate += Player_OnInventoryUpdate;
    }

    private void Init() {
        foreach (ManaSource.Types type in Enum.GetValues(typeof(ManaSource.Types)).Cast<ManaSource.Types>()) {
            CrystalCounterVisual crystalCounterUI = Instantiate(crystalCounterPrefab, crystalContainer).GetComponent<CrystalCounterVisual>();
            crystalCounterUI.Init(type);
        }

        UpdateUI(GameManager.Instance.CurrentPlayer.GetInventory());
    }

    private void UpdateUI(Inventory inventory) {
        // TODO: optimize it so it doesnt redraw everything every time
        foreach (Transform child in manaContainer) {
            Destroy(child.gameObject);
        }
        foreach (ManaSource.Types type in inventory.GetManaList()) {
            ManaTokenVisual manaTokenVisual = Instantiate(manaTokenPrefab, manaContainer).GetComponent<ManaTokenVisual>();
            manaTokenVisual.Init(type);
        }
    }

    private void GameManager_OnGameManagerDoneInitializing(object sender, EventArgs e) {
        Init();
    }

    private void Player_OnInventoryUpdate(object sender, Player.OnInventoryUpdateArgs e) {
        UpdateUI(e.inventory);
    }
}
