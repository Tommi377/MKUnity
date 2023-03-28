using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleableHighlightVisual : MonoBehaviour {
    [SerializeField] private Transform backgroundHighlight;

    public void Select() {
        backgroundHighlight.gameObject.SetActive(true);
    }

    public void Deselect() {
        backgroundHighlight.gameObject.SetActive(false);
    }
}
