using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class OrderManager : MonoBehaviour {

	public Button button1, button2, button3, button4, button5, button6;
	public GameObject recipePanel, teamPanel, cuisinePanel, connectPanel;
	private OrderClient network;

	private static readonly string[] frenchOrders = {"crepe", "chips", "omlette", "quiche", "ratatouille", "steak_hache"};
	private static readonly string[] spanishOrders = {"calamari", "churros", "paella", "patatas_bravas", "quesadilla", "spanish_omelette"};

	private List<Button> buttons = new List<Button>();

	// Use this for initialization
	void Start () {
		network = GameObject.Find("Client").GetComponent<OrderClient>();

		buttons.Add(button1);
		buttons.Add(button2);
		buttons.Add(button3);
		buttons.Add(button4);
		buttons.Add(button5);
		buttons.Add(button6);

	}

	// Update is called once per frame
	void Update () {

	}

	public void onRed() {

	}

	public void onBlue() {

	}

	public void onFrench() {
	}

	public void onSpanish() {
	}

	public void onConnect() {
		connectPanel.SetActive(false);
		cuisinePanel.SetActive(true);
	}

}
