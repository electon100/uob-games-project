using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGameOver : MonoBehaviour {

	public Text MainText;
	public Text RedScoreText;
	public Text BlueScoreText;
	public Material redBackground;
	public Material blueBackground;
	public Renderer background;

	private Client client;

	private GameEndState gameEndState;

	void Start () {
		Screen.orientation = ScreenOrientation.Portrait;

		DontDestroyOnLoad(GameObject.Find("Client"));
		client = GameObject.Find("Client").GetComponent<Client>();

		gameEndState = client.gameEndState;

		MainText.text = gameEndState.winningTeamStr() + " Team Wins!";

		RedScoreText.text = "Red Score: " + gameEndState.getRedScore().ToString();
		BlueScoreText.text = "Blue Score: " + gameEndState.getBlueScore().ToString();

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
	}

	void Loser() {
		MainText.text = "You lose!";
	}

	void Draw() {
		MainText.text = "Draw!";
	}

	void UpdateBackground() {
		if (client.getTeam().Equals("blue")) {
			background.material = blueBackground;
		} else if (client.getTeam().Equals("red")) {
			background.material = redBackground;
		}
	}
}
