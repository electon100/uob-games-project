using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WiimoteApi;

public class WiimoteBehaviour : MonoBehaviour {

    private Wiimote wiimoteRed, wiimoteBlue;
    public RectTransform ir_pointerRed, ir_pointerBlue;
    bool redIsSet;

	// Use this for initialization
	void Start () {
        redIsSet = false;
    }
	
	// Update is called once per frame
	void Update () {
        WiimoteManager.FindWiimotes();
        if (!WiimoteManager.HasWiimote()) return;

        if (!redIsSet)
        {
            wiimoteRed = WiimoteManager.Wiimotes[0];
            wiimoteRed.SetupIRCamera(IRDataType.BASIC);
            redIsSet = true;
        }

        int ret;
        do
        {
            ret = wiimoteRed.ReadWiimoteData();
        } while (ret > 0);

        float[] pointer = wiimoteRed.Ir.GetPointingPosition();
        ir_pointerRed.anchorMin = new Vector2(pointer[0], pointer[1]);
        ir_pointerRed.anchorMax = new Vector2(pointer[0], pointer[1]);
    }

    private void OnApplicationQuit()
    {
        if (wiimoteRed != null)
        {
            WiimoteManager.Cleanup(wiimoteRed);
            wiimoteRed = null;
        }
    }
}
