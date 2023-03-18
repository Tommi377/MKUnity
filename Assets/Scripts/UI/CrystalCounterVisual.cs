using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrystalCounterVisual : MonoBehaviour {
    [SerializeField] private TMP_Text countText;

    private ManaSource.Types type;

    private void Start() {
        Player.OnInventoryUpdate += Player_OnInventoryUpdate;
    }

    public void Init(ManaSource.Types type) {
        this.type = type;
        Inventory inventory = GameManager.Instance.CurrentPlayer.GetInventory();
        UpdateUI(inventory);

        GetComponent<Image>().color = ManaSource.GetColor(type);
    }

    private void UpdateUI(Inventory inventory) {
        int count = inventory.GetCrystalCount(type);
        countText.SetText(count.ToString());
    }

    private void Player_OnInventoryUpdate(object sender, Player.OnInventoryUpdateArgs e) {
        UpdateUI(e.inventory);
    }
}
