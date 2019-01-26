using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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

    IDictionary<int, GameObject> redTeam = new Dictionary<int, GameObject>();
    IDictionary<int, GameObject> blueTeam = new Dictionary<int, GameObject>();

    // dictionary <station, status>
    IDictionary<string, string> redKitchen = new Dictionary<string, string>();
    IDictionary<string, string> blueKitchen = new Dictionary<string, string>();

    private void Start () {
        NetworkTransport.Init();
        ConnectionConfig connectConfig = new ConnectionConfig();

        reliableChannel = connectConfig.AddChannel(QosType.ReliableSequenced);
        HostTopology topo = new HostTopology(connectConfig, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo, port, null /*ipAddress*/);
        webHostId = NetworkTransport.AddWebsocketHost(topo, port, null /*ipAddress*/);

        isStarted = true;
    }
	
	private void Update () {
        if (!isStarted) return;

        int recHostId; // Player ID
        int connectionId; // ID of connection to recHostId.
        int channelID; // ID of channel connected to recHostId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;

        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelID,
                                                            recBuffer, bufferSize, out dataSize, out error);

        //Networking events
        switch (recData)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("Player " + connectionId + " has connected");
                break;
            case NetworkEventType.DataEvent:
                string message = OnData(hostId, connectionId, channelID, recBuffer, bufferSize, (NetworkError)error);
                manageMessageEvents(message, connectionId);
                break;
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
        string messageType = decodeMessage(message)[0];
        string messageContent = decodeMessage(message)[1];
        Debug.Log(message);
        switch(messageType)
        {
            case "connect":
                if (redTeam.ContainsKey(connectionId) || blueTeam.ContainsKey(connectionId)){
                    break;
                }
                else {
                    messageContent = decodeMessage(message)[1];
                    allocateToTeam(connectionId, messageContent);
                }
                break;
            case "station":
                //If this station already exists, check what's in it and send it back to player.
                if (redKitchen.ContainsKey(messageContent))
                {
                    checkCurrentIngredient("red", messageContent);
                }
                else if (redKitchen.ContainsKey(messageContent))
                {
                    checkCurrentIngredient("blue", messageContent);
                }
                //If this is the first time a player has logged into that station, initialise it.
                else 
                {
                    if (redTeam.ContainsKey(connectionId))
                    {
                        redKitchen.Add(messageContent, "pancake");
                    }
                    else
                    {
                        blueKitchen.Add(messageContent, "potato");
                    }
                }
                break;
            case "NFC":
                //Do NFC stuff
                Debug.Log("Player " + connectionId + " has sent: " + messageContent);
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

    //Allocates a player to a team based on their choice.
    private void allocateToTeam(int connectionId, string message)
    {
        if (message == "red")
        {
            createRedPlayer(connectionId);
        }
        else if (message == "blue")
        {
            createBluePlayer(connectionId);
        }
    }

    private string[] decodeMessage(string message)
    {
        string[] splitted = Regex.Split(message, "&");

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
        {
            return redTeam;
        }
        else if (blueTeam.ContainsKey(connectionID))
        {
            return blueTeam;
        }
        else
        {
            return null;
        }
    }

    private checkCurrentIngredient(string kitchen, string station)
    {
        if (kitchen == "red")
        {
            Debug.Log(redKitchen.TryGetValue(station));
        }
        else if (kitchen == "blue")
        {
            Debug.Log(blueKitchen.TryGetValue(station));
        }
    }
}
