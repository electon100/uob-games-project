using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WiimoteApi;

public class WiimoteBehaviour : MonoBehaviour {

    private Wiimote wiimoteRed, wiimoteBlue;
    public RectTransform ir_pointerRed, ir_pointerBlue;
    bool redIsSet, blueIsSet;
    public Text redTimer, blueTimer;
    private float redTime, blueTime;

    public Transform blueTimeOverPanel;
    public Transform redTimeOverPanel;



    // Use this for initialization
    void Start () {
        redIsSet = false;
        blueIsSet = false;
        redTime = 5.0f;
        blueTime = 5.0f;
        displayTime();
    }

    // Update is called once per frame
    void Update() {
        WiimoteManager.FindWiimotes();
        if (!WiimoteManager.HasWiimote()) return;

        // Setup red camera when at least one remote is detected
        if (!redIsSet) { 
            wiimoteRed = WiimoteManager.Wiimotes[0];
            wiimoteRed.SetupIRCamera(IRDataType.BASIC);
            redIsSet = true;
        }

        // Set up the blue camera when multiple remotes are detected
        if (!blueIsSet && WiimoteManager.Wiimotes.Count > 1)
        {
            wiimoteBlue = WiimoteManager.Wiimotes[1];
            wiimoteBlue.SetupIRCamera(IRDataType.BASIC);
            blueIsSet = true;
        }

        if (redIsSet)
        {
            collectWiimoteData(wiimoteRed);
            if (!wiimoteRed.Button.b) {
                redTime -= Time.deltaTime;
                updateCrosshairPosition(wiimoteRed, ir_pointerRed);
                if (redTime <= 0)
                {
                    redTimeOverPanel.gameObject.SetActive(true);
                    redTime = 0f;
                }
            }

            /*check if b button pressed and check accel data*/
            else
            {
                Vector3 accelData = GetAccelVector(wiimoteRed);
                Debug.Log(accelData.ToString());
                //throw some projectile
            }
        }
        if (blueIsSet)
        {
            collectWiimoteData(wiimoteBlue);
            if (!wiimoteBlue.Button.b)
            {
                blueTime -= Time.deltaTime;
                updateCrosshairPosition(wiimoteBlue, ir_pointerBlue);
                if (blueTime <= 0)
                {
                    blueTimeOverPanel.gameObject.SetActive(true);
                    blueTime = 0f;
                }
            }

            /*check if b button pressed and check accel data*/
            else
            {
                Vector3 accelData = GetAccelVector(wiimoteBlue);
                Debug.Log(accelData.ToString());
                //throw some projectile
            }
        }
        displayTime();
    }

    private void displayTime()
    {
        TimeSpan tRed = TimeSpan.FromSeconds(redTime);
        string timerFormattedRed = string.Format("{0:D2}:{1:D2}", tRed.Minutes, tRed.Seconds);
        redTimer.text = "Time left " + timerFormattedRed;

        TimeSpan tBlue = TimeSpan.FromSeconds(blueTime);
        string timerFormattedBlue = string.Format("{0:D2}:{1:D2}", tBlue.Minutes, tBlue.Seconds);
        blueTimer.text = "Time left " + timerFormattedBlue;
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

    private Vector3 GetAccelVector(Wiimote wiimote)
    {
        float accel_x;
        float accel_y;
        float accel_z;

        float[] accel = wiimote.Accel.GetCalibratedAccelData();
        accel_x = accel[0];
        accel_y = -accel[2];
        accel_z = -accel[1];

        return new Vector3(accel_x, accel_y, accel_z);
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
