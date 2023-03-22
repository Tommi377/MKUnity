using System.Collections.Generic;
using UnityEngine;


// [CreateAssetMenu(fileName = "StructurePrefabMap", menuName = "Temp/StructurePrefabMap")]
public class StructurePrefabMapSO : ScriptableObject {
    [SerializeField] private StructureTypes[] structureTypeKey;
    [SerializeField] private GameObject[] prefabValue;

    public Dictionary<StructureTypes, GameObject> Map() {
        Dictionary <StructureTypes, GameObject> map = new Dictionary <StructureTypes, GameObject>();
        for (int i = 0; i < structureTypeKey.Length; i++) {
            map.Add(structureTypeKey[i], prefabValue[i]);
        }
        return map;
    }
}
