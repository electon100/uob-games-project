using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WiimoteApi;

public class WiimoteBehaviour : MonoBehaviour {

    private Wiimote wiimoteRed, wiimoteBlue;
    public RectTransform ir_pointerRed, ir_pointerBlue;
    bool redIsSet, blueIsSet;

	// Use this for initialization
	void Start () {
        redIsSet = false;
        blueIsSet = false;
    }
	
	// Update is called once per frame
	void Update () {
        WiimoteManager.FindWiimotes();
        if (!WiimoteManager.HasWiimote()) return;

        // Setup red camera when at least one remote is detected
        if (!redIsSet) redIsSet = setupWiimote(wiimoteRed, 0);

        // Set up the blue camera when multiple remotes are detected
        if (!blueIsSet && WiimoteManager.Wiimotes.Count > 1) 
            blueIsSet = setupWiimote(wiimoteBlue, 1);

        if (redIsSet)
        {
            collectWiimoteData(wiimoteRed);
            updateCrosshairPosition(wiimoteRed, ir_pointerRed);
        }
        if (blueIsSet)
        {
            collectWiimoteData(wiimoteBlue);
            updateCrosshairPosition(wiimoteBlue, ir_pointerBlue);
        }
    }

    private void updateCrosshairPosition(Wiimote wiimote, RectTransform irPointer)
    {
        float[] pointer = wiimote.Ir.GetPointingPosition();
        irPointer.anchorMin = new Vector2(pointer[0], pointer[1]);
        irPointer.anchorMax = new Vector2(pointer[0], pointer[1]);
    }

    private void collectWiimoteData(Wiimote wiimote)
    {
        int ret;
        do
        {
            ret = wiimote.ReadWiimoteData();
        } while (ret > 0);
    }

    private bool setupWiimote(Wiimote wiimote, int num)
    {
        wiimote = WiimoteManager.Wiimotes[num];
        wiimote.SetupIRCamera(IRDataType.BASIC);
        return true;
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
