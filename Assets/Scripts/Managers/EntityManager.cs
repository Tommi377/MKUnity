using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public List<EnemySO> GetEnemySOs(EntityTypes type) {
        if (enemyListSOMap.TryGetValue(type, out List<EnemySO> enemyListSO)) {
            return enemyListSO;
        }
        return new List<EnemySO>();
    }

    public EnemySO GetRandomEnemySO(EntityTypes type) {
        List<EnemySO> entitySOs = GetEnemySOs(type);

        if (entitySOs.Count > 0) {
            return entitySOs[Random.Range(0, entitySOs.Count)];
        } else {
            Debug.Log("Type didn't have any possible enities (" + type + ")");
            return null;
        }
    }
}
