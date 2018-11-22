using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Client : MonoBehaviour {
    private const int MAX_CONNECTION = 10;
    private const string serverIP = "192.168.0.100";

    private int port = 8080;

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

    public void Connect ()
    {
        NetworkTransport.Init();
        connectConfig = new ConnectionConfig();

        reliableChannel = connectConfig.AddChannel(QosType.ReliableSequenced);
        HostTopology topo = new HostTopology(connectConfig, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo, port, null /*ipAddress*/);
        connectionId = NetworkTransport.Connect(hostId, serverIP, port, 0, out error);

        isConnected = true;

        if (! areButtonsHere)
        {
            initialiseStartButtons();
            areButtonsHere = true;
        }
    }

    public void onClickRed()
    {
        Debug.Log("red");
        SendMyMessage("red");
    }

    public void onClickBlue()
    {
        Debug.Log("blue");
        SendMyMessage("blue");
    }

    private void initialiseStartButtons ()
    {
        GameObject redButton = (GameObject) Instantiate(buttonPrefab, new Vector3(-100, -17, 0), Quaternion.identity);
        redButton.transform.SetParent(startPanel.transform);//Setting button parent
        redButton.GetComponent<Button>().onClick.AddListener(onClickRed);//Setting what button does when clicked
        redButton.transform.GetChild(0).GetComponent<Text>().text = "Red Team";//Changing text

        GameObject blueButton = (GameObject)Instantiate(buttonPrefab, new Vector3(100, -17, 0), Quaternion.identity);
        blueButton.transform.SetParent(startPanel.transform);//Setting button parent
        blueButton.GetComponent<Button>().onClick.AddListener(onClickBlue);//Setting what button does when clicked
        blueButton.transform.GetChild(0).GetComponent<Text>().text = "Blue Team";//Changing text
    }

    private void SendMyMessage(string textInput)
    {
        byte error;
        byte[] buffer = new byte[1024];
        int bufferSize = 1024;
        Stream message = new MemoryStream(buffer);
        BinaryFormatter formatter = new BinaryFormatter();
        //Serialize the message
        formatter.Serialize(message, textInput);

        //Send the message from the "client" with the serialized message and the connection information
        NetworkTransport.Send(hostId, connectionId, reliableChannel, buffer, (int)message.Position, out error);

        //If there is an error, output message error to the console
        if ((NetworkError)error != NetworkError.Ok)
        {
            Debug.Log("Message send error: " + (NetworkError)error);
        }
    }

    private void Update ()
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

        switch (recData)
        {
            case NetworkEventType.Nothing: break;
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
