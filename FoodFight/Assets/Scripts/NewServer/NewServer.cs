using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class NewServer : MonoBehaviour {

  private const int MAX_CONNECTION = 10;
  private const int port = 8000;
  private int hostId, webHostId, reliableChannel;

  private int minimumPlayers = -1;

  public GameObject bluePlayerPrefab, redPlayerPrefab;
  public Transform pickPlayersCanvas, startGameCanvas, mainGameCanvas, gameOverCanvas;
  public Text startScreenText, redEndGameText, blueEndGameText, redScoreText, blueScoreText;
  public Image gameOverBackground;

  private NewGameTimer timer;
  private WiimoteBehaviourBlue wiiBlue;
  private WiimoteBehaviourRed wiiRed;

  private readonly Color redTeamColour = new Color(1.0f, 0.3f, 0.3f, 1.0f), blueTeamColour = new Color(0.3f, 0.5f, 1.0f, 1.0f);
  private Team redTeam, blueTeam;
  private GameState gameState = GameState.ConfigureGame;

  private void Start () {
    initialiseTeams();
    initialiseNetwork();

    timer = GameObject.Find("GameTimer").GetComponent<NewGameTimer>();
    wiiBlue = GameObject.Find("WiimoteManager").GetComponent<WiimoteBehaviourBlue>();
    wiiRed = GameObject.Find("WiimoteManager").GetComponent<WiimoteBehaviourRed>();
  }

  void Update() {
    SetCanvasForGameState();

    switch(gameState) {
      case GameState.ConfigureGame:
        listenForData();
        break;
      case GameState.AwaitingPlayers:
        listenForData();
        startScreenText.text = "Red: " + redTeam.Players.Count + " | Blue: " + blueTeam.Players.Count;
        break;
      case GameState.Countdown:
        break;
      case GameState.GameRunning:
        listenForData();
        redScoreText.text = "Red score: " + redTeam.Score;
        blueScoreText.text = "Blue score: " + blueTeam.Score;
        break;
      case GameState.EndGame:
        break;
    }
  }

  private void initialiseTeams() {
    redTeam = new Team("red", redTeamColour); blueTeam = new Team("blue", blueTeamColour);
  }

  private void initialiseNetwork() {
    NetworkTransport.Init();
    ConnectionConfig connectConfig = new ConnectionConfig();

    /* Network configuration */
    connectConfig.AckDelay = 33;
    connectConfig.AllCostTimeout = 20;
    connectConfig.ConnectTimeout = 1000;
    connectConfig.DisconnectTimeout = 5000;
    connectConfig.FragmentSize = 500;
    connectConfig.MaxCombinedReliableMessageCount = 10;
    connectConfig.MaxCombinedReliableMessageSize = 100;
    connectConfig.MaxConnectionAttempt = 32;
    connectConfig.MaxSentMessageQueueSize = 4096;
    connectConfig.MinUpdateTimeout = 20;
    connectConfig.NetworkDropThreshold = 40; // we had to set these high to avoid UNet disconnects during lag spikes
    connectConfig.OverflowDropThreshold = 40;
    connectConfig.PacketSize = 1500;
    connectConfig.PingTimeout = 500;
    connectConfig.ReducedPingTimeout = 100;
    connectConfig.ResendTimeout = 500;

    reliableChannel = connectConfig.AddChannel(QosType.ReliableSequenced);
    HostTopology topo = new HostTopology(connectConfig, MAX_CONNECTION);

    hostId = NetworkTransport.AddHost(topo, port, null /*ipAddress*/);
    webHostId = NetworkTransport.AddWebsocketHost(topo, port, null /*ipAddress*/);
  }

  /* Listens for data on the network, processing messages if required */
  private void listenForData() {
    int recHostId, // Player ID
        connectionId, // ID of connection to recHostId.
        channelID; // ID of channel connected to recHostId
    byte[] recBuffer = new byte[4096];
    int bufferSize = 4096, dataSize;
    byte error;

    NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelID, recBuffer, bufferSize, out dataSize, out error);

    switch (recData) {
      case NetworkEventType.Nothing: // Do nothing if nothing was sent to server
        break;
      case NetworkEventType.ConnectEvent: // Have a phone connect to the server
        Debug.Log("Player " + connectionId + " has connected");
        break;
      case NetworkEventType.DataEvent: // Have the phone send data to the server
        string message = OnNetworkData(hostId, connectionId, channelID, recBuffer, bufferSize, (NetworkError) error);
        manageMessageEvents(message, connectionId);
        break;
      case NetworkEventType.DisconnectEvent: // Remove the player from the game
        Debug.Log("Player " + connectionId + " has disconnected");
        OnNetworkDisconnect(connectionId);
        break;
      case NetworkEventType.BroadcastEvent:
        Debug.Log("Broadcast event.");
        break;
    }
  }

  /* Deserialises an incoming custom message ready for handling */
  private string OnNetworkData(int hostId, int connectionId, int channelId, byte[] data, int size, NetworkError error) {
    /* Deserialise the message */
    Stream serializedMessage = new MemoryStream(data);
    BinaryFormatter formatter = new BinaryFormatter();
    string message = formatter.Deserialize(serializedMessage).ToString();

    Debug.Log("OnData(hostId = " + hostId + ", connectionId = "
      + connectionId + ", channelId = " + channelId + ", data = "
      + message + ", size = " + size + ", error = " + error.ToString() + ")");

    return message;
  }

  /* Manages the disconnection of a player */
  private void OnNetworkDisconnect(int connectionId) {
    /* Determine the team from which the message originated */
    Team relevantTeam = getTeamForConnectionId(connectionId);

    if (relevantTeam != null) {
      ConnectedPlayer player = relevantTeam.getPlayerForId(connectionId);
      if (player != null) {
        Destroy(player.PlayerPrefab);
        relevantTeam.removePlayer(player);
        Debug.Log("Player " + connectionId + " removed from team " + relevantTeam.Name);
      }
    } else {
      Debug.Log("Disconnected player not part of a team");
    }
  }

  /* Sends a message across the network */
  public void SendMyMessage(string messageType, string textInput, int connectionId) {
    byte error;
    byte[] buffer = new byte[4096];
    Stream message = new MemoryStream(buffer);
    BinaryFormatter formatter = new BinaryFormatter();
    /* Serialise the message */
    string messageToSend = messageType + "&" + textInput;
    formatter.Serialize(message, messageToSend);

    NetworkTransport.Send(hostId, connectionId, reliableChannel, buffer, (int)message.Position, out error);

    if ((NetworkError) error != NetworkError.Ok) {
      Debug.Log("Message send error: " + (NetworkError) error);
    }
  }

  /* Handle incoming custom data messages */
  public void manageMessageEvents(string message, int connectionId) {
    string[] decodedMessage = decodeMessage(message, '&');
    string messageType = decodedMessage[0], messageContent = decodedMessage[1];

    switch (messageType) {
      case "connect": // Player chooses team to play on
        OnMessageConnect(connectionId, messageType, messageContent);
        break;
      case "station": // Player logs into station
        OnMessageStation(connectionId, messageType, messageContent);
        break;
      case "add": // Player adds an ingredient to a station
        OnMessageAdd(connectionId, messageType, messageContent);
        break;
      case "clear": // Player clears a station
        OnMessageClear(connectionId, messageType, messageContent);
        break;
      case "score": // Player updates score
        OnMessageScore(connectionId, messageType, messageContent);
        break;
      case "throw": // Player throws ingredient
        OnMessageThrow(connectionId, messageType, messageContent);
        break;
      case "leave": // Player leaves a station
        OnMessageLeave(connectionId, messageType, messageContent);
        break;
      default:
        Debug.Log("Invalid message type.");
        break;
    }
  }

  private void OnMessageConnect(int connectionId, string messageType, string messageContent) {
    if (gameState.Equals(GameState.AwaitingPlayers)) {
      /* Allocate the player to the team if they are not already on a team and send team to player */
      if (addPlayerToTeam(messageContent, connectionId)) {
        SendMyMessage(messageType, messageContent, connectionId);
      } else {
        SendMyMessage(messageType, "Unable to join team", connectionId);
      }
    } else {
      SendMyMessage(messageType, "Server not awaiting players", connectionId);
    }
  }

  private bool addPlayerToTeam(string teamName, int connectionId) {
    Team relevantTeam = null;
    GameObject relevantPrefab;

    if (teamName.Equals(redTeam.Name)) {
      relevantTeam = redTeam;
      relevantPrefab = redPlayerPrefab;
    } else if (teamName.Equals(blueTeam.Name)) {
      relevantTeam = blueTeam;
      relevantPrefab = bluePlayerPrefab;
    } else {
      Debug.Log("Invalid team name [" + teamName + "], could not allocate player to team.");
      return false;
    }

    if (relevantTeam.isPlayerOnTeam(connectionId)) {
      Debug.Log("Player [" + connectionId + "] already on team [" + teamName + "].");
      return false;
    }

    relevantPrefab = (GameObject) Instantiate(relevantPrefab, new Vector3(-40, 2, 5 * (relevantTeam.Players.Count + 1)), Quaternion.identity);

    return relevantTeam.addPlayerToTeam(new ConnectedPlayer(connectionId, relevantPrefab));
  }

  private void OnMessageStation(int connectionId, string messageType, string messageContent) {
    /* Determine the team from which the message originated */
    Team relevantTeam = getTeamForConnectionId(connectionId);

    if (relevantTeam != null) {
      /* Move the appropriate player to the destination station */
      ConnectedPlayer player = relevantTeam.getPlayerForId(connectionId);
      Station destinationStation = relevantTeam.Kitchen.getStationForId(messageContent);
      if (!relevantTeam.isStationOccupied(destinationStation)) {
        player.CurrentStation = destinationStation;
        string messageToSend = messageContent + "$";
        /* Send back all ingredients currently at this station */
        foreach (Ingredient ingredient in destinationStation.Ingredients) {
          messageToSend += Ingredient.SerializeObject(ingredient) + "$";
        }
        Debug.Log("Sending back to team [" + relevantTeam.Name + "]: " + messageToSend);
        SendMyMessage(messageType, messageToSend, connectionId);
      } else if (player.CurrentStation == destinationStation) {
        Debug.Log("Player already at station (" + destinationStation + ")");
        SendMyMessage(messageType, "Already at station", connectionId);
      } else {
        Debug.Log("Station already occupied (" + destinationStation + ")");
        SendMyMessage(messageType, "Station occupied", connectionId);
      }
    } else {
      Debug.Log("Could not determine team for given connectionId");
      SendMyMessage(messageType, "Fail", connectionId);
    }
  }

  private void OnMessageClear(int connectionId, string messageType, string messageContent) {
    /* Determine the team from which the message originated */
    Team relevantTeam = getTeamForConnectionId(connectionId);

    if (relevantTeam != null) {
      /* Clear the appropriate station */
      ConnectedPlayer player = relevantTeam.getPlayerForId(connectionId);
      if (player.CurrentStation != null) player.CurrentStation.clearIngredientsInStation();

      /* Send back success */
      // SendMyMessage(messageType, "Success", connectionId);
    } else {
      Debug.Log("Could not determine team for given connectionId");
      SendMyMessage(messageType, "Fail", connectionId);
    }
  }

  private void OnMessageAdd(int connectionId, string messageType, string messageContent)  {
    /* Determine the team from which the message originated */
    Team relevantTeam = getTeamForConnectionId(connectionId);

    if (relevantTeam != null) {
      /* Add the ingredient to the relevant station */
      ConnectedPlayer player = relevantTeam.getPlayerForId(connectionId);

      /* Deserialize and add ingredient */
      if (!messageContent.Equals("")) {
        Ingredient ingredientToAdd = new Ingredient();
        ingredientToAdd = Ingredient.XmlDeserializeFromString<Ingredient>(messageContent, ingredientToAdd.GetType());
        Debug.Log("Ingredient to add: " + ingredientToAdd.Name);
        player.CurrentStation.addIngredientToStation(ingredientToAdd);
        // SendMyMessage(messageType, "Success", connectionId);
      } else {
        Debug.Log("Invalid messageContent");
        SendMyMessage(messageType, "Fail", connectionId);
      }
    } else {
      Debug.Log("Could not determine team for given connectionId");
      SendMyMessage(messageType, "Fail", connectionId);
    }
  }

  private void OnMessageScore(int connectionId, string messageType, string messageContent)  {
    /* Determine the team from which the message originated */
    Team relevantTeam = getTeamForConnectionId(connectionId);

    if (relevantTeam != null) {
      ConnectedPlayer player = relevantTeam.getPlayerForId(connectionId);

      if (!messageContent.Equals("")) {
        Ingredient ingredientToScore = new Ingredient();
        ingredientToScore = Ingredient.XmlDeserializeFromString<Ingredient>(messageContent, ingredientToScore.GetType());
        Debug.Log("Ingredient to score: " + ingredientToScore.Name);
        relevantTeam.Score += ScoreIngredient(ingredientToScore);
        // SendMyMessage(messageType, "Success", connectionId);
      } else {
        Debug.Log("Invalid messageContent");
        SendMyMessage(messageType, "Fail", connectionId);
      }
    } else {
      Debug.Log("Could not determine team for given connectionId");
      SendMyMessage(messageType, "Fail", connectionId);
    }
  }

  private void OnMessageThrow(int connectionId, string messageType, string messageContent)  {
    /* Determine the team from which the message originated */
    Team relevantTeam = getTeamForConnectionId(connectionId);

    if (relevantTeam != null) {
      ConnectedPlayer player = relevantTeam.getPlayerForId(connectionId);

      if (!messageContent.Equals("")) {
        Ingredient ingredientToThrow = new Ingredient();
        ingredientToThrow = Ingredient.XmlDeserializeFromString<Ingredient>(messageContent, ingredientToThrow.GetType());
        Debug.Log("Ingredient to throw: " + ingredientToThrow.Name);
        if (relevantTeam.Name.Equals("red")) {
          wiiRed.reset(ingredientToThrow);
        } else if (relevantTeam.Name.Equals("blue")) {
          wiiBlue.reset(ingredientToThrow);
        }
        /* Call fighting reset here!!! */
      } else {
        Debug.Log("Invalid messageContent");
        SendMyMessage(messageType, "Fail", connectionId);
      }
    } else {
      Debug.Log("Could not determine team for given connectionId");
      SendMyMessage(messageType, "Fail", connectionId);
    }
  }

  private void OnMessageLeave(int connectionId, string messageType, string messageContent)  {
    /* Determine the team from which the message originated */
    Team relevantTeam = getTeamForConnectionId(connectionId);

    if (relevantTeam != null) {
      /* Set players current station to null */
      ConnectedPlayer player = relevantTeam.getPlayerForId(connectionId);
      player.CurrentStation = null;

      /* Send back success */
      // SendMyMessage(messageType, "Success", connectionId);
    } else {
      Debug.Log("Could not determine team for given connectionId");
      SendMyMessage(messageType, "Fail", connectionId);
    }
  }

  private int ScoreIngredient(Ingredient ingredient) {
    return FoodData.Instance.getScoreForIngredient(ingredient);
  }

  public void OnStationHit(string team, string station){
    Debug.Log(team);
    Debug.Log(station);
  }

  /* Gets the team that a connected player is on, returning null if not found */
  private Team getTeamForConnectionId(int connectionId) {
    Team relevantTeam = null;
    if (redTeam.isPlayerOnTeam(connectionId)) relevantTeam = redTeam;
    else if (blueTeam.isPlayerOnTeam(connectionId)) relevantTeam = blueTeam;
    return relevantTeam;
  }

  /* Splits a message with the provided delimiter */
  private string[] decodeMessage(string message, char delimiter) {
    return message.Split(delimiter);
  }

  /* Simple assertion tests for teams */
  private void test() {
    /* Reset teams */
    initialiseTeams();
    gameState = GameState.AwaitingPlayers;
    Ingredient egg = new Ingredient("Eggs", "eggsPrefab");

    Debug.Assert(Kitchen.isValidStation("0"));
    Debug.Assert(Kitchen.isValidStation("3"));
    Debug.Assert(!Kitchen.isValidStation("hello"));

    /* Simulate connections */
    OnMessageConnect(100, "connect", "red");
    OnMessageConnect(101, "connect", "red");
    OnMessageConnect(102, "connect", "red");
    Debug.Assert(redTeam.Players.Count == 3);

    /* Simulate connection with duplicate id */
    OnMessageConnect(555, "connect", "blue");
    OnMessageConnect(556, "connect", "blue");
    OnMessageConnect(557, "connect", "blue");
    OnMessageConnect(555, "connect", "blue");
    Debug.Assert(blueTeam.Players.Count == 3);
    Debug.Assert(blueTeam.getPlayerForId(555) != null);
    Debug.Assert(blueTeam.getPlayerForId(556) != null);
    Debug.Assert(blueTeam.getPlayerForId(557) != null);
    Debug.Assert(blueTeam.getPlayerForId(558) == null);

    /* Simulate connection with mis-spelled content */
    OnMessageConnect(558, "connect", "bule");
    Debug.Assert(blueTeam.Players.Count == 3);
    Debug.Assert(blueTeam.getPlayerForId(558) == null);

    /* Simulate station joining */
    OnMessageStation(101, "station", "1");
    Debug.Assert(redTeam.getPlayerForId(101).CurrentStation.Id.Equals("1"));

    /* Simulate station ingredient storage */
    blueTeam.Kitchen.getStationForId("2").addIngredientToStation(egg);
    blueTeam.Kitchen.getStationForId("3").addIngredientToStation(egg);
    OnMessageStation(555, "station", "2");
    OnMessageStation(556, "station", "1");
    Debug.Assert(blueTeam.getPlayerForId(557).CurrentStation == null);
    Debug.Assert(blueTeam.getPlayerForId(555).CurrentStation.Ingredients.Count == 1);
    Debug.Assert(blueTeam.getPlayerForId(556).CurrentStation.Ingredients.Count == 0);

    /* Simulate station clearing */
    OnMessageClear(555, "clear", "2");
    OnMessageClear(555, "clear", "3"); /* Player is not at this station, so it should not be cleared */
    Debug.Assert(blueTeam.getPlayerForId(555).CurrentStation.Ingredients.Count == 0);
    Debug.Assert(blueTeam.Kitchen.getStationForId("3").Ingredients.Count == 1);

    /* Station occupied tests */
    OnMessageStation(100, "station", "0");
    OnMessageStation(101, "station", "4");
    Debug.Assert(redTeam.isStationOccupied("0"));
    Debug.Assert(redTeam.isStationOccupied("4"));
    Debug.Assert(!redTeam.isStationOccupied("3"));

    /* Checking if station is occupied before moving */
    OnMessageStation(101, "station", "4");
    OnMessageStation(100, "station", "0");
    OnMessageStation(101, "station", "0");
    Debug.Assert(redTeam.isStationOccupied("0"));
    Debug.Assert(redTeam.getPlayerForId(100).CurrentStation.Id.Equals("0"));
    Debug.Assert(redTeam.getPlayerForId(101).CurrentStation.Id.Equals("4"));

    /* Tests for leaving stations */
    OnMessageStation(555, "station", "0");
    OnMessageStation(556, "station", "1");
    OnMessageStation(557, "station", "2");
    Debug.Assert(blueTeam.isStationOccupied("1"));
    Debug.Assert(blueTeam.isStationOccupied("2"));
    OnMessageLeave(556, "leave", "");
    Debug.Assert(!blueTeam.isStationOccupied("1"));
    Debug.Assert(blueTeam.isStationOccupied("2"));
    OnMessageLeave(557, "leave", "2");
    Debug.Assert(!blueTeam.isStationOccupied("2"));
    OnMessageStation(557, "station", "2");
    Debug.Assert(blueTeam.isStationOccupied("2"));

    /* Simulate adding to station */
    OnMessageStation(101, "station", "0");
    OnMessageStation(102, "station", "1");
    OnMessageAdd(101, "add", Ingredient.SerializeObject(egg));
    Debug.Assert(redTeam.getPlayerForId(101).CurrentStation.Ingredients.Count == 1);
    OnMessageAdd(101, "add", Ingredient.SerializeObject(egg));
    Debug.Assert(redTeam.getPlayerForId(101).CurrentStation.Ingredients.Count == 2);
    Debug.Assert(redTeam.getPlayerForId(102).CurrentStation.Ingredients.Count == 0);

    /* Reset teams */
    initialiseTeams();
    gameState = GameState.ConfigureGame;
  }

  /* Sets the active canvas based on the game state */
  public void SetCanvasForGameState() {
    pickPlayersCanvas.gameObject.SetActive(gameState.Equals(GameState.ConfigureGame));
    startGameCanvas.gameObject.SetActive(gameState.Equals(GameState.AwaitingPlayers));
    gameOverCanvas.gameObject.SetActive(gameState.Equals(GameState.EndGame));
    mainGameCanvas.gameObject.SetActive(gameState.Equals(GameState.GameRunning) || gameState.Equals(GameState.Countdown));
  }

  public void OnTwoPlayers() {
    minimumPlayers = 1;
    SetGameState(GameState.AwaitingPlayers);
  }

  public void OnThreePlayers() {
    minimumPlayers = 2;
    SetGameState(GameState.AwaitingPlayers);
  }

  public void OnFourPlayers() {
    minimumPlayers = 3;
    SetGameState(GameState.AwaitingPlayers);
  }

  /* Broadcasts start */
  public void StartGame() {
    // Commented out for testing:
    // if (minimumPlayers > 0 &&
    //     redTeam.Players.Count >= minimumPlayers &&
    //     blueTeam.Players.Count >= minimumPlayers) {
      BroadcastMessage("start", "");
      SetGameState(GameState.GameRunning);
      timer.StartTimer();
    // }
  }

  /* Called by GameTimer.cs */
  public void OnGameOver() {
    SetGameState(GameState.EndGame);
    redEndGameText.text = "Red score: " + redTeam.Score;
    blueEndGameText.text = "Blue score: " + blueTeam.Score;
    Team winningTeam = getWinningTeam();
    if (winningTeam != null) {
      gameOverBackground.color = winningTeam.Colour;
    } else {
      /* Draw! */
      gameOverBackground.color = Color.white;
    }
    /* TODO: Broadcast end game to players */
  }

  public void RestartGame() {
    initialiseTeams();
    timer.ResetTimer();
    SetGameState(GameState.ConfigureGame);
  }

  /* Returns the winning team, or null if draw */
  private Team getWinningTeam() {
    if (redTeam.Score > blueTeam.Score) return redTeam;
    if (blueTeam.Score > redTeam.Score) return blueTeam;
    return null;
  }

  private void SetGameState(GameState state) {
    gameState = state;
  }

  /* Sends a message to all connected players */
  public void BroadcastMessage(string messageType, string textInput) {
    Team[] allTeams = new Team[] {redTeam, blueTeam};
    foreach(Team team in allTeams) {
      foreach(ConnectedPlayer player in team.Players) {
        Debug.Log("Broadcasting [" + messageType + ", " + textInput + "]");
        SendMyMessage(messageType, textInput, player.ConnectionId);
      }
    }
  }
}
