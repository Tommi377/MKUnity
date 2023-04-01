using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class HealUI : MonoBehaviour {
    [SerializeField] private ExpandingButtonUI buttonContainer;
    [SerializeField] private TMP_Text healText;

    private Player Player => GameManager.Instance.CurrentPlayer;

    private void Start() {
        Player.OnPlayerHealUpdate += Player_OnPlayerHealUpdate;
        gameObject.SetActive(false);
    }

    private void UpdateUI() {
        healText.SetText("Current heal: " + Player.Heal);

        buttonContainer.ClearButtons();

        if (Player.IsWoundInHand()) buttonContainer.AddButton("Player", () => HealAction.HealClick(this, null));
        Player.GetWoundedUnits().ForEach(unit => {
            var options = new ExpandingButtonUI.Options() { Interactable = Player.Heal >= unit.Level };
            buttonContainer.AddButton(unit.ToString(), () => HealAction.HealClick(this, unit), options);
        });
    }

    private bool HealTargetExists => Player.IsWoundInHand() || Player.GetWoundedUnits().Any();

    private void Enable() {
        UpdateUI();
        gameObject.SetActive(true);
    }

    private void Disable() {
        gameObject.SetActive(false);
    }

    private void Player_OnPlayerHealUpdate(object sender, Player.PlayerIntEventArgs e) {
        if (e.Value == 0)
            Disable();
        else if (HealTargetExists)
            Enable();
    }
}

public static class HealAction {
    public static event EventHandler<OnHealClickArgs> OnHealClick;
    public class OnHealClickArgs : EventArgs {
        public UnitCard Unit; // Set null if healing player;
    }

    public static void ResetStaticData() {
        OnHealClick = null;
    }

    public static void HealClick(object sender, UnitCard unit) {
        OnHealClick?.Invoke(sender, new OnHealClickArgs { Unit = unit });
    }
}
