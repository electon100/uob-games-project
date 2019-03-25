using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WiimoteApi;

public class WiimoteBehaviourRed : MonoBehaviour {

    private Wiimote wiimoteRed;
    public RectTransform redCrosshair;
    bool redIsSet;
    public Text redTimer;
    private float redTime;
    private bool gamestarted;
    private bool aPressed;
    private float countdown;
    private bool firstTime;

    public Transform mainPanel;
    public Transform redTimeOverPanel;
    public Transform RedStartPanel;
    public Text redStartText;
    public Text redResultText;

    public GameObject redProjectile;
    private Vector3 targetVector;
    public float force;
    public float forcez;
    public bool redfired = false;
    private int ammoCount = 0;

    Ingredient chicken = new Ingredient("chicken", "chickenPrefab");


    // Use this for initialization
    void Start () {
        gamestarted = false;
        redIsSet = false;
        aPressed = false;
        firstTime = false;
        redTime = 5.0f;
        DisplayTime();
    }

    // Update is called once per frame
    void Update() {
        WiimoteManager.FindWiimotes();
        if (!WiimoteManager.HasWiimote()) return;

        // Setup red camera when at least one remote is detected
        if (!redIsSet)
        {
            wiimoteRed = WiimoteManager.Wiimotes[0];
            redIsSet = wiimoteRed.SetupIRCamera(IRDataType.BASIC);
            reset(chicken);
        }

        if(!gamestarted){
            StartScreen();
        }
        else{
            if (redIsSet)
            {
                CollectWiimoteData(wiimoteRed);

                if (!wiimoteRed.Button.b && redTime > 0)
                {
                    redTime -= Time.deltaTime;
                    UpdateCrosshairPosition(wiimoteRed, redCrosshair);
                    if (redTime <= 0)
                    {
                        roundOver("Too late! Times up");
                    }
                }

                /*check if b button pressed and check accel data*/
                else if(ammoCount > 0)
                {
                    // Debug.Log("Red B button pressed");
                    Vector3 accelData = GetAccelVector(wiimoteRed);
                    // Debug.Log(accelData.ToString());
                    if ((accelData.x < -3.0f || accelData.y < -3.0f) && !redfired)
                    {
                        targetVector.y = redCrosshair.anchorMin.y - 0.5f;
                        targetVector.x = redCrosshair.anchorMin.x - 0.5f;

                        Transform ingredTransform = redProjectile.GetComponentsInChildren<Transform>(true)[0];
                        Quaternion ingredRotation = ingredTransform.rotation;
                        Vector3 ingredPosition = new Vector3(-11, 10, 0);
                        
                        GameObject foodBullet = Instantiate(redProjectile, ingredPosition, ingredRotation) as GameObject;
                        if (foodBullet.GetComponent<Rigidbody>() == null){
                            foodBullet.AddComponent<Rigidbody>();
                        }
                        if(foodBullet.GetComponent<SphereCollider>() == null){
                            foodBullet.AddComponent<SphereCollider>();
                        }
                        if(foodBullet.GetComponent<ProjectileBehaviour>() == null){
                            foodBullet.AddComponent<ProjectileBehaviour>();
                        }
                        // GameObject foodBullet = Instantiate(redProjectile, redProjectile.transform.position, Quaternion.identity) as GameObject;
                        foodBullet.name = "RedProjectile";
                        foodBullet.GetComponent<Rigidbody>().useGravity = true;
                        foodBullet.GetComponent<SphereCollider>().radius = 0.01f;
                        ScaleProjectile(foodBullet);
                        Debug.Log(foodBullet.transform.localScale);
                        foodBullet.GetComponent<Rigidbody>().AddForce(force, targetVector.y * force, -targetVector.x * forcez);
                        redfired = true;
                        ammoCount -= 1;
                    }
                }
            }

            DisplayTime();
        }

        
    }

    void StartScreen()
    {
        CollectWiimoteData(wiimoteRed);
        if(wiimoteRed.Button.a && !aPressed && firstTime){
            // Check if player has ammo
            // Display no ammo or set ammo count to 1
            CheckAmmo();
            countdown = 4;
            aPressed = true;
        }

        if (aPressed){
            TimeSpan tcountdown = TimeSpan.FromSeconds(countdown);
            string countdownFormated = string.Format("{0:D2}", tcountdown.Seconds);
            redStartText.text = countdownFormated;
            // Debug.Log("countdown reducing");
            countdown -= Time.deltaTime;
        }          

        if(aPressed && countdown < 0){
            gamestarted = true;
            mainPanel.gameObject.SetActive(true);
            RedStartPanel.gameObject.SetActive(false);
        }
    }

    private bool CheckAmmo(){
        ammoCount += 1;
        redfired = false;
        return true;
    }

    private void DisplayTime()
    {
        TimeSpan tRed = TimeSpan.FromSeconds(redTime);
        string timerFormattedRed = string.Format("{0:D2}:{1:D2}", tRed.Minutes, tRed.Seconds);
        redTimer.text = "Time left " + timerFormattedRed;
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
        redResultText.text = text;
        redTime = 0;
        redTimeOverPanel.gameObject.SetActive(true);
    }

    public void reset(Ingredient ingredient)
    {
        firstTime = true;
        redTime = 5.0f;
        gamestarted = false;
        aPressed = false;
        redTimeOverPanel.gameObject.SetActive(false);
        mainPanel.gameObject.SetActive(false);
        RedStartPanel.gameObject.SetActive(true);
        redStartText.text = "Press -a- to start game";
        redProjectile = (GameObject) Resources.Load(ingredient.Model, typeof(GameObject));
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
        if (wiimoteRed != null)
        {
            WiimoteManager.Cleanup(wiimoteRed);
            wiimoteRed = null;
        }
    }
}
