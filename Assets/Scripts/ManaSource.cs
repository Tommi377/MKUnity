using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaSource {

    /* EVENT DEFINITIONS - START */
    public static event EventHandler<OnManaSourceRollArgs> OnManaSourceUpdate;
    public class OnManaSourceRollArgs : EventArgs {
        public ManaSource manaSource;
    }
    /* EVENT DEFINITIONS - END */

    public Mana.Types ManaType { get; private set; }

    public ManaSource() {
        Roll();
    }

    public void Roll() {
        ManaType = (Mana.Types)UnityEngine.Random.Range(0, 6);
        OnManaSourceUpdate?.Invoke(this, new OnManaSourceRollArgs() { manaSource = this });
    }

    public void SetManaType(Mana.Types type) {
        ManaType = type;
        OnManaSourceUpdate?.Invoke(this, new OnManaSourceRollArgs() { manaSource = this });
    }
}
