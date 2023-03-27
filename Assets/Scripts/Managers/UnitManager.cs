using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitManager : MonoBehaviour {
    public static UnitManager Instance;

    [SerializeField] private UnitListSO unitListNormalSO;
    [SerializeField] private UnitListSO unitListEliteSO;

    [SerializeField] private int unitOfferLimit = 3;

    /* EVENT DEFINITIONS - START */
    public event EventHandler<OnUnitRecruitArgs> OnUnitRecruit;
    public class OnUnitRecruitArgs : EventArgs {
        public UnitCard unit;
    }
    /* EVENT DEFINITIONS - END */

    private Stack<UnitCard> unitCardNormalStack;
    private Stack<UnitCard> unitCardEliteStack;

    public List<UnitCard> UnitOffer { get; private set; } = new List<UnitCard>();

    private void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogError("More than one instance of a singleton");
        } else {
            Instance = this;
        }

        List<UnitCard> unitsNormal = new List<UnitCard>();
        List<UnitCard> unitsElite = new List<UnitCard>();

        foreach (UnitCardCount cardCount in unitListNormalSO.List) {
            for (int i = 0; i < cardCount.Count; i++) {
                unitsNormal.Add(cardCount.Unit.CreateInstance() as UnitCard);
            }
        }
        foreach (UnitCardCount cardCount in unitListEliteSO.List) {
            for (int i = 0; i < cardCount.Count; i++) {
                unitsElite.Add(cardCount.Unit.CreateInstance() as UnitCard);
            }
        }
        unitCardNormalStack = new Stack<UnitCard>(unitsNormal.OrderBy(x => UnityEngine.Random.value).ToList());
        unitCardEliteStack = new Stack<UnitCard>(unitsElite.OrderBy(x => UnityEngine.Random.value).ToList());

        for (int i = 0; i < unitOfferLimit; i++) {
            UnitOffer.Add(unitCardNormalStack.Pop());
        }
    }

    public List<UnitCard> GetUnitOfferForStructure(StructureTypes structureType) => UnitOffer.Where(unit => unit.Locations.Contains(structureType)).ToList();

    public void RecruitUnit(UnitCard unit) {
        if (!UnitOffer.Contains(unit)) {
            Debug.LogError("Unit does not exist in the unit offer");
            return;
        }

        UnitOffer.Remove(unit);
        OnUnitRecruit?.Invoke(this, new OnUnitRecruitArgs { unit = unit });
    } 

    // TODO new round init stuff
}
