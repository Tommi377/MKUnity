using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrystalCounterVisual : MonoBehaviour, ISelectableUI {
    [SerializeField] private Transform visual;
    [SerializeField] private Transform highlight;
    [SerializeField] private TMP_Text countText;

    public Mana.Types Type { get; private set; }

    private void Start() {
        Inventory.OnInventoryUpdate += Inventory_OnInventoryUpdate;
    }

    public void Init(Mana.Types type) {
        Type = type;
        Inventory inventory = GameManager.Instance.CurrentPlayer.GetInventory();

        UpdateUI(inventory);
        DrawVisual();
    }

    private void UpdateUI(Inventory inventory) {
        int count = inventory.GetCrystalCount(Type);
        countText.SetText(count.ToString());
    }

    public void DrawVisual() {
        Image image = visual.GetComponent<Image>();
        image.color = Mana.GetColor(Type);
    }

    public void Select() {
        highlight.gameObject.SetActive(true);
    }

    public void Deselect() {
        highlight.gameObject.SetActive(false);
    }

    private void Inventory_OnInventoryUpdate(object sender, Inventory.OnInventoryUpdateArgs e) {
        UpdateUI(e.inventory);
    }
}
