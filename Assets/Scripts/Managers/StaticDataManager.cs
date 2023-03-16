using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticDataManager : MonoBehaviour {
    private void Awake() {
        Player.ResetStaticData();
        Combat.ResetStaticData();
        EventSignalManager.ResetStaticData();
    }
}
