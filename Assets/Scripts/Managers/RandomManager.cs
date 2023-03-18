using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomManager : MonoBehaviour {
    [SerializeField] private string seedString;
    
    private int seed;

    void Awake() {
        seed = seedString.GetHashCode();
        Random.InitState(seed);
    }
}
