using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfluenceUI : MonoBehaviour {
    [SerializeField] private ExpandingButtonUI buttonContainer;

    public void Initialize() {
        RoundManager.Instance.OnPhaseChange += RoundManager_OnPhaseChange;
    }

    private void UpdateUI() {
        foreach (InfluenceAction action in GetStructureActions()) {
            buttonContainer.AddButton(action.Name + "\nCost: " + action.Cost, () => { });
        }
        foreach (UnitCard unitCard in GetUnitOffer()) {
            string text = "Recruit\n" + unitCard.Name + "\nCost: " + unitCard.Influence;
            buttonContainer.AddButton(text, () => { });
        }

        buttonContainer.AddButton("End\nInfluence\nPhase", () => ButtonInputManager.Instance.EndInfluencePhaseClick());
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

    private void RoundManager_OnPhaseChange(object sender, RoundManager.OnPhaseChangeArgs e) {
        if (e.actionType == ActionTypes.Influence) {
            UpdateUI();
        }
    }
}
