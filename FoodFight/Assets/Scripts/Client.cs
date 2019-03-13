using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Reflection;
using System.IO.Compression;
using System.Text.RegularExpressions;

public class Client : MonoBehaviour {

    private const int MAX_CONNECTION = 10;
    public static string serverIP = "192.168.0.101";

    public int port = 8000;

    public int hostId;

    private int reliableChannel;
    private int unreliableChannel;

    ConnectionConfig connectConfig;

    public int connectionId;

    public bool isConnected = false;
    private bool areButtonsHere = false;

    private byte error;

    public GameObject buttonPrefab;
    public GameObject startPanel;
    public GameObject warningText;
    public GameObject connectButton;
    public GameObject diffIPButton;
    public GameObject inputField;
    public GameObject changeIPButton;
    public GameObject goBackButton;
    public GameObject defaultIP;

    //NFC Stuff:
    public Text tag_output_text;

	private AndroidJavaObject mActivity;
	private AndroidJavaObject mIntent;
	private string sAction;
	private int lastTag = -1;

    // List of ingredient for each station.
    public List<Ingredient> ingredientsInStation;
    // A kitchen is a dictionary of different stations and their associated ingredients.
    public IDictionary<string, List<Ingredient>> myKitchen = new Dictionary<string, List<Ingredient>>();

    /* Current ingredient that the player is holding
       -> Can be used externally */
    public static Ingredient currentIngred;

    public GameEndState gameEndState;

    public string team;
    public bool startGame = false;

    public InputField changeIPText;

    public void Start()
    {
        //NetworkServer.Reset();
        ingredientsInStation = new List<Ingredient>();

        gameEndState = new GameEndState();

        for (int i = 0; i < 4; i++)
        {
            string stationId = i.ToString();
            myKitchen.Add(stationId, ingredientsInStation);
        }
    }

    public void Awake()
    {
        DontDestroyOnLoad(GameObject.Find("Client"));
    }

    private void Update()
    {
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

        switch (recData)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                SceneManager.LoadScene("PickTeamScreen");
                Debug.Log("Player " + connectionId + " has been connected to server.");
                break;
            case NetworkEventType.DataEvent:
                string message = OnData(hostId, connectionId, channelID, recBuffer, bufferSize, (NetworkError)error);
                manageReceiveFromServer(message);
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Player " + connectionId + " has been disconnected to server");
                NetworkTransport.Disconnect(recHostId, connectionId, out error);
                isConnected = false;
                SceneManager.LoadScene("DisconnectScreen");
                break;
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Broadcast event.");
                break;
        }

    }

    public void Connect ()
    {
        NetworkTransport.Init();
        connectConfig = new ConnectionConfig();

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

        /* Check for if there is an error */
        if ((NetworkError)error != NetworkError.Ok)
        {
            //Output this message in the console with the Network Error
            Debug.Log("There was this error : " + (NetworkError)error);
            warningText.SetActive(true);
            isConnected = false;
        }
        else {
            isConnected = true;
        }

    }

    public string serialiseIngredient(Ingredient ingredient)
    {
        byte[] buffer = new byte[4096];
        int bufferSize = 4096;
        Stream message = new MemoryStream(buffer);
        BinaryFormatter formatter = new BinaryFormatter();
        //Serialize the message
        Ingredient ingredientString = ingredient;
        formatter.Serialize(message, ingredientString);

        return ingredientString.ToString();
    }

    public void SendMyMessage(string messageType, string textInput)
    {
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

    // Splits up a string based on a given character
    private string[] decodeMessage(string message, char character)
    {
        string[] splitted = message.Split(character);
        return splitted;
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

    public void manageReceiveFromServer(string message)
    {
        string messageType = decodeMessage(message, '&')[0];
        string messageContent = decodeMessage(message, '&')[1];

        switch (messageType)
        {
            case "station":
                string[] data = decodeMessage(messageContent, '$');
                string stationId = data[0];
                for (int i = 1; i < data.Length; i++)
                {
                    //receives whole string with flags, e.g. Eggs^0^2
                    if (data[i] != "")
                    {
                        string receivedIngredient = data[i];
                        Ingredient received = new Ingredient();
                        received = Ingredient.XmlDeserializeFromString<Ingredient>(receivedIngredient, received.GetType());
                        ingredientsInStation.Add(received);
                    }
                }

                myKitchen[stationId] = ingredientsInStation;
                logAppropriateStation(stationId);
                ingredientsInStation = new List<Ingredient>();
                break;
            case "endgame":
                string[] details = messageContent.Split('$');
                string winningTeam = details[0];
                string redScoreStr = details[1];
                string blueScoreStr = details[2];

                int redScore = 0;
                int blueScore = 0;

                int.TryParse(redScoreStr, out redScore);
                int.TryParse(blueScoreStr, out blueScore);

                gameEndState = new GameEndState(winningTeam, redScore, blueScore);

                Debug.Log("END GAME: " + winningTeam + " " + redScore + " " + blueScore);

                Player.removeCurrentIngredient();

                SceneManager.LoadScene("PlayerGameOverScreen");
                break;
            case "team":
                team = messageContent;
                break;
            case "start":
                startGame = true;
                break;
            default:
                break;
        }
    }

    private void logAppropriateStation(string stationId) {
      if (!stationId.Equals(Player.currentStation)) {
        switch(stationId) {
            case "0":
                Player.ingredientsFromStation = getIngredientsFromStation("0");
                SceneManager.LoadScene("CupboardStation");
                break;
            case "1":
                Player.ingredientsFromStation = getIngredientsFromStation("1");
                SceneManager.LoadScene("FryingStation");
                break;
            case "2":
                Player.ingredientsFromStation = getIngredientsFromStation("2");
                SceneManager.LoadScene("ChoppingStation");
                break;
            case "3":
                Player.ingredientsFromStation = getIngredientsFromStation("3");
                SceneManager.LoadScene("PlatingStation");
                break;
            default:
                break;
        }
      }
    }

    public List<Ingredient> getIngredientsFromStation(string stationID)
    {
        return myKitchen[stationID];
    }

    public void onClickRed()
    {
        SendMyMessage("connect", "red");
        SceneManager.LoadScene("LobbyScreen");
    }

    public void onClickBlue()
    {
        SendMyMessage("connect", "blue");
        SceneManager.LoadScene("LobbyScreen");
    }

    public void useDifferentIP() {
        connectButton.SetActive(false);
        diffIPButton.SetActive(false);
        inputField.SetActive(true);
        changeIPButton.SetActive(true);
        goBackButton.SetActive(true);
        defaultIP.SetActive(true);
    }

    public void changeIP()
    {
        serverIP = "192.168.0." + Regex.Replace(changeIPText.text, @"\t|\n|\r", "");
        inputField.SetActive(false);
        changeIPButton.SetActive(false);
        connectButton.SetActive(true);
        diffIPButton.SetActive(true);
        goBackButton.SetActive(false);
        defaultIP.SetActive(false);
    }

    public void goBack() {
      inputField.SetActive(false);
      changeIPButton.SetActive(false);
      connectButton.SetActive(true);
      diffIPButton.SetActive(true);
      goBackButton.SetActive(false);
      defaultIP.SetActive(false);
    }

    private string FirstLetterToUpper(string str)
    {
        if (str == null)
            return null;

        if (str.Length > 1)
            return char.ToUpper(str[0]) + str.Substring(1);

        return str.ToUpper();
    }

    public GameEndState getGameEndState() {
      return gameEndState;
    }

    public string getTeam() {
      return team;
    }

}
