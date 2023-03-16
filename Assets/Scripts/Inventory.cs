using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;

public class Inventory {
    private int[] crystalCounts = new int[6] { 0, 0, 0, 0, 0, 0};
    private List<ManaSource.Types> manaList = new List<ManaSource.Types>();

    public int GetCrystalCount(ManaSource.Types type) => crystalCounts[(int)type];
    public bool HasCrystalOf(ManaSource.Types type) => crystalCounts[(int)type] > 0;

    public int GetManaCount(ManaSource.Types type) => manaList.Count;
    public bool HasManaOf(ManaSource.Types type) => manaList.Contains(type);
    public ReadOnlyCollection<ManaSource.Types> GetManaList() => manaList.AsReadOnly();

    public void AddCrystal(ManaSource.Types type) {
        int i = (int)type;
        if (crystalCounts[i] <= 3) {
            crystalCounts[i]++;
        }
    }

    public void RemoveCrystal(ManaSource.Types type) {
        int i = (int)type;
        if (crystalCounts[i] > 0) {
            crystalCounts[i]--;
        }
    }

    public void AddMana(ManaSource.Types type) {
        manaList.Add(type);
    }

    public void RemoveMana(ManaSource.Types type) {
        manaList.Remove(type);
    }
}
