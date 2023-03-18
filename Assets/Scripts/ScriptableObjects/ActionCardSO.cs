using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActionCard", menuName = "Cards/ActionCard")]
public class ActionCardSO : CardSO {
    public string DescUp;
    public string DescDown;
    public List<Mana.Types> ManaTypes;
}
