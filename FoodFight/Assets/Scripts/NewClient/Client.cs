using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour {

  private const int MAX_CONNECTION = 10;
  public int port = 8000;
	public static string serverIP = "192.168.0.100";
  public int hostId = 0;
	public int connectionId, reliableChannel;

	public bool isConnected = false;
	public bool startGame = false;
	public string team;

	public List<Ingredient> ingredientsInStation = new List<Ingredient>();

	public GameEndState gameEndState;

	public GameObject buttonPrefab;
	public GameObject startPanel;
	public GameObject warningText;
	public GameObject connectButton;
	public GameObject diffIPButton;
	public GameObject inputField;
	public GameObject changeIPButton;
	public GameObject goBackButton;
	public GameObject defaultIP;
	public InputField changeIPText;
	public Text gameNotRunningText;

	public void Start() {
    DontDestroyOnLoad(GameObject.Find("Client"));
	}

	public void Update() {
		listenForData();
	}

	public void Connect() {
		NetworkTransport.Init();
		ConnectionConfig connectConfig = new ConnectionConfig();

		byte error;
    string myIPaddress;

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
		connectConfig.OverflowDropThreshold = 40; //
		connectConfig.PacketSize = 1500;
		connectConfig.PingTimeout = 500;
		connectConfig.ReducedPingTimeout = 100;
		connectConfig.ResendTimeout = 500;

		reliableChannel = connectConfig.AddChannel(QosType.ReliableSequenced);
		HostTopology topo = new HostTopology(connectConfig, MAX_CONNECTION);

		if (hostId >= 0) {
			NetworkTransport.RemoveHost(hostId);
		}
		hostId = NetworkTransport.AddHost(topo, port, null /*ipAddress*/);
		connectionId = NetworkTransport.Connect(hostId, serverIP, port, 0, out error);

		/* Check if there is an error */
		if ((NetworkError)error != NetworkError.Ok)
		{
			//Output this message in the console with the Network Error
			Debug.Log("There was this error : " + (NetworkError)error);
			NetworkTransport.Disconnect(hostId, connectionId, out error);
			isConnected = false;
			NetworkTransport.RemoveHost(hostId);
			warningText.SetActive(true);
		}
		else {
			isConnected = true;
		}
	}

	// Client always listen for incoming data
	public void listenForData() {
		if (!isConnected) {
			return;
		}

		int recHostId; // Player ID
		int connectionId; // ID of connection to recHostId.
		int channelID; // ID of channel connected to recHostId.
		byte[] recBuffer = new byte[4096];
		int bufferSize = 4096;
		int dataSize;
		byte error;

		NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelID,
																												recBuffer, bufferSize, out dataSize, out error);

		switch (recData) {
			case NetworkEventType.Nothing:
					break;
			case NetworkEventType.ConnectEvent:
					Debug.Log("Player " + connectionId + " has been connected to server.");
					SceneManager.LoadScene("PickTeamScreen");
					break;
			case NetworkEventType.DataEvent:
					string message = OnData(hostId, connectionId, channelID, recBuffer, bufferSize, (NetworkError)error);
          manageMessageEvents(message);
					break;
			case NetworkEventType.DisconnectEvent:
					Debug.Log("Player " + connectionId + " has been disconnected to server");
					NetworkTransport.Disconnect(hostId, connectionId, out error);
					NetworkTransport.RemoveHost(hostId);
					startGame = false;
					isConnected = false;
					SceneManager.LoadScene("DisconnectScreen");
					break;
			case NetworkEventType.BroadcastEvent:
					Debug.Log("Broadcast event.");
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

	// Sends a message across the network
	public void SendMyMessage(string messageType, string textInput){
		byte error;
		byte[] buffer = new byte[4096];
		int bufferSize = 4096;
		Stream message = new MemoryStream(buffer);
		BinaryFormatter formatter = new BinaryFormatter();
		//Serialize the message
		string messageToSend = messageType + "&" + textInput;
		formatter.Serialize(message, messageToSend);
		Debug.Log("Sending station " + messageToSend);
		//Send the message from the "client" with the serialized message and the connection information
		NetworkTransport.Send(hostId, connectionId, reliableChannel, buffer, (int)message.Position, out error);

		//If there is an error, output message error to the console
		if ((NetworkError)error != NetworkError.Ok)
		{
				Debug.Log("Message send error: " + (NetworkError)error);
		}
	}

	//This function serialises the object of type Ingredient to an XML string
	public string SerialiseIngredient(Ingredient ingredient) {
		byte[] buffer = new byte[4096];
		int bufferSize = 4096;
		Stream message = new MemoryStream(buffer);
		BinaryFormatter formatter = new BinaryFormatter();
		//Serialize the message
		Ingredient ingredientString = ingredient;
		formatter.Serialize(message, ingredientString);

		return ingredientString.ToString();
	}

	// Splits up a string based on a given character
	private string[] decodeMessage(string message, char character)
	{
		string[] splitted = message.Split(character);
		return splitted;
	}

	// This is where all the work happens.
  public void manageMessageEvents(string message) {
		string[] decodedMessage = decodeMessage(message, '&');
		string messageType = decodedMessage[0];
		string messageContent = decodedMessage[1];

		switch (messageType) {
			case "station": // Player wants to find out what ingredients are in a station after they log in
				OnStationEnter(messageType, messageContent);
				break;
			case "endgame": // Called when the game is ended with the name of the winning team and the relevant scores
				OnGameEnd(messageType, messageContent);
				break;
      case "connect": // Called after a join team event, for the player to find out which team they are on and load the lobby
				OnConnect(messageContent);
        break;
      case "start": // Broadcasted from the server when the required number of players is reached
        Debug.Log("Starting game...");
        startGame = true;
        break;
			case "add":
				Debug.Log("Adding ingredient failed.");
				break;
			case "leave":
				Debug.Log("Leaving station failed.");
				break;
			case "clear":
				Debug.Log("Clearing ingredients failed.");
				break;
      default:
        Debug.Log("Invalid message type.");
        break;
		}
	}

	private void OnStationEnter(string messageType, string messageContent) {
		string[] data = decodeMessage(messageContent, '$');
		string stationId = data.Length > 0 ? data[0] : "";

    if (stationId != "") {
      if (Kitchen.isValidStation(stationId)) {

				/* Ingredients are separated by $, so iterate over them and deserialise, adding them to the list of ingredients */
				for (int i = 1; i < data.Length; i++) {
					if (data[i] != ""){
						string receivedIngredient = data[i];
						Ingredient received = new Ingredient();
						received = Ingredient.XmlDeserializeFromString<Ingredient>(receivedIngredient, received.GetType());
						ingredientsInStation.Add(received);
					} else {
						Debug.Log("No ingredients currently exist in that station");
					}
				}

				/* Log the player into the station with that list of ingredients */
				logAppropriateStation(stationId);
				/* Clear out the list of ingredients in that station, since the player already has a referance to them */
				ingredientsInStation = new List<Ingredient>();
			} else if (stationId.Equals("Station disabled")) {
				Debug.Log("Station is disabled.");
				Player.resetCurrentStation();
			} else if (stationId.Equals("Station occupied")) {
				Debug.Log("Station is already occupied.");
				Player.resetCurrentStation();
			} else if (stationId.Equals("Already at station")) {
				Debug.Log("Already at station.");
			} else {
				Debug.Log("Error: invalid station");
			}
    } else {
			Debug.Log("Error: no station sent");
		}

	}

	private void OnGameEnd(string messageType, string messageContent) {
		string[] details = messageContent.Split('$');

		/* Check if content is empty */
		if (messageContent != "") {
			string winningTeam = details[0];
			string redScoreStr = details[1];
			string blueScoreStr = details[2];

			/* Check if all details are present */
			if (winningTeam != "" && redScoreStr != "" && blueScoreStr != "") {
				int redScore = 0;
				int blueScore = 0;

				/* Convert the received string to integer (score) */
				int.TryParse(redScoreStr, out redScore);
				int.TryParse(blueScoreStr, out blueScore);

				/* Initialise the game end state and load the scene */
				gameEndState = new GameEndState(winningTeam, redScore, blueScore);
				Debug.Log("END GAME: " + winningTeam + " " + redScore + " " + blueScore);
				Player.removeCurrentIngredient();
				SceneManager.LoadScene("PlayerGameOverScreen");
			} else {
				SendMyMessage(messageType, "Error: one of the details is missing");
			}
		} else {
			SendMyMessage(messageType, "Error: no details about end game");
		}
	}

	private void OnConnect(string messageContent) {
		if (messageContent == "red" || messageContent == "blue") {
			team = messageContent;
			SceneManager.LoadScene("LobbyScreen");
		} else {
			/* Error: could not proceed to lobby */
			// gameNotRunningText = GameObject.Find("GameNotRunningText").GetComponent<Text>();
			// // gameNotRunningText.gameObject.SetActive(true);
			// gameNotRunningText.text = "Game is not running. Please try again.";
			Debug.Log("Error: [" + messageContent + "]");
		}
	}

	private void logAppropriateStation(string stationId) {
		string currentScene = SceneManager.GetActiveScene().name;

		Debug.Log(currentScene);

		switch(stationId) {
			case "0": // Cupboard Minigame
				if (!currentScene.Equals("CupboardStation")) {
					Player.ingredientsFromStation = ingredientsInStation;
					SceneManager.LoadScene("CupboardStation");
				}
				break;
			case "1": // Frying Minigame
				if (!currentScene.Equals("FryingStation")) {
					Player.ingredientsFromStation = ingredientsInStation;
					SceneManager.LoadScene("FryingStation");
				}
				break;
			case "2": // Chopping Minigame
				if (!currentScene.Equals("ChoppingStation")) {
					Player.ingredientsFromStation = ingredientsInStation;
					SceneManager.LoadScene("ChoppingStation");
				}
				break;
			case "3": // Plating Minigame
				if (!currentScene.Equals("PlatingStation")) {
					Player.ingredientsFromStation = ingredientsInStation;
					SceneManager.LoadScene("PlatingStation");
				}
				break;
			case "4": // Fighting Minigame
				if (!currentScene.Equals("FightingStation")) {
					Player.ingredientsFromStation = ingredientsInStation;
					SceneManager.LoadScene("FightingStation");
				}
				break;
			default:
					break;
		}
	}

	// All of the functions below are used for buttons
	public void onClickRed() {
		SendMyMessage("connect", "red");
	}

	public void onClickBlue() {
		SendMyMessage("connect", "blue");
	}

	public void useDifferentIP() {
		connectButton.SetActive(false);
		diffIPButton.SetActive(false);
		inputField.SetActive(true);
		changeIPButton.SetActive(true);
		goBackButton.SetActive(true);
		defaultIP.SetActive(true);
	}

	public void changeIP() {
		serverIP = "192.168.0." + Regex.Replace(changeIPText.text, @"\t|\n|\r", "");
		inputField.SetActive(false);
		changeIPButton.SetActive(false);
		goBackButton.SetActive(false);
		defaultIP.SetActive(false);
		connectButton.SetActive(true);
		diffIPButton.SetActive(true);
	}

	public void goBack() {
		inputField.SetActive(false);
		changeIPButton.SetActive(false);
		goBackButton.SetActive(false);
		defaultIP.SetActive(false);
		connectButton.SetActive(true);
		diffIPButton.SetActive(true);
	}

}