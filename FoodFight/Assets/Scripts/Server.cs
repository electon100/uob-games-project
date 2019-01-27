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
                //If this station already exists, check what's in it and send it back to player.
                string[] words = decodeMessage(messageContent, '$');
                string stationId = words[0];
                
                string ingredient = words[1];
                Debug.Log("Word 0: " + stationId);
                Debug.Log("Word 1: " + ingredient);

                // Case where we want to send back ingredient stored at the station to player
                if (ingredient.Equals(""))
                {
                    if (redKitchen.ContainsKey(stationId))
                    {
                        checkCurrentIngredient("red", stationId, connectionId);
                    }
                    else if (blueKitchen.ContainsKey(stationId))
                    {
                        checkCurrentIngredient("blue", stationId, connectionId);
                    }
                }
                
                //If the player wants to add an ingredient, add it
                else 
                {
                    if (redTeam.ContainsKey(connectionId))
                    {
                        if (redKitchen.ContainsKey(stationId))
                        {
                            redKitchen[stationId] += "$";
                            redKitchen[stationId] += ingredient;
                            Debug.Log("Added " + ingredient + " to red kitchen station. Now " + redKitchen[stationId]);
                        }
                        else {
                            redKitchen.Add(stationId, ingredient);
                            Debug.Log("Created new red station with ingredient list: " + redKitchen[stationId]);
                        }

                        checkCurrentIngredient("red", stationId, connectionId);
                    }
                    else
                    {
                        if (blueKitchen.ContainsKey(stationId))
                        {
                            blueKitchen[stationId] += "$";
                            blueKitchen[stationId] += ingredient;
                            Debug.Log("Added " + ingredient + " to blue kitchen station. Now " + blueKitchen[stationId]);
                        }
                        else
                        {
                            blueKitchen.Add(stationId, ingredient);
                            Debug.Log("Created new blue station with ingredient list: " + blueKitchen[stationId]);
                        }

                        checkCurrentIngredient("blue", stationId, connectionId);
                    }
                }
                break;

            // Player sends NFC data
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

    // Used for sending data to the players
    public void SendMyMessage(string messageType, string textInput, int connectionId)
    {
        byte error;
        byte[] buffer = new byte[1024];
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
        {
            createRedPlayer(connectionId);
        }
        else if (message == "blue")
        {
            createBluePlayer(connectionId);
        }
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

    private void checkCurrentIngredient(string kitchen, string station, int hostId)
    {
        if (kitchen == "red")
        {
            SendMyMessage("", redKitchen[station], hostId);
            Debug.Log("Sent red kitchen list to player: " + redKitchen[station]);
        }
        else if (kitchen == "blue")
        {
            SendMyMessage("", blueKitchen[station], hostId);
            Debug.Log("Sent blue kitchen list to player: " + blueKitchen[station]);
        }
    }
}
