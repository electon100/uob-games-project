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

    private GameObject networkClient;
    private Client network;

    /*Current station:
    -1 - Main Screen
    0  - Cupboard
    1  - Frying
    2  - Chopping
    3  - Plating*/
    public static string currentStation = "-1";

    /* Current ingredient that the player is holding
       -> Can be used externally */
    public static Ingredient currentIngred;
    public static List<Ingredient> ingredientsFromStation;
    public Text mainText;
    public GameObject mainPanel;

    //NFC Stuff:
    public Text tag_output_text;
    private NFCHandler nfcHandler = new NFCHandler();

    void Start () {
        Screen.orientation = ScreenOrientation.Portrait;
        networkClient = GameObject.Find("Client");
        network = networkClient.GetComponent<Client>();
    }

    void Awake() {
        DontDestroyOnLoad(GameObject.Find("Player"));
    }

	void Update () {
        //Testing on computer/////////////////
        if (Input.GetKeyDown(KeyCode.R))
        {
            checkStation("0");
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            checkStation("1");
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            checkStation("2");
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            checkStation("3");
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log(Player.currentIngred.Model);
        }

        /////////////////////////////////////

        /* Check for any NFC scans, forwarding to checkStation if present */
        string lastTag = nfcHandler.getScannedTag();
        if (lastTag != "-1") {
            checkStation(lastTag);
        }
    }

    private void cupboardStation()
    {
        ingredientsFromStation = network.getIngredientsFromStation("0");
        SceneManager.LoadScene("CupboardStation");
    }

    private void fryingStation()
    {
        ingredientsFromStation = network.getIngredientsFromStation("1");
        SceneManager.LoadScene("FryingStation");
    }

    private void choppingStation()
    {
        ingredientsFromStation = network.getIngredientsFromStation("2");
        SceneManager.LoadScene("ChoppingStation");
    }

    private void platingStation()
    {
        ingredientsFromStation = network.getIngredientsFromStation("3");
        SceneManager.LoadScene("PlatingStation");
    }

    private string sendCurrentIngredient(Ingredient addedIngredient)
    {
        string message;
        if (addedIngredient != null)
        {
            message = "$" + Ingredient.SerializeObject(addedIngredient);
        }
        else
        {
            message = "$";
        }

        return message;
    }

    public void notifyServerAboutIngredientPlaced(Ingredient ingredient)
    {
        string message;
        message = currentStation + sendCurrentIngredient(ingredient);
        network.SendMyMessage("station", message);
    }

    public void clearIngredientsInStation(string stationID) {
        ingredientsFromStation.Clear();
        network.myKitchen[stationID].Clear();
        network.SendMyMessage("clear", stationID);
    }

    public void notifyAboutStationLeft(string stationID) {
        network.SendMyMessage("leave", stationID);
        resetCurrentStation();
    }

    public static void removeCurrentIngredient()
    {
        currentIngred = null;
    }

    public static bool isHoldingIngredient()
    {
        return currentIngred != null;
    }

    private void resetCurrentStation() {
        currentStation = "-1";
    }

    public void sendScoreToServer(Ingredient recipe) {
        string message = Ingredient.SerializeObject(recipe);
        network.SendMyMessage("score", message);
    }

    private void checkStation(string text)
    {
        if (currentStation != text)
        {
            // mainText.text = "Logging into station " + text + "...";
            switch (text)
            {
                case "0":
                    currentStation = text;
                    //Tell server you've logged into the station, holding that food item
                    text += sendCurrentIngredient(null);
                    network.SendMyMessage("station", text);
                    break;
                case "1":
                    currentStation = text;
                    //Tell server you've logged into the station, holding that food item
                    text += sendCurrentIngredient(null);
                    network.SendMyMessage("station", text);
                    break;
                case "2":
                    currentStation = text;
                    //Tell server you've logged into the station, holding that food item
                    text += sendCurrentIngredient(null);
                    network.SendMyMessage("station", text);
                    break;
                case "3":
                    currentStation = text;
                    //Tell server you've logged into the station, holding that food item
                    text += sendCurrentIngredient(null);
                    network.SendMyMessage("station", text);
                    break;
                case "8":
                    // Join red team
                    if (!network.isConnected) {
                      network.Connect();
                    }
                    network.onClickRed();
                    break;
                case "9":
                    // Join blue team
                    if (!network.isConnected) {
                      network.Connect();
                    }
                    network.onClickBlue();
                    break;
                default:
                    currentStation = "-1";
                    break;
            }
        }
    }
}
