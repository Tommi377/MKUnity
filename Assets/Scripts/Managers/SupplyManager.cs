using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SupplyManager : MonoBehaviour {
    public static SupplyManager Instance;

    [SerializeField] private CardListSO advancedActionListSO;

    public event EventHandler OnSupplyUpdate;

    private Stack<Card> advancedActionStack = new Stack<Card>();

    public List<Card> AdvancedActionOffer = new List<Card>();

    private void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogError("More than one instance of a singleton");
        } else {
            Instance = this;
        }

        IEnumerable<Card> advancedActions = advancedActionListSO.cards.OrderBy(x => UnityEngine.Random.value).Select(cardSO => cardSO.CreateInstance());

        advancedActionStack = new Stack<Card>(advancedActions);

        SupplyOffer();
    }

    public bool GainAdvancedAction(Card card) {
        if (AdvancedActionOffer.Contains(card)) {
            AdvancedActionOffer.Remove(card);
            SupplyOffer();
            return true;
        }
        return false;
    }

    private void SupplyOffer() {
        int advancedActionOfferLimit = 3;
        bool updateNeed = AdvancedActionOffer.Count < advancedActionOfferLimit;
        while (AdvancedActionOffer.Count < advancedActionOfferLimit) {
            if (advancedActionStack.Count > 0) {
                AdvancedActionOffer.Add(advancedActionStack.Pop());
            }
        }

        if (updateNeed) {
            OnSupplyUpdate?.Invoke(this, EventArgs.Empty);
        }
    }
}
