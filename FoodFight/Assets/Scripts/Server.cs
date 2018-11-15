using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;

public class Server : MonoBehaviour {
    private const int MAX_CONNECTION = 10;

    private int port = 8080;

    private int hostId;
    private int webHostId;

    private bool isStarted = false;

    private byte error;
    public Transform brick;
    private void Start () {
        NetworkTransport.Init();
        ConnectionConfig connectConfig = new ConnectionConfig();

        HostTopology topo = new HostTopology(connectConfig, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo, port, null /*ipAddress*/);
        webHostId = NetworkTransport.AddWebsocketHost(topo, port, null /*ipAddress*/);

        isStarted = true;

        Instantiate(brick, new Vector3(1, 2, 1), Quaternion.identity);
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

        switch (recData)
        {
            case NetworkEventType.Nothing:
                createPlayerPrefab(0);
                Debug.Log("A nothing Network Event.");
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("Player " + connectionId + " has connected");
                createPlayerPrefab(connectionId);
                break;
            case NetworkEventType.DataEvent:
                string message = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log("Player " + connectionId + " has sent: " + message);
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Player " + connectionId + " has disconnected");
                break;
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Broadcast event.");
                break;
        }

        return;
	}

    private void createPlayerPrefab(int connectiondId)
    {
        //        GameObject prefab = (GameObject)Resources.Load("/Assets/Prefabs/Player.prefab");
        //        GameObject player = Instantiate(prefab);
        

    }
}
