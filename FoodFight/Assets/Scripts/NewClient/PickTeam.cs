using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PickTeam : MonoBehaviour {
	public GameObject networkClient;
	private Client network;
	public Text gameNotRunningText;

	void Start () {
		Screen.orientation = ScreenOrientation.Portrait;
		networkClient = GameObject.Find("Client");
    network = networkClient.GetComponent<Client>();
		DontDestroyOnLoad(networkClient);
	}
	
	public void onClickRed() {
		network.onClickRed();
	}

	public void onClickBlue() {
		network.onClickBlue();
	}

	public void displayNotRunningText() {
		gameNotRunningText.gameObject.SetActive(true);
		gameNotRunningText.text = "Game is not running.\n Please try again.";
	}
	
	void Update () {
		
	}

	public void OnGoHome() {
		Client.gameState = ClientGameState.JoinState;
		SceneManager.LoadScene("PlayerStartScreen");
	}
}
