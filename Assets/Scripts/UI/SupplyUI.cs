using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyUI : MonoBehaviour {
    [SerializeField] private TabGroup tabGroup;
    [SerializeField] private ActionPageUI actionPageUI;

    //public void Start() {
    //    Close();
    //    transform.GetChild(0).gameObject.SetActive(true);
    //}

    public void OpenSpecial(RoundManager.States state) {
        Open();
        switch (state) {
            case RoundManager.States.LevelUp:
                actionPageUI.LevelUpMode(true);
                tabGroup.OnTabSelected(0);
                break;
            default:
                Debug.LogError("No special open for this state");
                Close();
                break;
        }
    }

    public void Toggle() {
        if (gameObject.activeSelf) {
            Close();
        } else {
            Open();
        }
    }

    public void Close() {
        gameObject.SetActive(false);
    }

    public void Open() {
        gameObject.SetActive(true);
    }
}

public static class SupplyAction {
    public static event EventHandler<ChoiceIndexArgs> OnAdvancedActionChoose;
    public class ChoiceIndexArgs : EventArgs {
        public int choiceId;
    }

    public static void ResetStaticData() {
        OnAdvancedActionChoose = null;
    }

    public static void AdvancedActionChoose(object sender, int index) {
        Debug.Log(index);
        OnAdvancedActionChoose?.Invoke(sender, new ChoiceIndexArgs { choiceId = index });
    }
}