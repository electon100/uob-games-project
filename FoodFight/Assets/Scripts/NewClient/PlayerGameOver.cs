using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGameOver : MonoBehaviour {

	public Text MainText;
	public Text YourScoreText;
	public Text TheirScoreText;
	public Material redBackground;
	public Material blueBackground;
	public Renderer background;
	public GameObject winnerObject, loserObject;

	private Client client;

	private GameEndState gameEndState;

	void Start () {
		Screen.orientation = ScreenOrientation.Portrait;

		DontDestroyOnLoad(GameObject.Find("Client"));
		client = GameObject.Find("Client").GetComponent<Client>();

		gameEndState = client.gameEndState;

		if (client.getTeam().Equals("red")) {
			YourScoreText.text = "Your Score: " + gameEndState.getRedScore().ToString();
			TheirScoreText.text = "Their Score: " + gameEndState.getBlueScore().ToString();
		} else if (client.getTeam().Equals("blue")) {
			YourScoreText.text = "Your Score: " + gameEndState.getBlueScore().ToString();
			TheirScoreText.text = "Their Score: " + gameEndState.getRedScore().ToString();
		}

		UpdateBackground();

		if (client.getTeam().Equals(gameEndState.winningTeamStr())) {
			Winner();
		} else {
			if (gameEndState.getWinningTeam() != GameEndState.EndState.DRAW) {
				Loser();
			} else {
				Draw();
			}
		}
	}

	void Update () {}

	void Winner() {
		MainText.text = "You win!";
		winnerObject.SetActive(true);
		loserObject.SetActive(false);
	}

	void Loser() {
		MainText.text = "You lose!";
		winnerObject.SetActive(false);
		loserObject.SetActive(true);
	}

	void Draw() {
		MainText.text = "Draw!";
		winnerObject.SetActive(false);
		loserObject.SetActive(false);
	}

	void UpdateBackground() {
		if (client.getTeam().Equals("blue")) {
			background.material = blueBackground;
		} else if (client.getTeam().Equals("red")) {
			background.material = redBackground;
		}
	}
}
