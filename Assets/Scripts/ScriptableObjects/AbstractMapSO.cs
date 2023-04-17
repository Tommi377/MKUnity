using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbstractMapSO<T1, T2> : ScriptableObject {
    [SerializeField] private T1[] keys;
    [SerializeField] private T2[] values;

    public Dictionary<T1, T2> Map() {
        Dictionary<T1, T2> map = new Dictionary<T1, T2>();
        for (int i = 0; i < keys.Length; i++) {
            map.Add(keys[i], values[i]);
        }
        return map;
    }

    public T2 GetValue(T1 key) => values[Array.IndexOf(keys, key)];
    public bool TryGetValue(T1 key, out T2 value) {
        int index = Array.IndexOf(keys, key);
        value = default;

        if (index >= 0) {
            value = values[index];
        }

        return index >= 0;
    }

    public bool HasKey(T1 key) => keys.Contains(key);
}
