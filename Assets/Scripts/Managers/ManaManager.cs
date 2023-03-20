using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class ManaManager : MonoBehaviour {
    public static ManaManager Instance;

    private List<ManaSource> manaSources = new List<ManaSource>();
    public ReadOnlyCollection<ManaSource> ManaSources => manaSources.AsReadOnly();

    private int defaultMaxChannels = 1;
    private int maxChannels = 1;
    private int manaChanneled = 0;

#nullable enable
    public ManaSource? SelectedManaSource { get; private set; } = null;
    public Mana? SelectedMana { get; private set; } = null;
#nullable disable

    /* EVENT DEFINITIONS - START */
    public event EventHandler OnManaUsed;
    public event EventHandler OnManaChannelUpdate;
    public event EventHandler<OnManaSourceCreatedArgs> OnManaSourceCreated;
    public class OnManaSourceCreatedArgs : EventArgs {
        public ManaSource manaSource;
    }
    public event EventHandler OnManaSourceDeselected;
    public event EventHandler<OnManaSourceSelectedArgs> OnManaSourceSelected;
    public class OnManaSourceSelectedArgs : EventArgs {
        public ManaSource manaSource;
    }
    public event EventHandler OnManaDeselected;
    public event EventHandler<OnManaSelectedArgs> OnManaSelected;
    public class OnManaSelectedArgs : EventArgs {
        public Mana mana;
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

        MouseInputManager.Instance.OnManaSourceClick += MouseInputManager_OnManaSourceClick;
        MouseInputManager.Instance.OnManaTokenClick += MouseInputManager_OnManaTokenClick;
        MouseInputManager.Instance.OnManaCrystalClick += MouseInputManager_OnManaCrystalClick;


        ButtonInputManager.Instance.OnChannelManaClick += ButtonInputManager_OnChannelManaClick;
    }

    public void RoundStartSetup() {
        DeselectManaSource();
        maxChannels = defaultMaxChannels;
        manaChanneled = 0;
        foreach (ManaSource manaSource in manaSources) {
            manaSource.Roll();
        }
    }

    public bool CanChannelMana() => manaChanneled < maxChannels;

    public void RollSelectedManaSource() {
        SelectedManaSource.Roll();
        DeselectManaSource();
    }

    public bool SelectedManaUsableWithCard(Card card) {
        if (card is ActionCard actionCard) {
            if (SelectedMana == null) return false;
            if (SelectedMana.Type == Mana.Types.Gold && RoundManager.Instance.IsDay()) return true;
            if (actionCard.ManaTypes.Contains(SelectedMana.Type)) return true;
        }
        return false;
    }

    public void UseSelectedMana() {

        GameManager.Instance.CurrentPlayer.GetInventory().RemoveMana(SelectedMana);
        OnManaUsed?.Invoke(this, EventArgs.Empty);
        DeselectMana();
    }

    public void IncreaseMaxManaChannels() {
        maxChannels += 1;
        OnManaChannelUpdate?.Invoke(this, EventArgs.Empty);
    }

    private void ChannelSelectedMana() {
        if (SelectedManaSource == null) {
            Debug.Log("No mana to channel");
            return;
        }

        if (!CanChannelMana()) {
            Debug.Log("Already channeled");
            return;
        }

        GameManager.Instance.CurrentPlayer.GetInventory().AddToken(SelectedManaSource.ManaType);

        manaChanneled += 1;
        RollSelectedManaSource();
        OnManaChannelUpdate?.Invoke(this, EventArgs.Empty);
    }

    private void SelectManaSource(ManaSource manaSource) {
        DeselectManaSource();
        if (!CanChannelMana()) return;

        SelectedManaSource = manaSource;
        OnManaSourceSelected?.Invoke(this, new OnManaSourceSelectedArgs() { manaSource = manaSource });
    }

    private void DeselectManaSource() {
        if (SelectedManaSource != null) {
            SelectedManaSource = null;
            OnManaSourceDeselected?.Invoke(this, EventArgs.Empty);
        }
    }

    private void SelectMana(Mana mana) {
        SelectedMana = mana;
        OnManaSelected?.Invoke(this, new OnManaSelectedArgs() { mana = mana });
    }

    private void DeselectMana() {
        if (SelectedMana != null) {
            SelectedMana = null;
            OnManaDeselected?.Invoke(this, EventArgs.Empty);
        }
    }

    private void MouseInputManager_OnManaSourceClick(object sender, MouseInputManager.OnManaSourceClickArgs e) {
        if (e.manaSourceVisual.ManaSource == SelectedManaSource) {
            DeselectManaSource();
        } else {
            SelectManaSource(e.manaSourceVisual.ManaSource);
        }
    }

    private void ButtonInputManager_OnChannelManaClick(object sender, EventArgs e) {
        ChannelSelectedMana();
    }

    private void MouseInputManager_OnManaTokenClick(object sender, MouseInputManager.OnManaTokenClickArgs e) {
        if (e.manaTokenVisual.Mana == SelectedMana) {
            DeselectMana();
        } else {
            SelectMana(e.manaTokenVisual.Mana);
        }
    }

    private void MouseInputManager_OnManaCrystalClick(object sender, MouseInputManager.OnManaCrystalClickArgs e) {
        Player player = GameManager.Instance.CurrentPlayer;

        if (player.GetInventory().HasCrystalOf(e.crystalCounterVisual.Type)) {
            if (SelectedMana != null && SelectedMana.Crystal && e.crystalCounterVisual.Type == SelectedMana.Type) {
                DeselectMana();
            } else {
                SelectMana(player.GetInventory().GetCrystalOf(e.crystalCounterVisual.Type));
            }
        }
    }

}
