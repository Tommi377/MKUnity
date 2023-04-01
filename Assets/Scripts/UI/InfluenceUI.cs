using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfluenceUI : MonoBehaviour {
    [SerializeField] private ExpandingButtonUI buttonContainer;
    [SerializeField] private TMP_Text influenceText;

    private void OnEnable() {
        Player.OnPlayerInfluenceUpdate += Player_OnPlayerInfluenceUpdate;
        UnitManager.Instance.OnUnitRecruit += UnitManager_OnUnitRecruit;

        UpdateUI();
    }

    private void OnDisable() {
        Player.OnPlayerInfluenceUpdate -= Player_OnPlayerInfluenceUpdate;
        UnitManager.Instance.OnUnitRecruit -= UnitManager_OnUnitRecruit;
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
        foreach (UnitCard unitCard in GetUnitOffer()) {
            string text = "Recruit\n" + unitCard.Name + "\nCost: " + unitCard.Influence;
            buttonContainer.AddButton(
                text, () => ButtonInputManager.Instance.RecruitUnitClick(unitCard),
                new ExpandingButtonUI.Options() { Interactable = unitCard.Influence <= influence }
            );
        }

        buttonContainer.AddButton("End\nInfluence\nPhase", () => ButtonInputManager.Instance.EndInfluencePhaseClick());

        influenceText.SetText("Influence: " + influence);
    }

    private List<InfluenceAction> GetStructureActions() {
        if (GameManager.Instance.CurrentPlayer.TryGetHex(out Hex hex) && hex.ContainsStructure()) {
            return hex.Structure.InfluenceChoices(GameManager.Instance.CurrentPlayer);
        }
        return new List<InfluenceAction>();
    }

    private List<UnitCard> GetUnitOffer() {
        if (GameManager.Instance.CurrentPlayer.TryGetHex(out Hex hex)) {
            return UnitManager.Instance.GetUnitOfferForStructure(hex.StructureType);
        }
        return new List<UnitCard>();
    }

    private void Player_OnPlayerInfluenceUpdate(object sender, Player.PlayerIntEventArgs e) {
        UpdateUI();
    }

    private void UnitManager_OnUnitRecruit(object sender, UnitManager.OnUnitRecruitArgs e) {
        UpdateUI();
    }
}
