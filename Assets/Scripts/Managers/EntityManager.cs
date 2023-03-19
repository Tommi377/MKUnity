using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EntityManager : MonoBehaviour {
    public static EntityManager Instance;

    [SerializeField] private List<EntityTypes> enemyListKeys;
    [SerializeField] private List<EnemyListSO> enemyListValues;

    private Dictionary<EntityTypes, EnemyListSO> enemyListSOMap = new Dictionary<EntityTypes, EnemyListSO>();

    private void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogError("More than one instance of a singleton");
        } else {
            Instance = this;
        }

        for (int i = 0; i < enemyListKeys.Count; i++) {
            enemyListSOMap.Add(enemyListKeys[i], enemyListValues[i]);
        }
        Debug.Log(enemyListSOMap);
    }

    public List<EnemySO> GetEntitySOs(EntityTypes type) {
        if (enemyListSOMap.TryGetValue(type, out EnemyListSO enemyListSO)) {
            return enemyListSO.List;
        }
        return new List<EnemySO>();
    }
}
