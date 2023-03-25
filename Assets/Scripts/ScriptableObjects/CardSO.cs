using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "Cards/Card")]
public class CardSO : ScriptableObject {
    public string Name;
    public List<CardChoice> Choices;
    public virtual Card.Types Type => Card.Types.Wound;
}