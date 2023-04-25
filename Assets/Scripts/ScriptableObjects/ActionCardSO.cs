using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActionCard", menuName = "Cards/ActionCard")]
public class ActionCardSO : CardSO {
    public string DescUp;
    public string DescDown;

    public override Card.Types Type => Card.Types.Action;
    public override Card CreateInstance() {
        Debug.Log("Instantiating ActionCard " + Name);
        Type type = System.Type.GetType(Name.Replace(" ", ""));
        return (ActionCard)Activator.CreateInstance(type, new object[] { this });
    }
}
