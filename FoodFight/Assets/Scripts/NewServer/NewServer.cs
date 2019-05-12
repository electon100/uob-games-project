using System.Diagnostics;
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

  public GameObject bluePlayerPrefab, redPlayerPrefab, redBannerObject, blueBannerObject;
  public Transform mainMenuCanvas, pickModeCanvas, startGameCanvas, mainGameCanvas, gameOverCanvas;
  public RectTransform redBannerTransform, blueBannerTransform;
  public Text startScreenText, redEndGameText, blueEndGameText, redScoreText, blueScoreText, winningTeamText;
  public Image gameOverBackground, redStarSlider, blueStarSlider, redStarBackground, blueStarBackground, blueBannerImage, redBannerImage;

  private NewGameTimer timer;
  private WiimoteBehaviourBlue wiiBlue;
  private WiimoteBehaviourRed wiiRed;

  private readonly Color redTeamColour = new Color(1.0f, 0.3f, 0.3f, 1.0f), blueTeamColour = new Color(0.3f, 0.5f, 1.0f, 1.0f);
  private readonly Color drawColour = new Color(0.23f, 0.71f, 0.58f, 1.0f);

  private readonly float disableStationDuration = 10.0f; /* 15 seconds */
  private readonly float negativeScoreMultiplier = 0.2f;
  private readonly float minNextOrderTime = 15.0f; /* Minimum time before a new order is added */
  private readonly float maxNextOrderTime = 25.0f; /* Maximum time before a new order is added */

  private Team redTeam, blueTeam;
  public GameState gameState = GameState.MainMenu;
  public GameMode gameMode = GameMode.None;

  private List<int> allConnections = new List<int>();

  private void Start () {
    initialiseTeams();
    initialiseNetwork();
    initialiseUI();

    timer = GameObject.Find("GameTimer").GetComponent<NewGameTimer>();
    wiiBlue = GameObject.Find("WiimoteManager").GetComponent<WiimoteBehaviourBlue>();
    wiiRed = GameObject.Find("WiimoteManager").GetComponent<WiimoteBehaviourRed>();
  }

  void Update() {
    SetCanvasForGameState(); /* Sets the main screen visible canvas based on game state */
    TickStations(); /* Ticks down the disabled timers on all stations */
    listenForKeyboardInput(); /* Process keyboard input */

    switch(gameState) {
      case GameState.ConfigureMode:
        listenForData();
        break;
      case GameState.AwaitingPlayers:
        listenForData();
        startScreenText.text = "Red: " + redTeam.Players.Count + " | Blue: " + blueTeam.Players.Count;
        break;
      case GameState.Countdown:
        setTeamStars();
        break;
      case GameState.GameRunning:
        listenForData();
        manageOrders();
        setTeamStars();
        break;
      case GameState.EndGame:
        break;
    }
  }

  private void listenForKeyboardInput() {
    if (Input.GetKeyDown(KeyCode.R)) RestartGame();
    if (Input.GetKeyDown(KeyCode.S)) OnLatinMode();
    if (Input.GetKeyDown(KeyCode.F)) OnFrenchMode();
    if (Input.GetKeyDown(KeyCode.Space) && gameState == GameState.AwaitingPlayers) StartGame();
  }

  private void setTeamStars() {
    int bannerWidth;
    int bannerHeight;

    bannerWidth = (int) Screen.width / 4;
    bannerHeight = (int) bannerWidth / 2;

    redBannerTransform.sizeDelta = new Vector2(bannerWidth, bannerHeight);
    blueBannerTransform.sizeDelta = new Vector2(bannerWidth, bannerHeight);

    int xPosRed = (int) (-Screen.width / 2 + bannerWidth / 2 + bannerWidth / 12);
    int xPosBlue = (int) (Screen.width / 2 - bannerWidth / 2 - bannerWidth / 12);

    int yPos = (int) (Screen.height / 2 - bannerHeight / 2);

    redBannerTransform.localPosition = new Vector2(xPosRed, yPos);
    blueBannerTransform.localPosition = new Vector2(xPosBlue, yPos);

    float maxScore = 500.0f;

    int redSliderWidth = clampScore(redTeam.Score, 0, (int) maxScore);
    int blueSliderWidth = clampScore(blueTeam.Score, 0, (int) maxScore);

    redSliderWidth = (int) ((redSliderWidth / maxScore) * (bannerWidth * 0.95f));
    blueSliderWidth = (int) ((blueSliderWidth / maxScore) * (bannerWidth * 0.95f));

    redStarSlider.rectTransform.sizeDelta = new Vector2(redSliderWidth, (int) (bannerHeight / 2.5f));
    blueStarSlider.rectTransform.sizeDelta = new Vector2(blueSliderWidth, (int) (bannerHeight / 2.5f));

    redStarSlider.rectTransform.localPosition = new Vector2((int) (xPosRed - bannerWidth / 2 + 0.025f * bannerWidth), yPos);
    blueStarSlider.rectTransform.localPosition = new Vector2((int) (xPosBlue + bannerWidth / 2 - 0.025f * bannerWidth), yPos);

    redStarBackground.rectTransform.sizeDelta = new Vector2((int) (0.95f * bannerWidth), (int) (bannerHeight / 2.5f));
    blueStarBackground.rectTransform.sizeDelta = new Vector2((int) (0.95f * bannerWidth), (int) (bannerHeight / 2.5f));

    redStarBackground.rectTransform.localPosition = new Vector2((int) (xPosRed - bannerWidth / 2 + 0.025f * bannerWidth), yPos);
    blueStarBackground.rectTransform.localPosition = new Vector2((int) (xPosBlue + bannerWidth / 2 - 0.025f * bannerWidth), yPos);

    redScoreText.text = redTeam.Score.ToString();
    blueScoreText.text = blueTeam.Score.ToString();

    redScoreText.alignment = TextAnchor.MiddleCenter;
    blueScoreText.alignment = TextAnchor.MiddleCenter;

    redScoreText.rectTransform.sizeDelta = new Vector2(bannerWidth / 4, bannerHeight / 2);
    blueScoreText.rectTransform.sizeDelta = new Vector2(bannerWidth / 4, bannerHeight / 2);

    redScoreText.fontSize = bannerHeight;
    blueScoreText.fontSize = bannerHeight;

    redScoreText.rectTransform.localPosition = new Vector2((int) (xPosRed - bannerWidth / 2 + bannerWidth / 4.5f), (int) (yPos - bannerHeight / 3.2f));
    blueScoreText.rectTransform.localPosition = new Vector2((int) (xPosBlue + bannerWidth / 2 - bannerWidth / 4.5f), (int) (yPos - bannerHeight / 3.2f));

  }

  private void initialiseUI() {
    redBannerObject = new GameObject("redBannerObject", typeof(RectTransform));
    redBannerObject.transform.SetParent(GameObject.Find("redBanner").transform);

    // Banner Position
    redBannerTransform = redBannerObject.GetComponent<RectTransform>();

    // Banner Image
		redBannerImage = redBannerObject.AddComponent<Image>();
		redBannerImage.sprite = Resources.Load("star_banner_red", typeof(Sprite)) as Sprite;

    blueBannerObject = new GameObject("blueBannerObject", typeof(RectTransform));
    blueBannerObject.transform.SetParent(GameObject.Find("blueBanner").transform);

    // Banner Position
    blueBannerTransform = blueBannerObject.GetComponent<RectTransform>();

    // Banner Image
    blueBannerImage = blueBannerObject.AddComponent<Image>();
    blueBannerImage.sprite = Resources.Load("star_banner_blue", typeof(Sprite)) as Sprite;

    setTeamStars();
  }

  private void turnLightsOn(string team, char stationID){
    try{
      ProcessStartInfo startInfo = new ProcessStartInfo("C:\\Program Files\\Python36\\python.exe");
      startInfo.WindowStyle = ProcessWindowStyle.Hidden;
      Process.Start(startInfo);

      string pythonArgs = "C:\\Users\\Benji\\Desktop\\red_lights_client.py " + team + " " + stationID;
      startInfo.Arguments = pythonArgs;
      UnityEngine.Debug.Log(pythonArgs);
      Process.Start(startInfo);
    }catch{
      UnityEngine.Debug.Log("Problem running python script");
    }

  }

  private void initialiseTeams() {
    if (redTeam != null) redTeam.removeAllOrders();
    if (blueTeam != null) blueTeam.removeAllOrders();
    redTeam = new Team("red", redTeamColour); blueTeam = new Team("blue", blueTeamColour);

    redTeam.Score = 100;
    blueTeam.Score = 100;

    Team[] allTeams = new Team[] {redTeam, blueTeam};
    foreach (Team team in allTeams) {
      foreach (Station station in team.Kitchen.Stations) {
        GameObject StationDisablePrefab = GameObject.Find(team.Name + "station" + station.Id + "prefabdisable");
        station.DisablePrefab = StationDisablePrefab;

        GameObject StationPrefab = GameObject.Find(team.Name + "stationprefab" + station.Id);
        station.Prefab = StationPrefab;
      }
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
        UnityEngine.Debug.Log(connectionId + " has connected");
        allConnections.Add(connectionId);
        break;
      case NetworkEventType.DataEvent: // Have the phone send data to the server
        string message = OnNetworkData(hostId, connectionId, channelID, recBuffer, bufferSize, (NetworkError) error);
        manageMessageEvents(message, connectionId);
        break;
      case NetworkEventType.DisconnectEvent: // Remove the player from the game
        UnityEngine.Debug.Log(connectionId + " has disconnected");
        OnNetworkDisconnect(connectionId);
        break;
      case NetworkEventType.BroadcastEvent:
        UnityEngine.Debug.Log("Broadcast event.");
        break;
    }
  }

  /* Deserialises an incoming custom message ready for handling */
  private string OnNetworkData(int hostId, int connectionId, int channelId, byte[] data, int size, NetworkError error) {
    /* Deserialise the message */
    Stream serializedMessage = new MemoryStream(data);
    BinaryFormatter formatter = new BinaryFormatter();
    string message = formatter.Deserialize(serializedMessage).ToString();

    UnityEngine.Debug.Log("OnData(hostId = " + hostId + ", connectionId = "
      + connectionId + ", channelId = " + channelId + ", data = "
      + message + ", size = " + size + ", error = " + error.ToString() + ")");

    return message;
  }

  /* Manages the disconnection of a player */
  private void OnNetworkDisconnect(int connectionId) {
    removePlayerFromTeam(connectionId);
    allConnections.Remove(connectionId);
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
      UnityEngine.Debug.Log("Message send error: " + (NetworkError) error);
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
      case "disconnect": // Player goes to home page
        OnMessageDisconnect(connectionId, messageType, messageContent);
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
      case "order": // An Order is sent to the server
        OnMessageOrder(connectionId, messageType, messageContent);
        break;
      default:
        UnityEngine.Debug.Log("Invalid message type.");
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

  private void OnMessageDisconnect(int connectionId, string messageType, string messageContent) {
    removePlayerFromTeam(connectionId);
  }

  private void removePlayerFromTeam(int connectionId) {
    /* Determine the team from which the request originated */
    Team relevantTeam = getTeamForConnectionId(connectionId);

    if (relevantTeam != null) {
      ConnectedPlayer player = relevantTeam.getPlayerForId(connectionId);
      if (player != null) {
        Destroy(player.PlayerPrefab);
        relevantTeam.removePlayer(player);
        UnityEngine.Debug.Log("Player " + connectionId + " removed from team " + relevantTeam.Name);
      }
    } else {
      UnityEngine.Debug.Log("Player not part of a team");
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
      UnityEngine.Debug.Log("Invalid team name [" + teamName + "], could not allocate player to team.");
      return false;
    }

    if (relevantTeam.isPlayerOnTeam(connectionId)) {
      UnityEngine.Debug.Log("Player [" + connectionId + "] already on team [" + teamName + "].");
      return false;
    }

    Vector3 startPosition;
    if (relevantTeam.Colour == redTeamColour){
      startPosition = new Vector3(-35, 2, 4 * (relevantTeam.Players.Count + 1));
    }
    else if (relevantTeam.Colour == blueTeamColour){
      startPosition = new Vector3(35, 2, 4 * (relevantTeam.Players.Count + 1));
    }
    else{
      startPosition = new Vector3(0, 0, 0);
    }

    relevantPrefab = (GameObject) Instantiate(relevantPrefab, startPosition, Quaternion.identity);
    relevantPrefab.GetComponent<PlayerMovement>().startPosition = startPosition;

    return relevantTeam.addPlayerToTeam(new ConnectedPlayer(connectionId, relevantPrefab));
  }

  private void OnMessageStation(int connectionId, string messageType, string messageContent) {
    /* Determine the team from which the message originated */
    Team relevantTeam = getTeamForConnectionId(connectionId);

    if (relevantTeam != null) {
      /* Move the appropriate player to the destination station */
      ConnectedPlayer player = relevantTeam.getPlayerForId(connectionId);
      Station destinationStation = relevantTeam.Kitchen.getStationForId(messageContent);
      if (destinationStation.isDisabled()) {
        UnityEngine.Debug.Log("Station disabled (" + destinationStation + ")");
        SendMyMessage(messageType, "Station disabled$" + destinationStation.DisabledTimer, connectionId);
      } else if (!relevantTeam.isStationOccupied(destinationStation)) {
        player.CurrentStation = destinationStation;
        MovePlayer(player.PlayerPrefab, relevantTeam, destinationStation);
        string messageToSend = messageContent + "$";
        /* Send back all ingredients currently at this station */
        foreach (Ingredient ingredient in destinationStation.Ingredients) {
          messageToSend += Ingredient.SerializeObject(ingredient) + "$";
        }
        UnityEngine.Debug.Log("Sending back to team [" + relevantTeam.Name + "]: " + messageToSend);
        SendMyMessage(messageType, messageToSend, connectionId);
      } else if (player.CurrentStation == destinationStation) {
        UnityEngine.Debug.Log("Player already at station (" + destinationStation + ")");
        SendMyMessage(messageType, "Already at station", connectionId);
      } else {
        UnityEngine.Debug.Log("Station already occupied (" + destinationStation + ")");
        SendMyMessage(messageType, "Station occupied", connectionId);
      }
    } else {
      UnityEngine.Debug.Log("Could not determine team for given connectionId");
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
    } else {
      UnityEngine.Debug.Log("Could not determine team for given connectionId");
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
        UnityEngine.Debug.Log("Ingredient to add: " + ingredientToAdd.Name);
        player.CurrentStation.addIngredientToStation(ingredientToAdd);
      } else {
        UnityEngine.Debug.Log("Invalid messageContent");
        SendMyMessage(messageType, "Fail", connectionId);
      }
    } else {
      UnityEngine.Debug.Log("Could not determine team for given connectionId");
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
        UnityEngine.Debug.Log("Ingredient to score: " + ingredientToScore.Name);
        relevantTeam.scoreRecipe(ingredientToScore);

        /* Broadcast new scores to devices */
        BroadcastScores();
      } else {
        UnityEngine.Debug.Log("Invalid messageContent");
        SendMyMessage(messageType, "Fail", connectionId);
      }
    } else {
      UnityEngine.Debug.Log("Could not determine team for given connectionId");
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
        UnityEngine.Debug.Log("Ingredient to throw: " + ingredientToThrow.Name);
        if (relevantTeam.Name.Equals("red")) {
          wiiRed.reset(ingredientToThrow);
        } else if (relevantTeam.Name.Equals("blue")) {
          wiiBlue.reset(ingredientToThrow);
        }
        /* Call fighting reset here!!! */
      } else {
        UnityEngine.Debug.Log("Invalid messageContent");
        SendMyMessage(messageType, "Fail", connectionId);
      }
    } else {
      UnityEngine.Debug.Log("Could not determine team for given connectionId");
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
      player.PlayerPrefab.GetComponent<PlayerMovement>().movePlayer(new Vector3(0, 0, 0));
    } else {
      UnityEngine.Debug.Log("Could not determine team for given connectionId");
      SendMyMessage(messageType, "Fail", connectionId);
    }
  }

  private void OnMessageOrder(int connectionId, string messageType, string messageContent) {
    if (!messageContent.Equals("")) {
      UnityEngine.Debug.Log("Order received");
      string[] messageDetails = decodeMessage(messageContent, '$');
      string team = messageDetails[0];
      string order = messageDetails[1];

      Ingredient ingred = new Ingredient(order, "mush");

      if (FoodData.Instance.MatchesMode(ingred)) {
        if (gameState == GameState.GameRunning){
          if (!order.Equals("")) {
            if (team.Equals("red")) {
              redTeam.addOrder(mainGameCanvas, order);
            } else if (team.Equals("blue")) {
              blueTeam.addOrder(mainGameCanvas, order);
            }
          }
        }
      }
    }
  }

  private int ScoreIngredient(Ingredient ingredient) {
    return FoodData.Instance.getScoreForIngredient(ingredient);
  }

  public void OnStationHit(string team, string station) {
    if (Kitchen.isValidStation(station)) {
      Team relevantTeam = null;
      Team throwingTeam = null;

      /* Values switched around as you want to disable the opposing kitchen, not your own */
      if (team.Equals(redTeam.Name)) {
        relevantTeam = blueTeam;
        throwingTeam = redTeam;
      }
      else if (team.Equals(blueTeam.Name)) {
        relevantTeam = redTeam;
        throwingTeam = blueTeam;
      }

      // Modify score when you hit an enemy station
      // Score depends on the station hit: cupboard = 4, chopping = 10, frying = 8, plating = 6
      switch(station) {
        case "0":
          throwingTeam.modifyScore(4);
          break;
        case "1":
          throwingTeam.modifyScore(10);
          break;
        case "2":
          throwingTeam.modifyScore(8);
          break;
        case "3":
          throwingTeam.modifyScore(6);
          break;
      }

      if (relevantTeam != null) {
        Station stationToDisable = relevantTeam.Kitchen.getStationForId(station);
        stationToDisable.DisabledTimer = disableStationDuration;
        UnityEngine.Debug.Log("Station has been disabled: " + stationToDisable);

        turnLightsOn(relevantTeam.Name, station[station.Length-1]);
      } else {
        UnityEngine.Debug.Log("Invalid team name [" + team + "], could not process station hit.");
      }
    } else {
      UnityEngine.Debug.Log("Invalid station id [" + station + "], could not process station hit.");
    }
  }

  private void MovePlayer(GameObject player, Team team, Station station){
    Vector3 newPosition = new Vector3(0f, 0f, 0f);
    newPosition.y = 1.7f;
    switch(station.Id){
      case "0":
        newPosition.z = 25f;
        break;
      case "1":
        newPosition.z = 7f;
        break;
      case "2":
        newPosition.z = -26f;
        break;
      case "3":
        newPosition.z = -14f;
        break;
      default:
        newPosition.z = 0f;
        break;
    }

    if (team.Colour == redTeamColour){
      newPosition.x = -30f;
      player.GetComponent<PlayerMovement>().movePlayer(newPosition);
    }
    else if (team.Colour == blueTeamColour){
      newPosition.x = 30f;
      player.GetComponent<PlayerMovement>().movePlayer(newPosition);
    }
  }

  public void manageOrders() {
    // Check if any orders have expired and remove some points
    float blueDeltaScore = blueTeam.checkExpiredOrders();
    float redDeltaScore = redTeam.checkExpiredOrders();

    if (redDeltaScore > 0 || blueDeltaScore > 0) {
      redTeam.Score -= (int) (redDeltaScore * negativeScoreMultiplier);
      blueTeam.Score -= (int) (blueDeltaScore * negativeScoreMultiplier);

      /* Broadcast new scores to devices */
      BroadcastScores();
    }

    if ((redTeam.NextOrderTimer <= 0 && redTeam.Orders.Count < 3) || redTeam.Orders.Count < 1) {
      redTeam.addRandomOrder(mainGameCanvas);
      redTeam.NextOrderTimer = redTeam.Orders.Count * Random.Range(minNextOrderTime, maxNextOrderTime);
    } else {
      redTeam.NextOrderTimer -= Time.deltaTime;
    }
    if ((blueTeam.NextOrderTimer <= 0 && blueTeam.Orders.Count < 3) || blueTeam.Orders.Count < 1) {
      blueTeam.addRandomOrder(mainGameCanvas);
      blueTeam.NextOrderTimer = blueTeam.Orders.Count * Random.Range(minNextOrderTime, maxNextOrderTime);
    } else {
      blueTeam.NextOrderTimer -= Time.deltaTime;
    }

    // Update the position of the orders
    redTeam.updateOrders();
    blueTeam.updateOrders();
  }

  private int clampScore(int score, int min, int max) {
    if (score > max) score = max;
    if (score < min) score = min;

    return score;
  }

  private void TickStations() {
    Team[] allTeams = new Team[] {redTeam, blueTeam};
    foreach (Team team in allTeams) {
      foreach (Station station in team.Kitchen.Stations) {
        if (station.isDisabled()) {
          station.DisabledTimer -= Time.deltaTime;
          if (station.DisabledTimer < 0) {
            station.resetTimer();
          }
        }
        station.DisablePrefab.SetActive(station.isDisabled());
        station.Prefab.SetActive(!station.isDisabled());
      }
    }
  }

  /* Broadcasts scores to all connected players in the form: score&<myScore>$<enemyScore> */
  private void BroadcastScores() {
    UnityEngine.Debug.Log("Broadcasting scores...");
    BroadcastMessageToTeam(redTeam, "score", redTeam.Score + "$" + blueTeam.Score);
    BroadcastMessageToTeam(blueTeam, "score", blueTeam.Score + "$" + redTeam.Score);
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

  /* Sets the active canvas based on the game state */
  public void SetCanvasForGameState() {
    mainMenuCanvas.gameObject.SetActive(gameState.Equals(GameState.MainMenu));
    pickModeCanvas.gameObject.SetActive(gameState.Equals(GameState.ConfigureMode));
    startGameCanvas.gameObject.SetActive(gameState.Equals(GameState.AwaitingPlayers));
    gameOverCanvas.gameObject.SetActive(gameState.Equals(GameState.EndGame));
    mainGameCanvas.gameObject.SetActive(gameState.Equals(GameState.GameRunning) || gameState.Equals(GameState.Countdown));
  }

  /* Broadcasts start */
  public void StartGame() {
    if (gameState == GameState.AwaitingPlayers) {
      BroadcastMessage("start", "");
      BroadcastScores();
      SetGameState(GameState.Countdown);
      timer.StartTimer();
    }
  }

  /* Called by GameTimer.cs */
  public void OnGameOver() {
    SetGameState(GameState.EndGame);
    redEndGameText.text = "Red score\n" + redTeam.Score;
    blueEndGameText.text = "Blue score\n" + blueTeam.Score;
    Team winningTeam = getWinningTeam();
    string broadcastMessage = "";
    if (winningTeam != null) {
      winningTeamText.text = winningTeam.Name + " team wins!";
      gameOverBackground.color = winningTeam.Colour;
      broadcastMessage += winningTeam.Name + "$";
    } else {
      /* Draw! */
      winningTeamText.text = "It was a draw!";
      gameOverBackground.color = drawColour;
      broadcastMessage += "draw$";
    }
    broadcastMessage += redTeam.Score + "$" + blueTeam.Score;
    BroadcastMessage("endgame", broadcastMessage);
  }

  /* Sets all disabled stations to active so they can be found */
  private void ShowDisabledStations() {
    Team[] allTeams = new Team[] {redTeam, blueTeam};
    foreach (Team team in allTeams) {
      foreach (Station station in team.Kitchen.Stations) {
        station.DisablePrefab.SetActive(true);
        station.Prefab.SetActive(true);
      }
    }
  }

  private void DestroyAllPlayerPrefabs() {
    Team[] allTeams = new Team[] {redTeam, blueTeam};
    foreach (Team team in allTeams) {
      foreach (ConnectedPlayer player in team.Players) {
        Destroy(player.PlayerPrefab);
      }
    }
  }

  /* Called by GameTimer.cs */
  public void OnGameStart() {
    SetGameState(GameState.GameRunning);
  }

  public void RestartGame() {
    DestroyAllPlayerPrefabs();
    ShowDisabledStations();
    SetGameState(GameState.MainMenu);
    initialiseTeams();
    timer.ResetTimer();
    wiiBlue.gameReset();
    wiiRed.gameReset();
    BroadcastAllConnections("newgame", "");
  }

  public void ExitMainScreen() {
    if (gameState == GameState.MainMenu) {
      SetGameState(GameState.ConfigureMode);
    }
  }

  public void OnLatinMode() {
    if (gameState == GameState.ConfigureMode) {
      gameMode = GameMode.Latin;
      SetGameState(GameState.AwaitingPlayers);
      FoodData.Instance.mode = "latin";
    }
  }

  public void OnFrenchMode() {
    if (gameState == GameState.ConfigureMode) {
      gameMode = GameMode.French;
      SetGameState(GameState.AwaitingPlayers);
      FoodData.Instance.mode = "french";
    }
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

  /* Sends a message to all network connections */
  private void BroadcastAllConnections(string messageType, string textInput) {
    foreach (int connectionId in allConnections) {
      UnityEngine.Debug.Log("Global Broadcasting [" + messageType + ", " + textInput + "]");
      SendMyMessage(messageType, textInput, connectionId);
    }
  }

  /* Sends a message to all connected players */
  private void BroadcastMessage(string messageType, string textInput) {
    Team[] allTeams = new Team[] {redTeam, blueTeam};
    foreach (Team team in allTeams) {
      BroadcastMessageToTeam(team, messageType, textInput);
    }
  }

  /* Sends a message to all connected players on a certain team */
  private void BroadcastMessageToTeam(Team team, string messageType, string textInput) {
    foreach (ConnectedPlayer player in team.Players) {
      UnityEngine.Debug.Log("Broadcasting [" + messageType + ", " + textInput + "]");
      SendMyMessage(messageType, textInput, player.ConnectionId);
    }
  }

  /* Simple assertion tests for teams */
  private void test() {
    /* Reset teams */
    initialiseTeams();
    gameState = GameState.AwaitingPlayers;
    Ingredient egg = new Ingredient("Eggs", "eggsPrefab");

    UnityEngine.Debug.Assert(Kitchen.isValidStation("0"));
    UnityEngine.Debug.Assert(Kitchen.isValidStation("3"));
    UnityEngine.Debug.Assert(!Kitchen.isValidStation("hello"));

    /* Simulate connections */
    OnMessageConnect(100, "connect", "red");
    OnMessageConnect(101, "connect", "red");
    OnMessageConnect(102, "connect", "red");
    UnityEngine.Debug.Assert(redTeam.Players.Count == 3);

    /* Simulate connection with duplicate id */
    OnMessageConnect(555, "connect", "blue");
    OnMessageConnect(556, "connect", "blue");
    OnMessageConnect(557, "connect", "blue");
    OnMessageConnect(555, "connect", "blue");
    UnityEngine.Debug.Assert(blueTeam.Players.Count == 3);
    UnityEngine.Debug.Assert(blueTeam.getPlayerForId(555) != null);
    UnityEngine.Debug.Assert(blueTeam.getPlayerForId(556) != null);
    UnityEngine.Debug.Assert(blueTeam.getPlayerForId(557) != null);
    UnityEngine.Debug.Assert(blueTeam.getPlayerForId(558) == null);

    /* Simulate connection with mis-spelled content */
    OnMessageConnect(558, "connect", "bule");
    UnityEngine.Debug.Assert(blueTeam.Players.Count == 3);
    UnityEngine.Debug.Assert(blueTeam.getPlayerForId(558) == null);

    /* Simulate station joining */
    OnMessageStation(101, "station", "1");
    UnityEngine.Debug.Assert(redTeam.getPlayerForId(101).CurrentStation.Id.Equals("1"));

    /* Simulate station ingredient storage */
    blueTeam.Kitchen.getStationForId("2").addIngredientToStation(egg);
    blueTeam.Kitchen.getStationForId("3").addIngredientToStation(egg);
    OnMessageStation(555, "station", "2");
    OnMessageStation(556, "station", "1");
    UnityEngine.Debug.Assert(blueTeam.getPlayerForId(557).CurrentStation == null);
    UnityEngine.Debug.Assert(blueTeam.getPlayerForId(555).CurrentStation.Ingredients.Count == 1);
    UnityEngine.Debug.Assert(blueTeam.getPlayerForId(556).CurrentStation.Ingredients.Count == 0);

    /* Simulate station clearing */
    OnMessageClear(555, "clear", "2");
    OnMessageClear(555, "clear", "3"); /* Player is not at this station, so it should not be cleared */
    UnityEngine.Debug.Assert(blueTeam.getPlayerForId(555).CurrentStation.Ingredients.Count == 0);
    UnityEngine.Debug.Assert(blueTeam.Kitchen.getStationForId("3").Ingredients.Count == 1);

    /* Station occupied tests */
    OnMessageStation(100, "station", "0");
    OnMessageStation(101, "station", "4");
    UnityEngine.Debug.Assert(redTeam.isStationOccupied("0"));
    UnityEngine.Debug.Assert(redTeam.isStationOccupied("4"));
    UnityEngine.Debug.Assert(!redTeam.isStationOccupied("3"));

    /* Checking if station is occupied before moving */
    OnMessageStation(101, "station", "4");
    OnMessageStation(100, "station", "0");
    OnMessageStation(101, "station", "0");
    UnityEngine.Debug.Assert(redTeam.isStationOccupied("0"));
    UnityEngine.Debug.Assert(redTeam.getPlayerForId(100).CurrentStation.Id.Equals("0"));
    UnityEngine.Debug.Assert(redTeam.getPlayerForId(101).CurrentStation.Id.Equals("4"));

    /* Tests for leaving stations */
    OnMessageStation(555, "station", "0");
    OnMessageStation(556, "station", "1");
    OnMessageStation(557, "station", "2");
    UnityEngine.Debug.Assert(blueTeam.isStationOccupied("1"));
    UnityEngine.Debug.Assert(blueTeam.isStationOccupied("2"));
    OnMessageLeave(556, "leave", "");
    UnityEngine.Debug.Assert(!blueTeam.isStationOccupied("1"));
    UnityEngine.Debug.Assert(blueTeam.isStationOccupied("2"));
    OnMessageLeave(557, "leave", "2");
    UnityEngine.Debug.Assert(!blueTeam.isStationOccupied("2"));
    OnMessageStation(557, "station", "2");
    UnityEngine.Debug.Assert(blueTeam.isStationOccupied("2"));

    /* Simulate adding to station */
    OnMessageStation(101, "station", "0");
    OnMessageStation(102, "station", "1");
    OnMessageAdd(101, "add", Ingredient.SerializeObject(egg));
    UnityEngine.Debug.Assert(redTeam.getPlayerForId(101).CurrentStation.Ingredients.Count == 1);
    OnMessageAdd(101, "add", Ingredient.SerializeObject(egg));
    UnityEngine.Debug.Assert(redTeam.getPlayerForId(101).CurrentStation.Ingredients.Count == 2);
    UnityEngine.Debug.Assert(redTeam.getPlayerForId(102).CurrentStation.Ingredients.Count == 0);

    /* Reset teams */
    initialiseTeams();
    gameState = GameState.AwaitingPlayers;
  }
}
