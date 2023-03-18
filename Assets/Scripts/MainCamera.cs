using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour {
    [SerializeField]
    Player _player;

    // Update is called once per frame
    void Update()     {
        transform.position = _player.transform.position + new Vector3(0, 9, -3);
    }
}
