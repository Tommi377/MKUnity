using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitCard", menuName = "Cards/UnitCard")]
public class UnitCardSO : CardSO {
    public int Level;
    public int Influence;
    public int Armor;
    public List<CombatElements> Resistances;
    public List<StructureTypes> Locations;

    public override Card.Types Type => Card.Types.Unit;
    public override Card CreateInstance() {
        Type type = System.Type.GetType(Name.Replace(" ", ""));
        return (UnitCard)Activator.CreateInstance(type, new object[] { this });
    }
}