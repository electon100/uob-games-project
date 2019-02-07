using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WiimoteApi;

public class WiimoteBehaviour : MonoBehaviour {

    private Wiimote wiimote1, wiimote2;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (!WiimoteManager.HasWiimote()) return;

        wiimote1 = WiimoteManager.Wiimotes[0];
        if (WiimoteManager.Wiimotes.Count > 1) wiimote2 = WiimoteManager.Wiimotes[1];
	}
}
