//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//public class InventoryUI : MonoBehaviour {
//    [SerializeField] private GameObject crystalCounterPrefab;
//    [SerializeField] private GameObject manaTokenPrefab;

//    [SerializeField] private Transform crystalContainer;
//    [SerializeField] private Transform manaContainer;

//    private List<CrystalCounterVisual> crystalCounterVisuals = new List<CrystalCounterVisual>();
//    private List<ManaTokenVisual> manaTokenVisuals = new List<ManaTokenVisual>();

//#nullable enable
//    private ISelectableUI? selectedElement;
//#nullable disable

//    private void Start() {
//        if (GameManager.Instance.DoneInitializing) {
//            Init();
//        } else {
//            GameManager.Instance.OnGameManagerDoneInitializing += GameManager_OnGameManagerDoneInitializing;
//        }

//        Inventory.OnInventoryUpdate += Inventory_OnInventoryUpdate;
//    }

//    private void Init() {
//        foreach (Mana.Types type in Enum.GetValues(typeof(Mana.Types)).Cast<Mana.Types>()) {
//            CrystalCounterVisual crystalCounterUI = Instantiate(crystalCounterPrefab, crystalContainer).GetComponent<CrystalCounterVisual>();
//            crystalCounterUI.Init(type);
//            crystalCounterVisuals.Add(crystalCounterUI);
//        }

//        UpdateUI(GameManager.Instance.CurrentPlayer.GetInventory());
//    }

//    private void UpdateUI(Inventory inventory) {
//        // TODO: optimize it so it doesnt redraw everything every time
//        foreach (Transform child in manaContainer) {
//            Destroy(child.gameObject);
//        }
//        manaTokenVisuals.Clear();
//        foreach (Mana mana in inventory.GetTokenList()) {
//            ManaTokenVisual manaTokenVisual = Instantiate(manaTokenPrefab, manaContainer).GetComponent<ManaTokenVisual>();
//            manaTokenVisual.Init(mana);
//            manaTokenVisuals.Add(manaTokenVisual);
//        }

//        if (ManaManager.Instance.SelectedMana != null) {
//            SelectMana(ManaManager.Instance.SelectedMana);
//        }
//    }

//    private void SelectMana(Mana mana) {
//        DeselectMana();
//        if (mana.Crystal) {
//            foreach (CrystalCounterVisual crystalCounterVisual in crystalCounterVisuals) {
//                if (crystalCounterVisual.Type == mana.Type) {
//                    crystalCounterVisual.Select();
//                    selectedElement = crystalCounterVisual;
//                    return;
//                }
//            }
//        } else {
//            foreach (ManaTokenVisual manaTokenVisual in manaTokenVisuals) {
//                if (manaTokenVisual.Mana == mana) {
//                    manaTokenVisual.Select();
//                    selectedElement = manaTokenVisual;
//                    return;
//                }
//            }
//        }
//    }

//    private void DeselectMana() {
//        if (selectedElement != null) {
//            selectedElement.Deselect();
//            selectedElement = null;
//        }
//    }

//    private void GameManager_OnGameManagerDoneInitializing(object sender, EventArgs e) {
//        Init();
//    }

//    private void Inventory_OnInventoryUpdate(object sender, Inventory.OnInventoryUpdateArgs e) {
//        UpdateUI(e.inventory);
//    }
//}
