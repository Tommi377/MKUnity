using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndPhaseUI : MonoBehaviour {
    [SerializeField] private Button endButton;

    private void Awake() {
        endButton.onClick.AddListener(() => { ButtonInput.Instance.EndEndPhaseClick(); });
    }
}
