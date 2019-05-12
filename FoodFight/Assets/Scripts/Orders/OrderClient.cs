using System;
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
using System.Linq;

public class OrderClient : MonoBehaviour {

	private const int MAX_CONNECTION = 10;
	public int port = 8000;
	private readonly string serverIPBase = "192.168.0."; // The base IP
	private string serverIPSuffix = "100"; // The default IP suffix
	public int hostId = 0;
	public int connectionId, reliableChannel;

	public bool isConnected = false;
	public bool startGame = false;
	public static bool isJoined = false;
	private string currentScene = "";

	private string mode = "";
	private string order = "";
	private string team = "";

	private Text warningText;

	public Button button1, button2, button3, button4, button5, button6;
	public GameObject recipePanel, teamPanel, cuisinePanel, connectPanel, completePanel;
	public Text orderText, teamText;

	private static readonly string[] frenchOrders = {"crepe", "chips", "omlette", "quiche", "ratatouille", "steak_hache"};
	private static readonly string[] spanishOrders = {"calamari", "churros", "paella", "patatas_bravas", "quesadilla", "spanish_omelette"};

	private List<Button> buttons = new List<Button>();

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(GameObject.Find("Client"));
		warningText = GameObject.Find("ConnectionFailedText").GetComponent<Text>();

		Screen.orientation = ScreenOrientation.Portrait;

		buttons.Add(button1);
		buttons.Add(button2);
		buttons.Add(button3);
		buttons.Add(button4);
		buttons.Add(button5);
		buttons.Add(button6);
	}

	// Update is called once per frame
	void Update () {
		listenForData();

		if (isConnected) {
			isJoined = true;
		}
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
		connectionId = NetworkTransport.Connect(hostId, getServerIp(), port, 0, out error);

		/* Check if there is an error */
		if ((NetworkError) error != NetworkError.Ok)
		{
			//Output this message in the console with the Network Error
			Debug.Log("There was this error: " + (NetworkError) error);
			isConnected = false;
		} else {
			isConnected = true;
			OnConnect();
		}

		switch ((NetworkError) error) {
			case NetworkError.WrongHost :
				warningText.text = "WrongHost";
				break;
			case NetworkError.WrongConnection :
				warningText.text = "WrongConnection";
				break;
			default :
				warningText.text = "";
				break;
		}
	}

	public void listenForData() {
		if (!isConnected) {
			return;
		}

		int recHostId; // Player ID
		int connectionId; // Connection ID
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
					break;
			case NetworkEventType.BroadcastEvent:
					Debug.Log("Broadcast event.");
					break;
		}
	}

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
		Debug.Log("Sending " + messageToSend);
		warningText.text = "Sending " + messageToSend;
		//Send the message from the "client" with the serialized message and the connection information
		Debug.Log("Host: " + hostId + " connection id: " + connectionId + " channel " + reliableChannel);
		NetworkTransport.Send(hostId, connectionId, reliableChannel, buffer, (int)message.Position, out error);

		//If there is an error, output message error to the console
		if ((NetworkError)error != NetworkError.Ok)
		{
				Debug.Log("Message send error: " + (NetworkError)error);
				warningText.text = "Could not send message";
		}

		switch ((NetworkError) error) {
			case NetworkError.WrongHost :
				warningText.text = "WrongHost";
				break;
			case NetworkError.WrongConnection :
				warningText.text = "WrongConnection";
				break;
			default :
				warningText.text = "";
				break;
		}
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
			case "endgame": // Called when the game is ended with the name of the winning team and the relevant scores
				OnGameEnd(messageType, messageContent);
				break;
			case "newgame": // Called after the new game button is pressed on the server, resets the whole game state
				Debug.Log("Resetting game state");
				OnGameReset();
				break;
      case "start": // Broadcasted from the server when the required number of players is reached
        Debug.Log("Starting game...");
        startGame = true;
        break;
      default:
        Debug.Log("Invalid message type.");
        break;
		}
	}

	private void OnGameEnd(string messageType, string messageContent) {
		resetUI();
	}

	public void OnGameReset() {
		resetUI();
	}

	private void OnConnect() {
		connectPanel.SetActive(false);
		cuisinePanel.SetActive(true);
	}

	public void onRed() {
		team = "red";

		teamPanel.SetActive(false);
		loadOrderCompletePanel();
	}

	public void onBlue() {
		team = "blue";

		teamPanel.SetActive(false);
		loadOrderCompletePanel();
	}

	public void onRecipe(int buttonIndex) {
		if (mode.Equals("french")) {
			order = frenchOrders[buttonIndex];
		} else {
			order = spanishOrders[buttonIndex];
		}

		recipePanel.SetActive(false);
		teamPanel.SetActive(true);
	}

	public void OnOderAgain() {
		completePanel.SetActive(false);
		cuisinePanel.SetActive(true);
	}

	public void onFrench() {
		setButtons(frenchOrders);
		mode = "french";

		cuisinePanel.SetActive(false);
		recipePanel.SetActive(true);
	}

	public void onSpanish() {
		setButtons(spanishOrders);
		mode = "spanish";

		cuisinePanel.SetActive(false);
		recipePanel.SetActive(true);
	}

	private void setButtons(string[] orders) {
		for (int i  = 0; i < 6; i++) {
			string orderName = orders[i];
			buttons[i].GetComponentInChildren<Text>().text = orderName.First().ToString().ToUpper() + orderName.Replace('_', ' ').Substring(1);
		}
	}

	private string getServerIp() {
		return serverIPBase + serverIPSuffix;
	}

	private void loadOrderCompletePanel() {
		completePanel.SetActive(true);
		orderText.text = order.First().ToString().ToUpper() + order.Replace('_', ' ').Substring(1);
		teamText.text = team + " Team";

		SendMyMessage("order", team + "$" + order);
	}

	private void resetUI() {
		recipePanel.SetActive(false);
		teamPanel.SetActive(false);
		cuisinePanel.SetActive(false);
		completePanel.SetActive(false);
		connectPanel.SetActive(true);
	}

}
