using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActionCard", menuName = "Cards/ActionCard")]
public class ActionCardSO : CardSO {
    public string DescUp;
    public string DescDown;
    public List<Mana.Types> ManaTypes;

    public override Card.Types Type => Card.Types.Action;
    public override Card CreateInstance() {
        Type type = System.Type.GetType(Name.Replace(" ", ""));
        return (ActionCard)Activator.CreateInstance(type, new object[] { this });
    }
}
