using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyList", menuName = "Entity/EnemyList")]
public class EnemyListSO : ScriptableObject {
    public List<EnemySO> List;
}
