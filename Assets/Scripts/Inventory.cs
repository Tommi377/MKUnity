using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;
//   Red, Green, Blue, White, Gold, Black
public class Inventory {
    public Player Owner {  get; private set; }

    public static event EventHandler<OnInventoryUpdateArgs> OnInventoryUpdate;
    public class OnInventoryUpdateArgs : EventArgs {
        public Player player;
        public Inventory inventory;
    }

    public static void ResetStaticData() {
        OnInventoryUpdate = null;
    }

    private Dictionary<Mana.Types, List<Mana>> crystalDict = new Dictionary<Mana.Types, List<Mana>> {
        { Mana.Types.Red, new List<Mana>() },
        { Mana.Types.Green, new List<Mana>() },
        { Mana.Types.Blue, new List<Mana>() },
        { Mana.Types.White, new List<Mana>() },
        { Mana.Types.Gold, new List<Mana>() },
        { Mana.Types.Black, new List<Mana>() }
    };
    private List<Mana> tokenList = new List<Mana>();

    public Inventory(Player owner) {
        Owner = owner;
    }

    public int GetCrystalCount(Mana.Types type) => crystalDict[type].Count;
    public bool HasCrystalOf(Mana.Types type) => GetCrystalCount(type) > 0;
    public Mana GetCrystalOf(Mana.Types type) => crystalDict[type].First();

    public bool HasTokenOf(Mana.Types type) => tokenList.Any(mana => type == mana.Type);
    public ReadOnlyCollection<Mana> GetTokenList() => tokenList.AsReadOnly();

    public void AddCrystal(Mana.Types type) => AddCrystal(new Mana(type, true));
    public void AddCrystal(Mana mana) {
        if (GetCrystalCount(mana.Type) <= 3) {
            crystalDict[mana.Type].Add(mana);
            OnInventoryUpdate?.Invoke(this, new OnInventoryUpdateArgs { player = Owner, inventory = this });
        }
    }

    public void RemoveCrystal(Mana mana) {
        if (GetCrystalCount(mana.Type) > 0) {
            crystalDict[mana.Type].Remove(mana);
            OnInventoryUpdate?.Invoke(this, new OnInventoryUpdateArgs { player = Owner, inventory = this });
        }
    }

    public void AddToken(Mana.Types type) => AddToken(new Mana(type, false));
    public void AddToken(Mana mana) {
        tokenList.Add(mana);
        OnInventoryUpdate?.Invoke(this, new OnInventoryUpdateArgs { player = Owner, inventory = this });
    }

    public void RemoveToken(Mana mana) {
        tokenList.Remove(mana);
        OnInventoryUpdate?.Invoke(this, new OnInventoryUpdateArgs { player = Owner, inventory = this });
    }

    public void RemoveMana(Mana mana) {
        if (mana.Crystal) {
            RemoveCrystal(mana);
        } else {
            RemoveToken(mana);
        }
    }
}