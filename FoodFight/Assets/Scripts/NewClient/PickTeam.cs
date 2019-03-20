using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickTeam : MonoBehaviour {
	public GameObject networkClient;
	private Client network;

	// Use this for initialization
	void Start () {
		networkClient = GameObject.Find("Client");
        network = networkClient.GetComponent<Client>();
	}
	
	public void onClickRed() {
		network.onClickRed();
	}

	public void onClickBlue() {
		network.onClickBlue();
	}

	// Update is called once per frame
	void Update () {
		
	}
}
