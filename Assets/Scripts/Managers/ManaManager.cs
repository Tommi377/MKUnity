using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class ManaManager : MonoBehaviour {
    public static ManaManager Instance;

    private List<ManaSource> manaSources = new List<ManaSource>();
    public ReadOnlyCollection<ManaSource> ManaSources => manaSources.AsReadOnly();

    private bool manaUsed = false;

#nullable enable
    public ManaSource? SelectedManaSource { get; private set; } = null;
#nullable disable

    /* EVENT DEFINITIONS - START */
    public event EventHandler OnManaUsed;
    public event EventHandler<OnManaSourceCreatedArgs> OnManaSourceCreated;
    public class OnManaSourceCreatedArgs : EventArgs {
        public ManaSource manaSource;
    }
    public event EventHandler OnManaSourceDeselected;
    public event EventHandler<OnManaSourceSelectedArgs> OnManaSourceSelected;
    public class OnManaSourceSelectedArgs : EventArgs {
        public ManaSource manaSource;
    }
    /* EVENT DEFINITIONS - END */

    private void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogError("More than one instance of a singleton");
        } else {
            Instance = this;
        }
    }

    private void Start() {
        // TODO: Change manasource count depending on factors
        for (int i = 0; i < 3; i++) {
            ManaSource manaSource = new ManaSource();
            manaSources.Add(manaSource);
            OnManaSourceCreated?.Invoke(this, new OnManaSourceCreatedArgs { manaSource = manaSource });
        }

        MouseInput.Instance.OnManaSourceClick += MouseInput_OnManaSourceClick;
    }

    public void RoundStartSetup() {
        DeselectManaSource();
        manaUsed = false;
        foreach (ManaSource manaSource in manaSources) {
            manaSource.Roll();
        }
    }

    public void UseMana() {
        if (!manaUsed) {
            manaUsed = true;
            RollSelectedManaSource();
            OnManaUsed?.Invoke(this, EventArgs.Empty);
        } else {
            Debug.Log("Trying to use mana when it has been alread used");
        }
    }

    public bool IsManaUsed() => manaUsed;

    public void RollSelectedManaSource() {
        SelectedManaSource.Roll();
        DeselectManaSource();
    }

    public bool SelectedManaUsableWithCard(Card card) {
        if (card is ActionCard) {
            ActionCard actionCard = (ActionCard)card;
            if (SelectedManaSource == null) return false;
            if (manaUsed) return false;
            if (SelectedManaSource.Type == ManaSource.Types.Gold && RoundManager.Instance.IsDay()) return true;
            if (actionCard.ManaTypes.Contains(SelectedManaSource.Type)) return true;
        }
        return false;
    }

    private void SelectManaSource(ManaSource manaSource) {
        DeselectManaSource();
        if (manaUsed) return;

        SelectedManaSource = manaSource;
        OnManaSourceSelected?.Invoke(this, new OnManaSourceSelectedArgs() { manaSource = manaSource });
    }

    private void DeselectManaSource() {
        if (SelectedManaSource != null) {
            SelectedManaSource = null;
            OnManaSourceDeselected?.Invoke(this, EventArgs.Empty);
        }
    }

    private void MouseInput_OnManaSourceClick(object sender, MouseInput.OnManaSourceClickArgs e) {
        if (e.manaSourceVisual.ManaSource == SelectedManaSource) {
            DeselectManaSource();
        } else {
            SelectManaSource(e.manaSourceVisual.ManaSource);
        }
    }
}
