using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Disconnect : MonoBehaviour {

	public GameObject networkClient;
	private Client network;
	private bool connected;

	void Start () {
		networkClient = GameObject.Find("Client");
        network = networkClient.GetComponent<Client>();
	}
	
	public void ConnectAgain() {
		int hostId = network.hostId;
		string serverIP = Client.serverIP;
		int port = network.port;
		byte error;
		int connectionId = NetworkTransport.Connect(hostId, serverIP, port, 0, out error);
		network.connectionId = connectionId;
		network.isConnected = true;
		connected = true;
	}

	public void ExitGame() {
		Application.Quit();
	}

	void Update () {
		if (connected) {
			SceneManager.LoadScene("PickTeamScreen");
		}
	}
}
