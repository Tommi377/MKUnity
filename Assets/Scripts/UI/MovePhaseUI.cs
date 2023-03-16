using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovePhaseUI : MonoBehaviour {
    [SerializeField] private Button endMoveButton;

    private void Awake() {
        endMoveButton.onClick.AddListener(() => { ButtonInput.Instance.EndMovementClick(); });
    }
}
