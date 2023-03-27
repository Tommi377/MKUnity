using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class CardSO : ScriptableObject {
    public string Name;
    public List<CardChoice> Choices;

    public abstract Card.Types Type { get; }
    public abstract Card CreateInstance();
}