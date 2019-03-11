using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class NetManager : MonoBehaviour {

    private const int MAX_CONNECTION = 10;

    private int port = 8000;

    private int hostId;
    private int webHostId;

    private int reliableChannel;
    private int unreliableChannel;

    private bool isStarted = false;

    private byte error;

    public string eventType;
    public string message;

    public Server server;

    // Use this for initialization
    void Start() {
        server = GameObject.Find("Server").GetComponent<Server>();

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
        connectConfig.OverflowDropThreshold = 40; //
        connectConfig.PacketSize = 1500;
        connectConfig.PingTimeout = 500;
        connectConfig.ReducedPingTimeout = 100;
        connectConfig.ResendTimeout = 500;

        reliableChannel = connectConfig.AddChannel(QosType.ReliableSequenced);
        HostTopology topo = new HostTopology(connectConfig, MAX_CONNECTION);

        //NetworkServer.Reset();
        hostId = NetworkTransport.AddHost(topo, port, null /*ipAddress*/);
        webHostId = NetworkTransport.AddWebsocketHost(topo, port, null /*ipAddress*/);
        isStarted = true;
    }

  	// Update is called once per frame
  	void Update() {
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
                eventType = "nothing";
                break;
            // Have a phone connect to the server
            case NetworkEventType.ConnectEvent:
                eventType = "connect";
                Debug.Log("Player " + connectionId + " has connected");
                break;
            // Have the phone send data to the server
            case NetworkEventType.DataEvent:
                eventType = "data";
                message = OnData(hostId, connectionId, channelID, recBuffer, bufferSize, (NetworkError)error);
                break;
            // Remove the player from the game
            case NetworkEventType.DisconnectEvent:
                eventType = "disconnect";
                Debug.Log("Player " + connectionId + " has disconnected");
                break;
            case NetworkEventType.BroadcastEvent:
                eventType = "broadcast";
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

        server.manageMessageEvents(message, connectionId);

        return message;
    }

    // Used for sending data to the players
    public void SendMyMessage(string messageType, string textInput, int connectionId)
    {
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
        if ((NetworkError)error != NetworkError.Ok)
        {
            Debug.Log("Message send error: " + (NetworkError)error);
        }
    }
}
