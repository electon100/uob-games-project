using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScript : MonoBehaviour {
    float redScore, blueScore;
    Text redScoreText, blueScoreText, winnerText;

	// Use this for initialization
	void Start () {

        Image img = GameObject.Find("Panel").GetComponent<Image>();

        redScoreText = GameObject.Find("RedScore").GetComponent<Text>();
        blueScoreText = GameObject.Find("BlueScore").GetComponent<Text>();
        winnerText = GameObject.Find("WinnerText").GetComponent<Text>();

        redScore = Server.finalRedScore;
        blueScore = Server.finalBlueScore;

        if (blueScore > redScore)
        {
            img.color = UnityEngine.Color.blue;
            winnerText.text = "Blue Team Wins";
        }
        else
        {
            img.color = UnityEngine.Color.red;
            winnerText.text = "Red Team Wins";
        }

        redScoreText.text = redScore.ToString();
        blueScoreText.text = blueScore.ToString();
	}
}
