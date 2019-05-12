using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WiimoteApi;

public class WiimoteBehaviourBlue : MonoBehaviour {

    private Wiimote wiimoteBlue;
    public RectTransform blueCrosshair;
    bool blueIsSet;
    public Text blueTimer;
    private float blueTime;
    private bool gamestarted;
    private bool aPressed;
    private float countdown;
    private bool firstTime;

    public Transform mainPanel;
    public Transform blueTimeOverPanel;
    public Transform BlueStartPanel;
    public Text blueStartText;
    public Text blueResultText;

    public GameObject blueProjectile;
    private Vector3 targetVector;
    public float forcex;
    public float forcey;
    public float forcez;
    public bool bluefired = false;
    private int ammoCount = 0;

    // Use this for initialization
    void Start () {
        gamestarted = false;
        blueIsSet = false;
        aPressed = false;
        firstTime = false;
        blueTime = 20.0f;
        DisplayTime();
    }

    // Update is called once per frame
    void Update() {
        WiimoteManager.FindWiimotes();
        if (!WiimoteManager.HasWiimote()) return;

        // Setup blue camera when at least one remote is detected
        if (!blueIsSet)
        {
            wiimoteBlue = WiimoteManager.Wiimotes[1];
            blueIsSet = wiimoteBlue.SetupIRCamera(IRDataType.BASIC);
        }

        if(!gamestarted){
            StartScreen();
        }
        else{
            if (blueIsSet)
            {
                CollectWiimoteData(wiimoteBlue);

                if (!wiimoteBlue.Button.a && blueTime > 0)
                {
                    blueTime -= Time.deltaTime;
                    UpdateCrosshairPosition(wiimoteBlue, blueCrosshair);
                    if (blueTime <= 0)
                    {
                        roundOver("Too late! Times up");
                        ammoCount = 0;
                    }
                }

                /*check if b button pressed and check accel data*/
                else if(ammoCount > 0)
                {
                    // Debug.Log("Blue B button pressed");
                    Vector3 accelData = GetAccelVector(wiimoteBlue);
                    // Debug.Log(accelData.ToString());
                    if ((accelData.x < -3.0f || accelData.y < -3.0f) && !bluefired)
                    {
                        targetVector.y = blueCrosshair.anchorMin.y - 0.5f;
                        targetVector.x = blueCrosshair.anchorMin.x - 0.5f;

                        if (targetVector.y < 0.0f) targetVector.y = 0.0f; //cap it for calibration

                        Transform ingredTransform = blueProjectile.GetComponentsInChildren<Transform>(true)[0];
                        Quaternion ingredRotation = ingredTransform.rotation;
                        Vector3 ingredPosition = new Vector3(12, 9.66f, -1.5f);

                        GameObject foodBullet = Instantiate(blueProjectile, ingredPosition, ingredRotation) as GameObject;
                        if (foodBullet.GetComponent<Rigidbody>() == null){
                            foodBullet.AddComponent<Rigidbody>();
                        }
                        if(foodBullet.GetComponent<SphereCollider>() == null){
                            foodBullet.AddComponent<SphereCollider>();
                        }
                        if(foodBullet.GetComponent<ProjectileBehaviour>() == null){
                            foodBullet.AddComponent<ProjectileBehaviour>();
                        }
                        // GameObject foodBullet = Instantiate(blueProjectile, blueProjectile.transform.position, Quaternion.identity) as GameObject;
                        foodBullet.name = "BlueProjectile";
                        foodBullet.GetComponent<Rigidbody>().useGravity = true;
                        foodBullet.GetComponent<SphereCollider>().radius = 0.01f;
                        ScaleProjectile(foodBullet);
                        foodBullet.GetComponent<Rigidbody>().AddForce(-forcex, targetVector.y * forcey, targetVector.x * forcez);
                        bluefired = true;
                        ammoCount -= 1;
                    }
                }
            }

            DisplayTime();
        }

        
    }

    void StartScreen()
    {
        CollectWiimoteData(wiimoteBlue);
        if(wiimoteBlue.Button.a && !aPressed && firstTime){
            // Check if player has ammo
            // Display no ammo or set ammo count to 1
            CheckAmmo();
            countdown = 4;
            aPressed = true;
        }

        if (aPressed){
            BlueStartPanel.gameObject.SetActive(false);
            TimeSpan tcountdown = TimeSpan.FromSeconds(countdown);
            string countdownFormated = string.Format("{0:D2}", tcountdown.Seconds);
            blueStartText.text = countdownFormated;
            // Debug.Log("countdown reducing");
            countdown -= Time.deltaTime;
        }          

        if(aPressed && countdown < 0){
            blueStartText.text = "";
            gamestarted = true;
            mainPanel.gameObject.SetActive(true);
        }
    }

    private bool CheckAmmo(){
        ammoCount += 1;
        bluefired = false;
        return true;
    }

    private void DisplayTime()
    {
        TimeSpan tBlue = TimeSpan.FromSeconds(blueTime);
        string timerFormattedBlue = string.Format("{0:D2}:{1:D2}", tBlue.Minutes, tBlue.Seconds);
        blueTimer.text = "Time left " + timerFormattedBlue;
    }

    private void UpdateCrosshairPosition(Wiimote wiimote, RectTransform irPointer)
    {
        float[] pointer = wiimote.Ir.GetPointingPosition();
        irPointer.anchorMin = new Vector2(pointer[0], pointer[1]);
        irPointer.anchorMax = new Vector2(pointer[0], pointer[1]);
    }

    private void CollectWiimoteData(Wiimote wiimote)
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

    public void roundOver(String text)
    {
        blueResultText.text = text;
        blueTime = 0;
        blueTimeOverPanel.gameObject.SetActive(true);
    }

    public void reset(Ingredient ingredient)
    {
        firstTime = true;
        blueTime = 20.0f;
        gamestarted = false;
        aPressed = false;
        blueTimeOverPanel.gameObject.SetActive(false);
        mainPanel.gameObject.SetActive(false);
        BlueStartPanel.gameObject.SetActive(true);
        blueStartText.text = "";
        blueProjectile = (GameObject) Resources.Load(ingredient.Model, typeof(GameObject));
    }

    public void gameReset()
    {
        blueIsSet = false;
        firstTime = false;
        blueTime = 20.0f;
        gamestarted = false;
        aPressed = false;
        blueTimeOverPanel.gameObject.SetActive(false);
        mainPanel.gameObject.SetActive(false);
        BlueStartPanel.gameObject.SetActive(true);
        blueStartText.text = "";
        DisplayTime();
    }

    float scaleX;
    float scaleY;
    float scaleZ;

    private void ScaleProjectile(GameObject projectile){
        scaleX = projectile.transform.localScale.x; 
        scaleY = projectile.transform.localScale.y; 
        scaleZ = projectile.transform.localScale.z;
        
        projectile.transform.localScale = new Vector3(scaleX*0.3f, scaleY*0.3f, scaleZ*0.3f);
    }

    private void OnApplicationQuit()
    {
        if (wiimoteBlue != null)
        {
            WiimoteManager.Cleanup(wiimoteBlue);
            wiimoteBlue = null;
        }
    }
}
