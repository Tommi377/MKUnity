using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "Cards/Card")]
public class CardSO : ScriptableObject {
    public string Name;
    public Card.Types Type;
    public List<CardChoice> Choices;
}