using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitCard", menuName = "Cards/UnitCard")]
public class UnitCardSO : CardSO {
    public int Level;
    public int Influence;
    public int Armor;
    public List<CombatElements> resistances;
}