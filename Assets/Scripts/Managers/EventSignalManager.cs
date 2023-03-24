using System;

public static class EventSignalManager {
    public static event EventHandler<OnChangeHandUIModeArgs> OnChangeHandUIMode;
    public class OnChangeHandUIModeArgs : EventArgs {
        public HandUI.SelectionMode mode;
    }

    public static void ResetStaticData() {
        OnChangeHandUIMode = null;
    }

    public static void ChangeHandUIMode(object sender, HandUI.SelectionMode mode) => OnChangeHandUIMode?.Invoke(sender, new OnChangeHandUIModeArgs() { mode = mode });


}
