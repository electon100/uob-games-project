﻿using System;
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
    private GameObject networkClient;
    private Client network;

    private string currentStation = "-1";

    /* Current ingredient that the player is holding
       -> Can be used externally */
    public static Ingredient currentIngred;
    public Text mainText;
    private int numberOfScans = 0;

    //NFC Stuff:
    public Text tag_output_text;
    private AndroidJavaObject mActivity;
    private AndroidJavaObject mIntent;
    private string sAction;
    private int lastTag = -1;

    void Start () {
        networkClient = GameObject.Find("Client");
        network = networkClient.GetComponent<Client>();
    }

	void Update () {

        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log(currentIngred);
        }
        if (currentItem != null)
        {
            currentItem.transform.Rotate(0, Time.deltaTime*20, 0);
        }

        checkNFC();
    }

    public void goToFrying()
    {
        SceneManager.LoadScene("PanStation");
    }

    public void goToCupboard()
    {
        SceneManager.LoadScene("PickUpStation");
    }

    public void viewItems()
    {
        if (currentItem == null)
        {
            currentItem = (GameObject)Instantiate(currentIngred.Model, new Vector3(0, 0, 80), Quaternion.identity);
        }
        else
        {
            Destroy(currentItem);
        }
    }

    private void pickUpStation() {
        currentIngred = ARCupboard.ingredient;
    }

    private void checkStation(string text)
    {

        //network.SendMyMessage("station", text);
        if (text != currentStation)
        {
            switch (text)
            {
                case "0":
                    pickUpStation();
                    currentStation = "0";
                    text += "$" + currentIngred.Name;
                    Debug.Log(text);
                    network.SendMyMessage("station", text);
                    break;
                case "1":
                    pickUpStation();
                    currentStation = "1";
                    text += "$" + currentIngred.Name;
                    Debug.Log(text);
                    network.SendMyMessage("station", text);
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
                            checkStation(text);
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
