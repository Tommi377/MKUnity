using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatUI : MonoBehaviour {

    private void Start() {
        Combat.OnCombatStateEnter += Combat_OnCombatStateEnter;

        gameObject.SetActive(false);
    }

    private void UpdateUI(Combat.States state) {
        gameObject.SetActive(true);

        switch(state) {
            case Combat.States.Start:
                break;
            case Combat.States.RangedStart:
                break;
            case Combat.States.RangedPlay:
                break;
            case Combat.States.BlockStart:
                break;
            case Combat.States.BlockPlay:
                break;
            case Combat.States.AssignStart:
                break;
            case Combat.States.AssignDamage:
                break;
            case Combat.States.AttackStart:
                break;
            case Combat.States.AttackPlay:
                break;
            case Combat.States.End:
                break;
        }

    }

    private void Combat_OnCombatStateEnter(object sender, System.EventArgs e) {
        throw new System.NotImplementedException();
    }
}
