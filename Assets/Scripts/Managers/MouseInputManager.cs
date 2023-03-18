using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class MouseInputManager : MonoBehaviour {
    public static MouseInputManager Instance;

    [SerializeField] private GraphicRaycaster raycaster;

    /* EVENT DEFINITIONS - START */
    public event EventHandler OnNonCardClick;
    public event EventHandler<OnCardClickArgs> OnCardClick;
    public class OnCardClickArgs : EventArgs {
        public CardVisual cardVisual;
    }
    public event EventHandler<OnHexClickArgs> OnHexClick;
    public class OnHexClickArgs : EventArgs {
        public Hex hex;
    }
    public event EventHandler<OnManaSourceClickArgs> OnManaSourceClick;
    public class OnManaSourceClickArgs : EventArgs {
        public ManaSourceVisual manaSourceVisual;
    }
    public event EventHandler<OnManaCrystalClickArgs> OnManaCrystalClick;
    public class OnManaCrystalClickArgs : EventArgs {
        public CrystalCounterVisual crystalCounterVisual;
    }
    public event EventHandler<OnManaTokenClickArgs> OnManaTokenClick;
    public class OnManaTokenClickArgs : EventArgs {
        public ManaTokenVisual manaTokenVisual;
    }
    /* EVENT DEFINITIONS - END */

    void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogError("More than one instance of a singleton");
        } else {
            Instance = this;
        }
    }

    // Update is called once per frame
    private void Update() {
        if (Input.GetMouseButtonDown(0)) {

            // UI ray
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            List<RaycastResult> results = new List<RaycastResult>();

            pointerData.position = Input.mousePosition;
            raycaster.Raycast(pointerData, results);

            if (results.Count > 0) {

                // Raycast hits card
                RaycastResult found = results.Find(result => result.gameObject.CompareTag("Card"));
                if (found.gameObject) {
                    CardVisual cardVisual = found.gameObject.GetComponent<CardVisual>();
                    Debug.Log("Hit " + found.gameObject.name);
                    OnCardClick?.Invoke(this, new OnCardClickArgs { cardVisual = cardVisual });
                    return;
                }


                // Raycast hits mana source
                found = results.Find(result => result.gameObject.CompareTag("ManaSource"));
                if (found.gameObject) {
                    ManaSourceVisual manaSourceVisual = found.gameObject.GetComponent<ManaSourceVisual>();
                    Debug.Log("Hit " + found.gameObject.name);
                    OnManaSourceClick?.Invoke(this, new OnManaSourceClickArgs { manaSourceVisual = manaSourceVisual });
                    return;
                }

                // Raycast hits mana crystal
                found = results.Find(result => result.gameObject.CompareTag("ManaCrystal"));
                if (found.gameObject) {
                    CrystalCounterVisual crystalCounterVisual = found.gameObject.GetComponent<CrystalCounterVisual>();
                    Debug.Log("Hit " + found.gameObject.name);
                    OnManaCrystalClick?.Invoke(this, new OnManaCrystalClickArgs { crystalCounterVisual = crystalCounterVisual });
                    return;
                }

                // Raycast hits mana token
                found = results.Find(result => result.gameObject.CompareTag("ManaToken"));
                if (found.gameObject) {
                    ManaTokenVisual manaTokenVisual = found.gameObject.GetComponent<ManaTokenVisual>();
                    Debug.Log("Hit " + found.gameObject.name);
                    OnManaTokenClick?.Invoke(this, new OnManaTokenClickArgs { manaTokenVisual = manaTokenVisual });
                    return;
                }

                return;
            }

            OnNonCardClick?.Invoke(this, EventArgs.Empty);

            // Check non UI stuff after this point

            // 3D ray
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            int layerMask = 1 << 6; // Hex Layer

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) {
                Transform selection = hit.transform;
                if (selection) {
                    Hex hex = selection.parent.parent.GetComponent<Hex>();
                    OnHexClick?.Invoke(this, new OnHexClickArgs { hex = hex });
                    Debug.Log("Clicked hex: " + selection.parent.parent.name);
                }
            }
        }
    }
}
