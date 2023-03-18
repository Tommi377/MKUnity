using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ManaTokenVisual : MonoBehaviour, ISelectableUI {
    [SerializeField] private Transform visual;
    [SerializeField] private Transform highlight;

    public Mana Mana { get; private set; }

    public void Init(Mana mana) {
        Mana = mana;
        DrawVisual();
    }

    public void DestroySelf() {
        Destroy(gameObject);
    }

    public void DrawVisual() {
        Image image = visual.GetComponent<Image>();
        image.color = ManaSource.GetColor(Mana.Type);
    }

    public void Select() {
        highlight.gameObject.SetActive(true);
    }

    public void Deselect() {
        highlight.gameObject.SetActive(false);
    }
}
