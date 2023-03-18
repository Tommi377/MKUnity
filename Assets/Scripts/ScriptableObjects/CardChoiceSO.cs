using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardChoice", menuName = "Cards/CardChoice")]
public class CardChoiceSO : ScriptableObject {
    public string Name;
    public string Description;
    public bool Super;
    public int Id;
    public ActionTypes ActionType;
}
