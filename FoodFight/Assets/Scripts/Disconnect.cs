using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Disconnect : MonoBehaviour {

	public GameObject networkClient;
	private Client network;

	void Start () {
		networkClient = GameObject.Find("Client");
        network = networkClient.GetComponent<Client>();
	}
	
	public void ConnectAgain() {
		Debug.Log("here");
		network.Connect();
	}

	public void ExitGame() {
		Application.Quit();
	}

	void Update () {
		if (network.isConnected) {
			SceneManager.LoadScene("PlayerMainScreen");
		}
	}
}
