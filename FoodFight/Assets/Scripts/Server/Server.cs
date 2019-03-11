﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Server : MonoBehaviour {
    private bool isStarted = false;

    // Spawning and movement
    public GameObject redPlayer;
    public GameObject bluePlayer;

    public GameObject blueStation;
    public GameObject redStation;

    // Scoring and Timing
    public Manager gameManager;
    public static float finalRedScore, finalBlueScore;

    // Networking
    public NetManager netManager;
    public Manager manager;

    public static GameEndState gameEndState;

    // Dictionaries of players on each team
    IDictionary<int, GameObject> redTeam = new Dictionary<int, GameObject>();
    IDictionary<int, GameObject> blueTeam = new Dictionary<int, GameObject>();

    // dictionary <station, status>
    IDictionary<string, List<Ingredient>> redKitchen = new Dictionary<string, List<Ingredient>>();
    IDictionary<string, List<Ingredient>> blueKitchen = new Dictionary<string, List<Ingredient>>();

    IDictionary<string, GameObject> redOccupied = new Dictionary<string, GameObject>();
    IDictionary<string, GameObject> blueOccupied = new Dictionary<string, GameObject>();

    private string[] stations = {"0","1","2","3"};

    int redIdleCount = 0;
    int blueIdleCount = 0;

    private void Start () {
        isStarted = true;

        manager = GameObject.Find("Manager").GetComponent<Manager>();
        netManager = GameObject.Find("NetManager").GetComponent<NetManager>();
    }

	  private void Update () {
	  }

    // This is where all the work happens.
    public void manageMessageEvents(string message, int connectionId)
    {
        string messageType = decodeMessage(message, '&')[0];
        string messageContent = decodeMessage(message, '&')[1];

        switch(messageType)
        {
            // Player chooses team to play on
            case "connect":
                // Allocate the player to the team if they are not already on a team
                if (!redTeam.ContainsKey(connectionId) && !blueTeam.ContainsKey(connectionId)) {
                    allocateToTeam(connectionId, messageContent);
                    netManager.SendMyMessage("team", getPlayersTeam(connectionId), connectionId);
                }
                break;
            // Player connects to a work station
            case "station":
                OnStation(messageContent, connectionId);
                break;
            // Player sends NFC data
            case "clear":
                OnClearStation(messageContent, connectionId);
                break;
            case "NFC":
                //Do NFC stuff
                Debug.Log("Player " + connectionId + " has sent: " + messageContent);
                break;
            // Player sends recipe to score
            case "score":
                OnScore(messageContent, connectionId);
                break;
            case "leave":
                OnLeave(messageContent, connectionId);
                break;
        }
    }

    private void OnScore(string messageContent, int connectionId) {
        Ingredient recipe = Ingredient.XmlDeserializeFromString<Ingredient>(messageContent, (new Ingredient()).GetType());

        int recipeScore = FoodData.Instance.getScoreForIngredient(recipe);

        // Add score to red team
        if (redTeam.ContainsKey(connectionId)) manager.increaseRed(recipeScore);
        // Add score to blue team
        else if (blueTeam.ContainsKey(connectionId)) manager.increaseBlue(recipeScore);

        Debug.Log(messageContent);
    }

    private void OnLeave(string messageContent, int connectionId)
    {
        string stationId = messageContent;
        if (redTeam.ContainsKey(connectionId) && redOccupied.ContainsKey(stationId))
        {
            redOccupied[stationId] = null;
            redIdleCount += 1;
            Vector3 newPosition = new Vector3(-40, 2, 5 * (redIdleCount + 1));
            PlayerMovement.movePlayer(newPosition, redTeam[connectionId]);
        }
        if (blueTeam.ContainsKey(connectionId) && blueOccupied.ContainsKey(stationId))
        {
            blueOccupied[stationId] = null;
            blueIdleCount += 1;
            Vector3 newPosition = new Vector3(-40, 2, 5 * (blueIdleCount + 1));
            PlayerMovement.movePlayer(newPosition, blueTeam[connectionId]);
        }
    }

    private void OnClearStation(string stationId, int connectionId) {
        clearStationInKitchen(connectionId, stationId);
        sendIngredientsToPlayer(stationId, connectionId);
    }

    private void OnStation(string messageContent, int connectionId)
    {
        //If this station already exists, check what's in it and send it back to player.
        string[] words = decodeMessage(messageContent, '$');
        string stationId = words[0];

        moveServerPlayer(connectionId, stationId);

        string ingredientWithFlags = words[1];

        // Be aware of null value here. Shouldn't cause issues, but might
        Ingredient ingredientToAdd = new Ingredient();
        string ingredient = "";

        /* If a player sends back a list of ingredients, or another message, deal with that */
        if (!ingredientWithFlags.Equals(""))
        {
            ingredientToAdd = Ingredient.XmlDeserializeFromString<Ingredient>(ingredientWithFlags, ingredientToAdd.GetType());
            ingredient = ingredientToAdd.Name;
            Debug.Log("Ingredient to add: " + ingredient);
        }

        // Case where we add a station to a kitchen if it has not been seen before
        addStationToKitchen(stationId, connectionId);

        bool playerOnValidStation = isPlayerOnValidStation(connectionId, stationId);

        if (playerOnValidStation)
        {
            if (redTeam.ContainsKey(connectionId)) redIdleCount -= 1;
            if (blueTeam.ContainsKey(connectionId)) blueIdleCount -= 1;
            // Case where we want to send back ingredients stored at the station to player
            if (ingredient.Equals(""))
                sendIngredientsToPlayer(stationId, connectionId);

            //If the player wants to add an ingredient, add it
            else
                addIngredientToStation(stationId, ingredientToAdd, connectionId);
        }
    }

    private bool isPlayerOnValidStation(int connectionId, string stationId)
    {
        if (redTeam.ContainsKey(connectionId) && redKitchen.ContainsKey(stationId))
        {
            if (redOccupied[stationId] == redTeam[connectionId]) return true;
            else if (redOccupied[stationId] == null)
            {
                redOccupied[stationId] = redTeam[connectionId];
                Debug.Log("Red Station now occupied");
                return true;
            }
            else return false;
        }
        else if (blueTeam.ContainsKey(connectionId) && blueKitchen.ContainsKey(stationId))
        {
            if (blueOccupied[stationId] == blueTeam[connectionId]) return true;
            else if (blueOccupied[stationId] == null)
            {
                blueOccupied[stationId] = blueTeam[connectionId];
                Debug.Log("Blue Station now occupied");
                return true;
            }
            else return false;
        }
        return false;
    }

    // Add station to correct kitchen if it does not exist
    private void addStationToKitchen(string stationId, int connectionId)
    {
        if (redTeam.ContainsKey(connectionId))
        {
            if (!redKitchen.ContainsKey(stationId))
            {
                redKitchen.Add(stationId, new List<Ingredient>());
                redOccupied.Add(stationId, null);
                Debug.Log("Red Station Created");
            }
        }
        else if (blueTeam.ContainsKey(connectionId))
        {
            if (!blueKitchen.ContainsKey(stationId))
            {
                blueKitchen.Add(stationId, new List<Ingredient>());
                blueOccupied.Add(stationId, null);
                Debug.Log("Blue Station Created");
            }
        }
    }

    private void sendIngredientsToPlayer(string stationId, int connectionId)
    {
        if (redKitchen.ContainsKey(stationId) && redTeam.ContainsKey(connectionId))
            checkCurrentIngredient("station", "red", stationId, connectionId);

        else if (blueKitchen.ContainsKey(stationId) && blueTeam.ContainsKey(connectionId))
            checkCurrentIngredient("station", "blue", stationId, connectionId);
    }

    // Add to a station if it exists
    private void addIngredientToStation(string stationId, Ingredient ingredientToAdd, int connectionId)
    {
        if (redKitchen.ContainsKey(stationId) && redTeam.ContainsKey(connectionId))
        {
            Debug.Log("Adding ingredient to red");
            AddIngredientToList(stationId, ingredientToAdd, "red");
            checkCurrentIngredient("station", "red", stationId, connectionId);
        }

        else if (blueKitchen.ContainsKey(stationId) && blueTeam.ContainsKey(connectionId))
        {
            Debug.Log("Adding ingredient to blue");
            AddIngredientToList(stationId, ingredientToAdd, "blue");
            checkCurrentIngredient("station", "blue", stationId, connectionId);
        }
    }

    //Allocates a player to a team based on their choice.
    private void allocateToTeam(int connectionId, string message)
    {
        if (message == "red")
            createRedPlayer(connectionId);

        else if (message == "blue")
            createBluePlayer(connectionId);
    }

    // Splits up a string based on a given character
    private string[] decodeMessage(string message, char character)
    {
        string[] splitted = message.Split(character);
        return splitted;
    }

    private void createRedPlayer(int connectionId)
    {
        GameObject newRedPlayer = (GameObject) Instantiate(redPlayer, new Vector3(-40, 2, 5 * (redTeam.Count + 1)), Quaternion.identity);
        redTeam.Add(connectionId, newRedPlayer);
        redIdleCount += 1;
    }

    private void createBluePlayer(int connectiondId)
    {
        GameObject newBluePlayer = (GameObject) Instantiate(bluePlayer, new Vector3(40, 2, 5 * (blueTeam.Count + 1)), Quaternion.identity);
        blueTeam.Add(connectiondId, newBluePlayer);
        blueIdleCount += 1;
    }

    private void destroyPlayer(IDictionary<int, GameObject> team, int connectionID)
    {
        Destroy(team[connectionID]);
        team.Remove(connectionID);
        if (redTeam.ContainsKey(connectionID)) redIdleCount -= 1;
        if (blueTeam.ContainsKey(connectionID)) blueIdleCount -= 1;
    }

    private IDictionary<int, GameObject> getTeam(int connectionID)
    {
        if (redTeam.ContainsKey(connectionID))
            return redTeam;

        else if (blueTeam.ContainsKey(connectionID))
            return blueTeam;

        return null;
    }

    private string getPlayersTeam(int connectionId) {
        if(redTeam.ContainsKey(connectionId)) {
          return "red";
        } else if(blueTeam.ContainsKey(connectionId)) {
          return "blue";
        } else {
          return "none";
        }
    }

    private void checkCurrentIngredient(string messageType, string kitchen, string station, int hostId)
    {
        if (kitchen == "red")
        {
            string messageContent = station + "$";
            foreach (Ingredient ingredient in redKitchen[station])
            {
                messageContent += Ingredient.SerializeObject(ingredient);
                messageContent += "$";
            }

            Debug.Log("Sending back to red: " + messageContent);
            netManager.SendMyMessage(messageType, messageContent, hostId);
        }
        else if (kitchen == "blue")
        {
            string messageContent = station + "$";
            foreach (Ingredient ingredient in blueKitchen[station])
            {
                messageContent += Ingredient.SerializeObject(ingredient);
                messageContent += "$";
            }

            Debug.Log("Sending back to blue: " + messageContent);
            netManager.SendMyMessage(messageType, messageContent, hostId);
        }
    }

    private void AddIngredientToList(string stationId, Ingredient newIngredient, string kitchen)
    {
        if (kitchen == "red")
            redKitchen[stationId].Add(newIngredient);

        else if (kitchen == "blue")
            blueKitchen[stationId].Add(newIngredient);
    }

    private void clearStationInKitchen(int connectionID, string stationID) {
        if (redTeam.ContainsKey(connectionID)) {
            redKitchen[stationID].Clear();
        }
        else if (blueTeam.ContainsKey(connectionID)) {
            blueKitchen[stationID].Clear();
        }
    }

    private void clearAllStations() {
      foreach(string station in stations) {
        if (redKitchen.ContainsKey(station)) redKitchen[station].Clear();
        if (blueKitchen.ContainsKey(station)) blueKitchen[station].Clear();
      }
    }

    private void sendEndGame() {
      string endGameString = manager.getEndGameString();

      foreach(KeyValuePair<int, GameObject> player in redTeam) {
          netManager.SendMyMessage("endgame", endGameString, player.Key);
      }

      foreach(KeyValuePair<int, GameObject> player in blueTeam) {
          netManager.SendMyMessage("endgame", endGameString, player.Key);
      }
    }

    public void EndGame() {
      clearAllStations();
      sendEndGame();
    }

    private void moveServerPlayer(int connectionId, string stationId)
    {
        string stationText = "";
        if (redTeam.ContainsKey(connectionId))
        {
            stationText = "RedStation" + stationId;
            redStation = GameObject.Find(stationText);
            Vector3 newPosition = redStation.transform.position;
            newPosition.x -= 10.0f;
            PlayerMovement.movePlayer(newPosition, redTeam[connectionId]);
        }
        else if (blueTeam.ContainsKey(connectionId))
        {
            stationText = "BlueStation" + stationId;
            blueStation = GameObject.Find(stationText);
            Vector3 newPosition = blueStation.transform.position;
            newPosition.x += 10.0f;
            PlayerMovement.movePlayer(newPosition, blueTeam[connectionId]);
        }
    }
}