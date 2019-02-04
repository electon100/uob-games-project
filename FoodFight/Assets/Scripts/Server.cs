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
    private const int MAX_CONNECTION = 10;

    private int port = 8000;

    private int hostId;
    private int webHostId;

    private int reliableChannel;
    private int unreliableChannel;

    private bool isStarted = false;

    private byte error;

    public GameObject redPlayer;
    public GameObject bluePlayer;

    private Score redScore;
    private Score blueScore;

    public static float finalRedScore;
    public static float finalBlueScore;

    // Dictionaries of players on each team
    IDictionary<int, GameObject> redTeam = new Dictionary<int, GameObject>();
    IDictionary<int, GameObject> blueTeam = new Dictionary<int, GameObject>();

    // dictionary <station, status>
    IDictionary<string, List<Ingredient>> redKitchen = new Dictionary<string, List<Ingredient>>();
    IDictionary<string, List<Ingredient>> blueKitchen = new Dictionary<string, List<Ingredient>>();

    // Timer variable
    private float timer;
    public Text timerText;

    private void Start () {
        NetworkTransport.Init();
        ConnectionConfig connectConfig = new ConnectionConfig();

        /* Network configuration */
        connectConfig.AckDelay = 33;
        connectConfig.AllCostTimeout = 20;
        connectConfig.ConnectTimeout = 1000;
        connectConfig.DisconnectTimeout = 2000;
        connectConfig.FragmentSize = 500;
        connectConfig.MaxCombinedReliableMessageCount = 10;
        connectConfig.MaxCombinedReliableMessageSize = 100;
        connectConfig.MaxConnectionAttempt = 32;
        connectConfig.MaxSentMessageQueueSize = 2048;
        connectConfig.MinUpdateTimeout = 20;
        connectConfig.NetworkDropThreshold = 40; // we had to set these high to avoid UNet disconnects during lag spikes
        connectConfig.OverflowDropThreshold = 40; // 
        connectConfig.PacketSize = 1500;
        connectConfig.PingTimeout = 500;
        connectConfig.ReducedPingTimeout = 100;
        connectConfig.ResendTimeout = 500;

        reliableChannel = connectConfig.AddChannel(QosType.ReliableSequenced);
        HostTopology topo = new HostTopology(connectConfig, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo, port, null /*ipAddress*/);
        webHostId = NetworkTransport.AddWebsocketHost(topo, port, null /*ipAddress*/);
        isStarted = true;

        redScore = new Score();
        blueScore = new Score();

        timerText = GameObject.Find("TimerText").GetComponent<Text>();
        timer = 300.0f;
        displayTime();
    }

	private void Update () {
        if (!isStarted) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            if (redScore.getScore() > blueScore.getScore()) GameOver("red");
            else if (blueScore.getScore() > redScore.getScore()) GameOver("blue");
            // Defaults to red winning if it is a tie
            else GameOver("red");
        }
        displayTime();

        // Check if either team has reached a score of 0 and if they have, end the game
        if (redScore.getScore() == 0) GameOver("blue");
        else if (blueScore.getScore() == 0) GameOver("red");

        int recHostId; // Player ID
        int connectionId; // ID of connection to recHostId.
        int channelID; // ID of channel connected to recHostId;
        byte[] recBuffer = new byte[4096];
        int bufferSize = 4096;
        int dataSize;
        byte error;

        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelID,
                                                            recBuffer, bufferSize, out dataSize, out error);

        //Networking events
        switch (recData)
        {
            // Do nothing if nothing was sent to server
            case NetworkEventType.Nothing:
                break;
            // Have a phone connect to the server
            case NetworkEventType.ConnectEvent:
                Debug.Log("Player " + connectionId + " has connected");
                break;
            // Have the phone send data to the server
            case NetworkEventType.DataEvent:
                string message = OnData(hostId, connectionId, channelID, recBuffer, bufferSize, (NetworkError)error);
                manageMessageEvents(message, connectionId);
                break;
            // Remove the player from the game
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Player " + connectionId + " has disconnected");
                IDictionary<int, GameObject> teamToDestroyFrom = getTeam(connectionId);
                // Player with id connectionId has left the game, so destroy its object instance.
                if (teamToDestroyFrom != null)
                {
                    destroyPlayer(teamToDestroyFrom, connectionId);
                }
                break;
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Broadcast event.");
                break;
        }

        return;
	}

    // This is where all the work happens.
    private void manageMessageEvents(string message, int connectionId)
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
                }
                break;
            // Player connects to a work station
            case "station":
                OnStation(messageContent, connectionId);
                break;
            // Player sends NFC data
            case "NFC":
                //Do NFC stuff
                Debug.Log("Player " + connectionId + " has sent: " + messageContent);
                break;
            // Player sends recipe to score
            case "score":
                OnScore(messageContent, connectionId);
                break;
        }
    }

    //This function is called when data is sent
    private string OnData(int hostId, int connectionId, int channelId, byte[] data, int size, NetworkError error)
    {
        //Here the message being received is deserialized and output to the console
        Stream serializedMessage = new MemoryStream(data);
        BinaryFormatter formatter = new BinaryFormatter();
        string message = formatter.Deserialize(serializedMessage).ToString();

        //Output the deserialized message as well as the connection information to the console
        Debug.Log("OnData(hostId = " + hostId + ", connectionId = "
            + connectionId + ", channelId = " + channelId + ", data = "
            + message + ", size = " + size + ", error = " + error.ToString() + ")");

        return message;
    }

    private void OnScore(string messageContent, int connectionId) {
        Ingredient recipe = new Ingredient();
        recipe = Ingredient.XmlDeserializeFromString<Ingredient>(messageContent, recipe.GetType());

        int recipeScore = scoreRecipe(recipe);

        if (redTeam.ContainsKey(connectionId)) {
          // Add score to red team
          redScore.increaseScore(recipeScore);
        } else if (blueTeam.ContainsKey(connectionId)) {
          // Add score to blue team
          blueScore.increaseScore(recipeScore);
        }

        Debug.Log(messageContent);
    }

    private int scoreRecipe(Ingredient recipe) {
      int score = 100;

      return score;
    }

    private void OnStation(string messageContent, int connectionId)
    {
        //If this station already exists, check what's in it and send it back to player.
        string[] words = decodeMessage(messageContent, '$');
        string stationId = words[0];

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
            // Case where we want to send back ingredients stored at the station to player
            if (ingredient.Equals("")) {
                sendIngredientsToPlayer(ingredient, stationId, connectionId);
            }
            // Case where the player wants to clear what's stored in the station
            else if (ingredient.Equals("clear")) {
                clearStationInKitchen(connectionId, stationId);
                sendIngredientsToPlayer(ingredient, stationId, connectionId);
            }
            //If the player wants to add an ingredient, add it
            else {
                addIngredientToStation(stationId, ingredientToAdd, connectionId);
            }
        }
    }

    private bool isPlayerOnValidStation(int connectionId, string stationId)
    {
        if (redTeam.ContainsKey(connectionId) && redKitchen.ContainsKey(stationId))
            return true;
        else if (blueTeam.ContainsKey(connectionId) && blueKitchen.ContainsKey(stationId))
            return true;
        else
            return false;
    }

    // Add station to correct kitchen if it does not exist
    private void addStationToKitchen(string stationId, int connectionId)
    {
        if (redTeam.ContainsKey(connectionId))
        {
            if (!redKitchen.ContainsKey(stationId) && !blueKitchen.ContainsKey(stationId))
                redKitchen.Add(stationId, new List<Ingredient>());
        }
        else if (blueTeam.ContainsKey(connectionId))
        {
            if (!blueKitchen.ContainsKey(stationId) && !redKitchen.ContainsKey(stationId))
                blueKitchen.Add(stationId, new List<Ingredient>());
        }
    }

    private void sendIngredientsToPlayer(string ingredient, string stationId, int connectionId)
    {
        if (redKitchen.ContainsKey(stationId))
            checkCurrentIngredient("station", "red", stationId, connectionId);

        else if (blueKitchen.ContainsKey(stationId))
            checkCurrentIngredient("station", "blue", stationId, connectionId);
    }

    // Add to a station if it exists
    private void addIngredientToStation(string stationId, Ingredient ingredientToAdd, int connectionId)
    {
        if (redKitchen.ContainsKey(stationId))
        {
            AddIngredientToList(stationId, ingredientToAdd, "red");
            checkCurrentIngredient("station", "red", stationId, connectionId);
        }

        else if (blueKitchen.ContainsKey(stationId))
        {
            AddIngredientToList(stationId, ingredientToAdd, "blue");
            checkCurrentIngredient("station", "blue", stationId, connectionId);
        }
    }

    // Used for sending data to the players
    public void SendMyMessage(string messageType, string textInput, int connectionId)
    {
        byte error;
        byte[] buffer = new byte[4096];
        int bufferSize = 4096;
        Stream message = new MemoryStream(buffer);
        BinaryFormatter formatter = new BinaryFormatter();
        //Serialize the message
        string messageToSend = messageType + "&" + textInput;
        formatter.Serialize(message, messageToSend);

        //Send the message from the "client" with the serialized message and the connection information
        NetworkTransport.Send(hostId, connectionId, reliableChannel, buffer, (int)message.Position, out error);

        //If there is an error, output message error to the console
        if ((NetworkError)error != NetworkError.Ok)
        {
            Debug.Log("Message send error: " + (NetworkError)error);
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
        GameObject newRedPlayer = (GameObject) Instantiate(redPlayer, new Vector3(-5*(redTeam.Count + 1), 2, 1 * (redTeam.Count + 1)), Quaternion.identity);
        redTeam.Add(connectionId, newRedPlayer);
    }

    private void createBluePlayer(int connectiondId)
    {
        GameObject newBluePlayer = (GameObject) Instantiate(bluePlayer, new Vector3(5 * (redTeam.Count + 1), 2, 1 * (redTeam.Count + 1)), Quaternion.identity);
        blueTeam.Add(connectiondId, newBluePlayer);
    }

    private void destroyPlayer(IDictionary<int, GameObject> team, int connectionID)
    {
        Destroy(team[connectionID]);
        team.Remove(connectionID);
    }

    private IDictionary<int, GameObject> getTeam(int connectionID)
    {
        if (redTeam.ContainsKey(connectionID))
            return redTeam;

        else if (blueTeam.ContainsKey(connectionID))
            return blueTeam;

        return null;
    }

    private void checkCurrentIngredient(string messageType, string kitchen, string station, int hostId)
    {
        if (kitchen == "red")
        {
            string messageContent = station + "$";
            foreach (Ingredient ingredient in redKitchen[station])
            {
                Debug.Log("Sending back: " + ingredient.Name);
                messageContent += Ingredient.SerializeObject(ingredient);
                messageContent += "$";
            }

            SendMyMessage(messageType, messageContent, hostId);
        }
        else if (kitchen == "blue")
        {
            string messageContent = station + "$";
            foreach (Ingredient ingredient in blueKitchen[station])
            {
                messageContent += Ingredient.SerializeObject(ingredient);
                messageContent += "$";
            }

            SendMyMessage(messageType, messageContent, hostId);
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
            redKitchen[stationID] = new List<Ingredient>();
        }
        else if (blueTeam.ContainsKey(connectionID)) {
            blueKitchen[stationID] = new List<Ingredient>();
        }
    }

    private void GameOver(string winningTeam)
    {
        // Should call the game over screen, showing the final scores on the main screen
        // Should tell players on the winning team they have won on their phones
        // Should tell players on the losing team they have lost on their phones

        finalBlueScore = blueScore.getScore();
        finalRedScore = redScore.getScore();

        if (winningTeam.Equals("blue"))
        {
            SceneManager.LoadScene("GameOverScreen");
        }
        else if (winningTeam.Equals("red"))
        {
            SceneManager.LoadScene("GameOverScreen");
        }
    }

    private void displayTime()
    {
        TimeSpan t = TimeSpan.FromSeconds(timer);
        string timerFormatted = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        timerText.text = "Time left " + timerFormatted;
        Debug.Log(timerText.text);
    }
}
