using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "ItemList", menuName = "Cards/ItemList")]
public class ItemListSO : ScriptableObject {
    public int Tier;
    public List<ItemCardCount> List;
}

[Serializable]
public struct ItemCardCount {
    public ItemCardSO Item;
    public int Count;
}