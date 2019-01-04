using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEditor;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour {
    
    private GameObject currentItem;

    KeyValuePair<string, string> currentIngredient = new KeyValuePair<string, string>("", "");
    string currentStation = "-1";

    //NFC Stuff:
    public Text tag_output_text;

    private AndroidJavaObject mActivity;
    private AndroidJavaObject mIntent;
    private string sAction;
    private int lastTag = -1;

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        checkStation("0");
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log(currentIngredient);
        }
    }

    public void goToCupboard()
    {
        SceneManager.LoadScene("PickUpStation");
    }

    public void viewItems()
    {
        if (currentItem == null)
        {
            GameObject itemPrefab;
            switch (currentIngredient.Key)
            {
                case "Potato":
                    itemPrefab = (GameObject) Resources.Load("PotatoesPrefab", typeof(GameObject));
                    currentItem = (GameObject)Instantiate(itemPrefab, new Vector3(0, 0, 90), Quaternion.identity);
                    break;
                case "Vegetables":
                    itemPrefab = (GameObject) Resources.Load("VegetablesPrefab", typeof(GameObject));
                    currentItem = (GameObject)Instantiate(itemPrefab, new Vector3(0, 0, 85), Quaternion.identity);
                    break;
                case "Milk":
                    itemPrefab = (GameObject) Resources.Load("MilkPrefab", typeof(GameObject));
                    currentItem = (GameObject)Instantiate(itemPrefab, new Vector3(0, 0, 90), Quaternion.identity);
                    break;
                case "Eggs":
                    itemPrefab = (GameObject)Resources.Load("EggsPrefab", typeof(GameObject));
                    currentItem = (GameObject)Instantiate(itemPrefab, new Vector3(0, 0, 90), Quaternion.identity);
                    break;
                case "Noodles":
                    itemPrefab = (GameObject)Resources.Load("NoodlesPrefab", typeof(GameObject));
                    currentItem = (GameObject)Instantiate(itemPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                    break;
                default:
                    break;
            }
        }
        else
        {
            Destroy(currentItem);
        }
    }

    private void pickUpStation()
    {
        string ingredientPicked = ARCupboard.ingredient;
        currentIngredient = new KeyValuePair<string, string>(ingredientPicked, "uncooked");
    }

    private void checkStation(string text)
    {
        if (text != currentStation)
        {
            switch(text)
            {
                case "0":
                    pickUpStation();
                    currentStation = "0";
                    break;
                default:
                    break;  
            }
        }
    }

    private void checkNFC()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                mActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
                mIntent = mActivity.Call<AndroidJavaObject>("getIntent");
                sAction = mIntent.Call<String>("getAction");
                if (sAction == "android.nfc.action.NDEF_DISCOVERED")
                {
                    tag_output_text.text = "NDEF tag";
                }
                else if (sAction == "android.nfc.action.TECH_DISCOVERED")
                {
                    AndroidJavaObject[] mNdefMessage = mIntent.Call<AndroidJavaObject[]>("getParcelableArrayExtra", "android.nfc.extra.NDEF_MESSAGES");
                    AndroidJavaObject[] mNdefRecord = mNdefMessage[0].Call<AndroidJavaObject[]>("getRecords");
                    byte[] payLoad = mNdefRecord[0].Call<byte[]>("getPayload");

                    if (mNdefMessage != null)
                    {
                        string text = System.Text.Encoding.UTF8.GetString(payLoad).Substring(3);
                        int j = -1;
                        Int32.TryParse(text, out j);
                        // if (Int32.TryParse(text, out j)) tag_output_text.text = "Tag value: " + j;
                        // else tag_output_text.text = "Could not parse tag for text: " + text;

                        if (j != lastTag)
                        {

                            checkStation("0");

                            lastTag = j;
                        }
                    }
                    else
                    {
                        tag_output_text.text = "No ID found !";
                    }
                    mIntent.Call("removeExtra", "android.nfc.extra.TAG");
                    return;
                }
                else if (sAction == "android.nfc.action.TAG_DISCOVERED")
                {
                    tag_output_text.text = "Tag not supported";
                }
                else
                {
                    tag_output_text.text = "Scan a NFC tag...";
                    return;
                }
            }
            catch (Exception ex)
            {
                string text = ex.Message;
                tag_output_text.text = text;
            }
        }
    }
}
