using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EntityManager : MonoBehaviour {
    public static EntityManager Instance;

    [SerializeField] private List<EnemyListSO> enemyListValues;

    private Dictionary<EntityTypes, List<EnemySO>> enemyListSOMap = new Dictionary<EntityTypes, List<EnemySO>>();

    private void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogError("More than one instance of a singleton");
        } else {
            Instance = this;
        }

        for (int i = 0; i < enemyListValues.Count; i++) {
            enemyListSOMap.Add(enemyListValues[i].Type, enemyListValues[i].List);
        }
    }

    public List<EnemySO> GetEntitySOs(EntityTypes type) {
        if (enemyListSOMap.TryGetValue(type, out List<EnemySO> enemyListSO)) {
            return enemyListSO;
        }
        return new List<EnemySO>();
    }
}
