using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TabItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    [SerializeField] private TabGroup tabGroup;

    public event EventHandler OnTabSelected;
    public event EventHandler OnTabDeselected;

    private Image background;

    private void Awake() {
        background = GetComponent<Image>();
    }

    private void Start() {
        tabGroup.Subscribe(this);
    }

    public void SetColor(Color color) {
        background.color = color;
    }

    public void Select() {
        OnTabSelected?.Invoke(this, EventArgs.Empty);
    }

    public void Deselect() {
        OnTabDeselected?.Invoke(this, EventArgs.Empty);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        tabGroup.OnTabEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData) {
        tabGroup.OnTabExit(this);
    }

    public void OnPointerClick(PointerEventData eventData) {
        tabGroup.OnTabSelected(this);
    }
}
