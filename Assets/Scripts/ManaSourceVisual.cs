using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ManaSourceVisual : MonoBehaviour, ISelectableUI {
    [SerializeField] private Transform visual;
    [SerializeField] private Transform highlight;

    public ManaSource ManaSource { get; private set; }

    public void Init(ManaSource manaSource) {
        ManaSource = manaSource;
        DrawVisual();
    }

    public void DrawVisual() {
        Image image = visual.GetComponent<Image>();
        image.color = Mana.GetColor(ManaSource.ManaType);
    }

    public void Select() {
        highlight.gameObject.SetActive(true);
    }

    public void Deselect() {
        highlight.gameObject.SetActive(false);
    }
}
