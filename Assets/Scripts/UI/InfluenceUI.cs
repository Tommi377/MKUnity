using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfluenceUI : MonoBehaviour {
    [SerializeField] private ExpandingButtonUI buttonContainer;
    [SerializeField] private TMP_Text influenceText;

    public void Initialize() {
        Player.OnPlayerInfluenceUpdate += Player_OnPlayerInfluenceUpdate;

        RoundManager.Instance.OnPhaseChange += RoundManager_OnPhaseChange;
    }

    private void UpdateUI() {
        buttonContainer.ClearButtons();
        int influence = GameManager.Instance.CurrentPlayer.Influence + GameManager.Instance.CurrentPlayer.ReputationBonus;

        foreach (InfluenceAction action in GetStructureActions()) {
            buttonContainer.AddButton(action.Name + "\nCost: " + action.Cost, () => ButtonInputManager.Instance.InfluenceChoiceClick(action), action.Cost <= influence);
        }
        foreach (UnitCard unitCard in GetUnitOffer()) {
            string text = "Recruit\n" + unitCard.Name + "\nCost: " + unitCard.Influence;
            buttonContainer.AddButton(text, () => ButtonInputManager.Instance.RecruitUnitClick(unitCard), unitCard.Influence <= influence);
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

    private void Player_OnPlayerInfluenceUpdate(object sender, Player.IntEventArgs e) {
        UpdateUI();
    }

    private void RoundManager_OnPhaseChange(object sender, RoundManager.OnPhaseChangeArgs e) {
        if (e.actionType == ActionTypes.Influence) {
            UpdateUI();
        }
    }
}
