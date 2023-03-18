using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ManaSourceUI : MonoBehaviour {
    [SerializeField] private Transform manaSourceContainer;
    [SerializeField] private Transform manaSourcePrefab;
    [SerializeField] private TMP_Text manaUsedText;

#nullable enable
    private ManaSourceVisual? selectedManaSourceVisual;
#nullable disable

    private void Start() {
        if (ManaManager.Instance.ManaSources.Count > 0 ) {
            foreach(ManaSource manaSource in  ManaManager.Instance.ManaSources) {
                AddManaSourceVisual(manaSource);
            }
        }

        RoundManager.Instance.OnNewRound += RoundManager_OnNewRound;

        ManaManager.Instance.OnManaUsed += ManaManager_OnManaUsed;
        ManaManager.Instance.OnManaSourceCreated += ManaManager_OnManaSourceCreated;
        ManaManager.Instance.OnManaSourceSelected += ManaManager_OnManaSourceSelected;
        ManaManager.Instance.OnManaSourceDeselected += ManaManager_OnManaSourceDeselected;


        ManaSource.OnManaSourceRoll += ManaSource_OnManaSourceRoll;
    }

    private void SelectManaSource(ManaSourceVisual manaSourceVisual) {
        DeselectManaSource();
        selectedManaSourceVisual = manaSourceVisual;
        selectedManaSourceVisual.Select();
    }

    private void DeselectManaSource() {
        if (selectedManaSourceVisual != null) {
            selectedManaSourceVisual.Deselect();
            selectedManaSourceVisual = null;
        }
    }

    private void AddManaSourceVisual(ManaSource manaSource) {
        ManaSourceVisual visual = Instantiate(manaSourcePrefab, manaSourceContainer).GetComponent<ManaSourceVisual>();
        visual.Init(manaSource);
    }

    private void RoundManager_OnNewRound(object sender, System.EventArgs e) {
        manaUsedText.gameObject.SetActive(false);
    }

    private void ManaManager_OnManaUsed(object sender, System.EventArgs e) {
        manaUsedText.gameObject.SetActive(true);
    }

    private void ManaManager_OnManaSourceCreated(object sender, ManaManager.OnManaSourceCreatedArgs e) {
        AddManaSourceVisual(e.manaSource);
    }

    private void ManaManager_OnManaSourceSelected(object sender, ManaManager.OnManaSourceSelectedArgs e) {
        foreach (Transform child in manaSourceContainer) {
            ManaSourceVisual manaSourceVisual = child.GetComponent<ManaSourceVisual>();
            if (manaSourceVisual.ManaSource == e.manaSource) {
                SelectManaSource(manaSourceVisual);
            }
        }
    }

    private void ManaManager_OnManaSourceDeselected(object sender, System.EventArgs e) {
        DeselectManaSource();
    }

    private void ManaSource_OnManaSourceRoll(object sender, ManaSource.OnManaSourceRollArgs e) {
        foreach (Transform child in manaSourceContainer) {
            ManaSourceVisual manaSourceVisual = child.GetComponent<ManaSourceVisual>();
            if (manaSourceVisual.ManaSource == e.manaSource) {
                if (manaSourceVisual == selectedManaSourceVisual) DeselectManaSource();
                manaSourceVisual.DrawVisual();
            }
        }
    }
}
