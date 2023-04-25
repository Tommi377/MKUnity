using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfluenceUI : MonoBehaviour {
    [SerializeField] private ExpandingButtonUI buttonContainer;
    [SerializeField] private TMP_Text influenceText;

    private void OnEnable() {
        Player.OnPlayerInfluenceUpdate += Player_OnPlayerInfluenceUpdate;
        ItemManager.Instance.OnItemBuy += UnitManager_OnUnitRecruit;

        UpdateUI();
    }

    private void OnDisable() {
        Player.OnPlayerInfluenceUpdate -= Player_OnPlayerInfluenceUpdate;
        ItemManager.Instance.OnItemBuy -= UnitManager_OnUnitRecruit;
    }

    private void UpdateUI() {
        buttonContainer.ClearButtons();
        int influence = GameManager.Instance.CurrentPlayer.Influence + GameManager.Instance.CurrentPlayer.ReputationBonus;

        foreach (InfluenceAction action in GetStructureActions()) {
            buttonContainer.AddButton(
                action.Name + "\nCost: " + action.Cost,
                () => ButtonInputManager.Instance.InfluenceChoiceClick(action),
                new ExpandingButtonUI.Options() { Interactable = action.Cost <= influence }
            );
        }
        foreach (ItemCard itemCard in GetItemOffer()) {
            string text = "Buy\n" + itemCard.Name + "\nCost: " + itemCard.Cost;
            buttonContainer.AddButton(
                text, () => ButtonInputManager.Instance.BuyItemClick(itemCard),
                new ExpandingButtonUI.Options() { Interactable = itemCard.Cost <= influence }
            );
        }

        buttonContainer.AddButton("End\nInfluence\nPhase", () => RoundAction.RoundNextStateClick(this));

        influenceText.SetText("Influence: " + influence);
    }

    private List<InfluenceAction> GetStructureActions() {
        if (GameManager.Instance.CurrentPlayer.TryGetHex(out Hex hex) && hex.ContainsStructure()) {
            return hex.Structure.InfluenceChoices(GameManager.Instance.CurrentPlayer);
        }
        return new List<InfluenceAction>();
    }

    private List<ItemCard> GetItemOffer() {
        if (GameManager.Instance.CurrentPlayer.TryGetHex(out Hex hex)) {
            return ItemManager.Instance.GetItemOfferForStructure(hex.StructureType);
        }
        return new List<ItemCard>();
    }

    private void Player_OnPlayerInfluenceUpdate(object sender, Player.PlayerIntEventArgs e) {
        UpdateUI();
    }

    private void UnitManager_OnUnitRecruit(object sender, ItemManager.OnItemBuyArgs e) {
        UpdateUI();
    }
}