﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScript : MonoBehaviour {
    float redScore, blueScore;
    Text redScoreText, blueScoreText, winnerText;

	// Use this for initialization
	void Start () {

        DontDestroyOnLoad(GameObject.Find("Client"));
        DontDestroyOnLoad(GameObject.Find("Player"));

        GameEndState gameEndState = Client.getGameEndState();

        Image img = GameObject.Find("Panel").GetComponent<Image>();

        redScoreText = GameObject.Find("RedScore").GetComponent<Text>();
        blueScoreText = GameObject.Find("BlueScore").GetComponent<Text>();
        winnerText = GameObject.Find("WinnerText").GetComponent<Text>();

        switch (gameEndState.getWinningTeam()) {
          case EndState.RED_WIN :
            img.color = UnityEngine.Color.red;
            winnerText.text = "Red Team Wins!";
            break;
          case EndState.BLUE_WIN :
            img.color = UnityEngine.Color.blue;
            winnerText.text = "Blue Team Wins!";
            break;
          case EndState.DRAW :
            img.color = UnityEngine.Color.white;
            winnerText.text = "Draw!";
            break;
          default :
            break;
        }

        redScoreText.text = gameEndState.getRedScore().ToString();
        blueScoreText.text = gameEndState.getBlueScore().ToString();
	}
}
