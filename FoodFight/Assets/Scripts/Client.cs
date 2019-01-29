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

public class Client : MonoBehaviour {

    private const int MAX_CONNECTION = 10;
    private const string serverIP = "192.168.0.101";

    private int port = 8000;

    private int hostId;

    private int reliableChannel;
    private int unreliableChannel;

    ConnectionConfig connectConfig;

    private int connectionId;

    private bool isConnected = false;
    private bool areButtonsHere = false;

    private byte error;

    public GameObject buttonPrefab;
    public GameObject startPanel;

    //NFC Stuff:
    public Text tag_output_text;

	private AndroidJavaObject mActivity;
	private AndroidJavaObject mIntent;
	private string sAction;
	private int lastTag = -1;

    private GameObject currentItem;

    // List of ingredient for each station.
    public List<Ingredient> ingredientsInStation;
    // A kitchen is a dictionary of different stations and their associated ingredients.
    public IDictionary<string, List<Ingredient>> myKitchen = new Dictionary<string, List<Ingredient>>();

    private string currentStation = "-1";

    /* Current ingredient that the player is holding
       -> Can be used externally */
    public static Ingredient currentIngred;

    public void Start()
    {
        ingredientsInStation = new List<Ingredient>();

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
        if (!isConnected) return;

        int recHostId; // Player ID
        int connectionId; // ID of connection to recHostId.
        int channelID; // ID of channel connected to recHostId.
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;

        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelID,
                                                            recBuffer, bufferSize, out dataSize, out error);

        if (!areButtonsHere)
        {
            initialiseStartButtons();
            areButtonsHere = true;
        }

        switch (recData)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("Player " + connectionId + " has been connected to server.");
                break;
            case NetworkEventType.DataEvent:
                string message = OnData(hostId, connectionId, channelID, recBuffer, bufferSize, (NetworkError)error);
                manageReceiveFromServer(message);
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Player " + connectionId + " has been disconnected to server");
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

        reliableChannel = connectConfig.AddChannel(QosType.ReliableSequenced);
        HostTopology topo = new HostTopology(connectConfig, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo, port, null /*ipAddress*/);
        connectionId = NetworkTransport.Connect(hostId, serverIP, port, 0, out error);

        isConnected = true;
    }

    private void initialiseStartButtons ()
    {
        GameObject redButton = (GameObject) Instantiate(buttonPrefab, new Vector3(-100, -17, 0), Quaternion.identity);
        redButton.transform.SetParent(startPanel.transform);//Setting button parent
        redButton.GetComponent<Button>().onClick.AddListener(onClickRed);//Setting what button does when clicked
        redButton.transform.GetChild(0).GetComponent<Text>().text = "Red Team";//Changing text
        Color redCol;
        if (ColorUtility.TryParseHtmlString("#FF7000", out redCol))
        {
            redButton.GetComponent<Image>().color = redCol;//Changing colour
        }

        GameObject blueButton = (GameObject)Instantiate(buttonPrefab, new Vector3(100, -17, 0), Quaternion.identity);
        blueButton.transform.SetParent(startPanel.transform);//Setting button parent
        blueButton.GetComponent<Button>().onClick.AddListener(onClickBlue);//Setting what button does when clicked
        blueButton.transform.GetChild(0).GetComponent<Text>().text = "Blue Team";//Changing text
        Color blueCol;
        if (ColorUtility.TryParseHtmlString("#00ACFF", out blueCol))
        {
            blueButton.GetComponent<Image>().color = blueCol;//Changing colour
        }

        Destroy(GameObject.Find("ConnectButton"));
    }

    public void SendMyMessage(string messageType, string textInput)
    {
        byte error;
        byte[] buffer = new byte[1024];
        int bufferSize = 1024;
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
                        string ingredientString = FirstLetterToUpper(data[i]);
                        string[] ingredientStringList = decodeMessage(ingredientString, '^');
                        string ingredientName = ingredientStringList[0];
                        string prefab = ingredientName + "Prefab";
                        Ingredient ingredientToAdd = new Ingredient(ingredientName, (GameObject)Resources.Load(prefab, typeof(GameObject)));
                        ingredientToAdd.translateToIngredient(ingredientString);
                        ingredientsInStation.Add(ingredientToAdd);
                    }
                }

                myKitchen[stationId] = ingredientsInStation;
                ingredientsInStation = new List<Ingredient>();
                break;
            default:
                break;
        }
    }

    public List<Ingredient> getIngredientsFromStation(string stationID)
    {        
        return myKitchen[stationID];
    }

    public void onClickRed()
    {
        SendMyMessage("connect", "red");
        SceneManager.LoadScene("PlayerMainScreen");
    }

    public void onClickBlue()
    {
        SendMyMessage("connect", "blue");
        SceneManager.LoadScene("PlayerMainScreen");
    }

    private string FirstLetterToUpper(string str)
    {
        if (str == null)
            return null;

        if (str.Length > 1)
            return char.ToUpper(str[0]) + str.Substring(1);

        return str.ToUpper();
    }

}
