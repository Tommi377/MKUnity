using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartPhaseUI : MonoBehaviour {
    [SerializeField] private Button drawButton;
    [SerializeField] private Button startButton;

    private void Awake() {
        drawButton.onClick.AddListener(() => { 
            ButtonInput.Instance.DrawStartHandClick();
            drawButton.gameObject.SetActive(false);
            startButton.gameObject.SetActive(true);
        });
        startButton.onClick.AddListener(() => { ButtonInput.Instance.EndStartPhaseClick(); });

        startButton.gameObject.SetActive(false);
    }

    private void OnEnable() {
        drawButton.gameObject.SetActive(true);
        startButton.gameObject.SetActive(false);
    }
}
