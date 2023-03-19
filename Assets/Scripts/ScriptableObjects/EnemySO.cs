using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Entity/Enemy")]
public class EnemySO : ScriptableObject {
    public Sprite Sprite;
    public string Name;
    public int Armor;
    public int Fame;
    public EntityTypes EntityType;
    public List<EnemyAttack> Attacks;
    public List<CombatElements> Resistances;
    public List<EnemyAbilities> Abilities;
}


[Serializable]
public class EnemyAttack {
    public int Damage;
    public CombatElements Element;
}