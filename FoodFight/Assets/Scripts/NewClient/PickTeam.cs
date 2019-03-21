using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickTeam : MonoBehaviour {
	public GameObject networkClient;
	private Client network;
	public Text gameNotRunningText;

	void Start () {
		Screen.orientation = ScreenOrientation.Portrait;
		networkClient = GameObject.Find("Client");
    network = networkClient.GetComponent<Client>();
	}
	
	public void onClickRed() {
		network.onClickRed();
	}

	public void onClickBlue() {
		network.onClickBlue();
	}

	public void displayNotRunningText() {
		gameNotRunningText.gameObject.SetActive(true);
		gameNotRunningText.text = "Game is not running. Please try again.";
	}
	
	void Update () {
		
	}
}
