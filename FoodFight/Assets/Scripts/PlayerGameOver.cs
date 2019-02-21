using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGameOver : MonoBehaviour {

	public Text MainText;
	public Text RedScoreText;
	public Text BlueScoreText;
	private GameObject Background;

	private Player player;

	private GameEndState gameEndState;

	// Use this for initialization
	void Start () {
		Background = GameObject.Find("Background").GetComponent<GameObject>();

		DontDestroyOnLoad(GameObject.Find("Player"));
		player = GameObject.Find("Player").GetComponent<Player>();

		gameEndState = Player.getGameEndState();

		MainText.text = gameEndState.getWinningTeam() + " Team Wins!";

		RedScoreText.text = "Red Score: " + gameEndState.getRedScore().ToString();
		BlueScoreText.text = "Blue Score: " + gameEndState.getBlueScore().ToString();
	}

	// Update is called once per frame
	void Update () {

	}
}
