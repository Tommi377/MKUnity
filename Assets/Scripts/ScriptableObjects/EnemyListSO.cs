using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyList", menuName = "Entity/EnemyList")]
public class EnemyListSO : ScriptableObject {
    public EntityTypes Type;
    public List<EnemySO> List;
}
