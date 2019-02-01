using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScript : MonoBehaviour {
    float redScore, blueScore;
    Text redScoreText, blueScoreText;

	// Use this for initialization
	void Start () {
        redScoreText = GameObject.Find("RedScore").GetComponent<Text>();
        blueScoreText = GameObject.Find("BlueScore").GetComponent<Text>();

        redScore = Server.finalRedScore;
        blueScore = Server.finalBlueScore;

        redScoreText.text = redScore.ToString();
        blueScoreText.text = blueScore.ToString();
	}
}
