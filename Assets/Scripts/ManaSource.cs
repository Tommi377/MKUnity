using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaSource {

    /* EVENT DEFINITIONS - START */
    public static event EventHandler<OnManaSourceRollArgs> OnManaSourceRoll;
    public class OnManaSourceRollArgs : EventArgs {
        public ManaSource manaSource;
    }
    /* EVENT DEFINITIONS - END */

    public enum Types {
        Red, Green, Blue, White, Gold, Black
    }
    public Types Type { get; private set; }

    public ManaSource() {
        Roll();
    }

    public void Roll() {
        Type = (Types)UnityEngine.Random.Range(0, 6);
        OnManaSourceRoll?.Invoke(this, new OnManaSourceRollArgs() { manaSource = this });
    }

    private static readonly Color[] colors = { Color.red, Color.green, Color.blue, Color.white, Color.yellow, Color.black };
    public static Color GetColor(Types type) {
        return colors[(int)type];
    }
}
