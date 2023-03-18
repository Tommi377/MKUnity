using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardList", menuName = "Cards/CardList")]
public class CardListSO : ScriptableObject {
    public List<CardSO> cards;
}
