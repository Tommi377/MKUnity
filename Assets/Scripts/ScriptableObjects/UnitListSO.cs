using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "UnitList", menuName = "Cards/UnitListSO")]
public class UnitListSO : ScriptableObject {
    public bool Elite;
    public List<UnitCardCount> List;
}

[Serializable]
public struct UnitCardCount {
    public UnitCardSO Unit;
    public int Count;
}