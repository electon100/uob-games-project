using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour {
    private const int MAX_CONNECTION = 10;
    private const string serverIP = "192.168.0.100";

    private int port = 8080;

    private int hostId;
    private int connectionId;

    private bool isConnected = false;

    private byte error;
    
    public void Connect () {
        Debug.Log("Button pressed");
        NetworkTransport.Init();
        ConnectionConfig connectConfig = new ConnectionConfig();

        HostTopology topo = new HostTopology(connectConfig, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo, port, null /*ipAddress*/);
        connectionId = NetworkTransport.Connect(hostId, serverIP, port, 0, out error);

        isConnected = true;
    }
	
	private void Update () {
        if (!isConnected) return;

        int recHostId; // Player ID
        int connectionId; // ID of connection to recHostId.
        int channelID; // ID of channel connected to recHostId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;

        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelID,
                                                            recBuffer, bufferSize, out dataSize, out error);

        switch (recData)
        {
            case NetworkEventType.Nothing:
                Debug.Log("A nothing Network Event.");
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("Player " + connectionId + " has been connected to server.");
                break;
            case NetworkEventType.DataEvent:
                string message = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log("Server has sent: " + message);
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Player " + connectionId + " has been disconnected to server");
                break;
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Broadcast event.");
                break;
        }
    }
}
