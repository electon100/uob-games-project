using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class NewServer : MonoBehaviour {

  private const int MAX_CONNECTION = 10;
  private const int port = 8000;
  private int hostId, webHostId, reliableChannel;

  private int minimumPlayers = -1;

  public GameObject bluePlayerPrefab, redPlayerPrefab;
  public Transform pickPlayersCanvas, startGameCanvas, mainGameCanvas;
  private Team redTeam = new Team("red"), blueTeam = new Team("blue");

  private void Start () {
    initialiseNetwork();
    redTeam.Kitchen.getStationForId("0").addIngredientToStation(new Ingredient("Eggs", "eggsPrefab"));
  }

  void Update() {
    Debug.Log("Update!");
    listenForData();
    if (redTeam.Players.Count == minimumPlayers || blueTeam.Players.Count == minimumPlayers) {
      pickPlayersCanvas.gameObject.SetActive(false);
      startGameCanvas.gameObject.SetActive(true);
    }
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
    connectConfig.MaxSentMessageQueueSize = 2048;
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
        string message = OnData(hostId, connectionId, channelID, recBuffer, bufferSize, (NetworkError) error);
        manageMessageEvents(message, connectionId);
        break;
      case NetworkEventType.DisconnectEvent: // Remove the player from the game
        Debug.Log("Player " + connectionId + " has disconnected");
        break;
      case NetworkEventType.BroadcastEvent:
        Debug.Log("Broadcast event.");
        break;
    }
  }

  // This function is called when custom data is sent
  private string OnData(int hostId, int connectionId, int channelId, byte[] data, int size, NetworkError error) {
    /* Deserialise the message */
    Stream serializedMessage = new MemoryStream(data);
    BinaryFormatter formatter = new BinaryFormatter();
    string message = formatter.Deserialize(serializedMessage).ToString();

    Debug.Log("OnData(hostId = " + hostId + ", connectionId = "
      + connectionId + ", channelId = " + channelId + ", data = "
      + message + ", size = " + size + ", error = " + error.ToString() + ")");

    return message;
  }

  // Used for sending data to the players
  public void SendMyMessage(string messageType, string textInput, int connectionId) {
    byte error;
    byte[] buffer = new byte[4096];
    Stream message = new MemoryStream(buffer);
    BinaryFormatter formatter = new BinaryFormatter();
    //Serialize the message
    string messageToSend = messageType + "&" + textInput;
    formatter.Serialize(message, messageToSend);

    //Send the message from the "client" with the serialized message and the connection information
    NetworkTransport.Send(hostId, connectionId, reliableChannel, buffer, (int)message.Position, out error);

    //If there is an error, output message error to the console
    if ((NetworkError) error != NetworkError.Ok) {
      Debug.Log("Message send error: " + (NetworkError) error);
    }
  }

  // This is where all the work happens.
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
      case "leave": // Player leaves a station
        OnMessageLeave(connectionId, messageType, messageContent);
        break;
      default:
        Debug.Log("Invalid message type.");
        break;
    }
  }

  private void OnMessageConnect(int connectionId, string messageType, string messageContent) {
    /* Allocate the player to the team if they are not already on a team and send team to player */
    if (addPlayerToTeam(messageContent, connectionId)) {
      SendMyMessage(messageType, messageContent, connectionId);
    } else {
      SendMyMessage(messageType, "Invalid team name", connectionId);
    }
  }

  private void OnMessageStation(int connectionId, string messageType, string messageContent) {
    /* Determine the team from which the message originated */
    Team relevantTeam = getTeamForConnectionId(connectionId);

    if (relevantTeam != null) {
      /* Move the appropriate player to the destination station */
      ConnectedPlayer player = relevantTeam.getPlayerForId(connectionId);
      Station destinationStation = relevantTeam.Kitchen.getStationForId(messageContent);
      player.CurrentStation = destinationStation;

      /* Send back all ingredients currently at this station */
      string messageToSend = messageContent + "$";
      foreach (Ingredient ingredient in destinationStation.Ingredients) {
        messageToSend += Ingredient.SerializeObject(ingredient) + "$";
      }
      Debug.Log("Sending back to team [" + relevantTeam.Name + "]: " + messageToSend);
      SendMyMessage(messageType, messageToSend, connectionId);
    } else {
      SendMyMessage(messageType, "Fail", connectionId);
      Debug.Log("Could not determine team for given connectionId");
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
      SendMyMessage(messageType, "Success", connectionId);
    } else {
      SendMyMessage(messageType, "Fail", connectionId);
      Debug.Log("Could not determine team for given connectionId");
    }
  }

  private void OnMessageAdd(int connectionId, string messageType, string messageContent)  {
    /* Determine the team from which the message originated */
    Team relevantTeam = getTeamForConnectionId(connectionId);

    if (relevantTeam != null) {
      /* Add the ingredient to the relevant station */
      ConnectedPlayer player = relevantTeam.getPlayerForId(connectionId);
      // player.CurrentStation.addIngredientToStation(...); TODO

      /* Send back success */
      SendMyMessage(messageType, "Success", connectionId);
    } else {
      SendMyMessage(messageType, "Fail", connectionId);
      Debug.Log("Could not determine team for given connectionId");
    }
  }

  private void OnMessageScore(int connectionId, string messageType, string messageContent)  {
    /* TODO */
  }

  private void OnMessageLeave(int connectionId, string messageType, string messageContent)  {
    /* Determine the team from which the message originated */
    Team relevantTeam = getTeamForConnectionId(connectionId);

    if (relevantTeam != null) {
      /* Set players current station to null */
      ConnectedPlayer player = relevantTeam.getPlayerForId(connectionId);
      player.CurrentStation = null;

      /* Send back success */
      SendMyMessage(messageType, "Success", connectionId);
    } else {
      Debug.Log("Could not determine team for given connectionId");
    }
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

  private bool addPlayerToTeam(string teamName, int connectionId) {
    Team relevantTeam = null;
    GameObject relevantPrefab;

    if (teamName.Equals(redTeam.Name)) {
      relevantTeam = redTeam;
      relevantPrefab = (GameObject) Instantiate(redPlayerPrefab, new Vector3(-40, 2, 5 * (redTeam.Players.Count + 1)), Quaternion.identity);
    } else if (teamName.Equals(blueTeam.Name)) {
      relevantTeam = blueTeam;
      relevantPrefab = (GameObject) Instantiate(bluePlayerPrefab, new Vector3(-40, 2, 5 * (blueTeam.Players.Count + 1)), Quaternion.identity);
    } else {
      Debug.Log("Invalid team name [" + teamName + "], could not allocate player to team.");
      return false;
    }

    relevantTeam.addPlayerToTeam(new ConnectedPlayer(connectionId, relevantPrefab));
    return true;
  }

  public void OnTwoPlayers() {
    minimumPlayers = 1;
  }

  public void OnThreePlayers() {
    minimumPlayers = 2;
  }

  public void OnFourPlayers() {
    minimumPlayers = 3;
  }

  public void StartGame() {
    Team[] allTeams = new Team[] {redTeam, blueTeam};
    foreach(Team team in allTeams) {
      foreach(ConnectedPlayer player in team.Players) {
        SendMyMessage("start", "", player.ConnectionId);
        startGameCanvas.gameObject.SetActive(false);
        mainGameCanvas.gameObject.SetActive(true);
      }
    }
  }
}
