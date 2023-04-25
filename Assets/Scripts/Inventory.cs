using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

//   Red, Green, Blue, White, Gold, Black
[RequireComponent(typeof(Player))]
public class Inventory : MonoBehaviour {
    public Player Owner {  get; private set; }

    public static event EventHandler<OnInventoryUpdateArgs> OnInventoryUpdate;
    public class OnInventoryUpdateArgs : EventArgs {
        public Player player;
        public Inventory inventory;
    }

    public static void ResetStaticData() {
        OnInventoryUpdate = null;
    }

    private int crystalCount = 0;

    private void Awake() {
        Owner = GetComponent<Player>();
    }

    public int GetCrystalCount() => crystalCount;

    public void AddCrystal() {
        crystalCount += 1;
        OnInventoryUpdate?.Invoke(this, new OnInventoryUpdateArgs { player = Owner, inventory = this });
    }

    public void RemoveCrystal() {
        crystalCount -= 1;
        OnInventoryUpdate?.Invoke(this, new OnInventoryUpdateArgs { player = Owner, inventory = this });
    }
}