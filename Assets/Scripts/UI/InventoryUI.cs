using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour {
    [SerializeField] private TMP_Text crystalCountText;

    private void Start() {
        if (GameManager.Instance.DoneInitializing) {
            Init();
        } else {
            GameManager.Instance.OnGameManagerDoneInitializing += GameManager_OnGameManagerDoneInitializing;
        }

        Inventory.OnInventoryUpdate += Inventory_OnInventoryUpdate;
    }

    private void Init() {
        UpdateUI(GameManager.Instance.CurrentPlayer.GetInventory());
    }

    private void UpdateUI(Inventory inventory) {
        crystalCountText.SetText(inventory.GetCrystalCount().ToString());
    }

    private void GameManager_OnGameManagerDoneInitializing(object sender, EventArgs e) {
        Init();
    }

    private void Inventory_OnInventoryUpdate(object sender, Inventory.OnInventoryUpdateArgs e) {
        UpdateUI(e.inventory);
    }
}
