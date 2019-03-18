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
		Screen.orientation = ScreenOrientation.Portrait;
	}

	public void ConnectAgain() {
		network.Connect();
		connected = network.isConnected;
	}

	public void ExitGame() {
		Application.Quit();
	}

	void Update () {
		if (connected) {
			network.SendMyMessage("connect", network.team);
			SceneManager.LoadScene("LobbyScreen");
		}
	}
}
