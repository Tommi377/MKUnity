using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CardList", menuName = "Cards/CardList")]
public class CardListSO : ScriptableObject {
    public List<ActionCardCount> List;

    public List<CardSO> GetCards => List.Select(item => item.Card).ToList();
}

[Serializable]
public struct ActionCardCount {
    public CardSO Card;
    public int Count;
}