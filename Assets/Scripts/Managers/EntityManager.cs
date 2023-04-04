using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class EntityManager : MonoBehaviour {
    public static EntityManager Instance;

    [SerializeField] private List<EnemyListSO> enemyListSOs;

    private Dictionary<EntityTypes, List<EnemySO>> enemyListSOMap = new Dictionary<EntityTypes, List<EnemySO>>();

    private void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogError("More than one instance of a singleton");
        } else {
            Instance = this;
        }

        foreach (EnemyListSO enemyListSO in enemyListSOs) {
            // TODO: account relative token amounts with enemyListSO.List.Count
            enemyListSOMap.Add(enemyListSO.Type, enemyListSO.List.Select(list => list.Enemy).ToList());
        }
    }

    public List<EnemySO> GetEntitySOs(EntityTypes type) {
        if (enemyListSOMap.TryGetValue(type, out List<EnemySO> enemyListSO)) {
            return enemyListSO;
        }
        return new List<EnemySO>();
    }
}
