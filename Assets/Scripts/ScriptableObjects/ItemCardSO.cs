using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemCard", menuName = "Cards/ItemCard")]
public class ItemCardSO : CardSO {
    public int Tier;
    public int Cost;
    public int Weight;
    public List<StructureTypes> Locations;
    public ItemUseRate UseRate;

    public override Card.Types Type => Card.Types.Item;
    public override Card CreateInstance() {
        Type type = System.Type.GetType(Name.Replace(" ", ""));
        return (ItemCard)Activator.CreateInstance(type, new object[] { this });
    }
}