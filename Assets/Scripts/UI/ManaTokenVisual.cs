using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaTokenVisual : MonoBehaviour {
    ManaSource.Types type;

    public void Init(ManaSource.Types type) {
        this.type = type;
        GetComponent<Image>().color = ManaSource.GetColor(type);
    }

    public void DestroySelf() {
        Destroy(gameObject);
    }
}
