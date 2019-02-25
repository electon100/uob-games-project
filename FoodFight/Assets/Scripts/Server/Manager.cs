using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Manager : MonoBehaviour {

    GameTimer timer;

    // Scoring
    public Text redScoreText;
    public Text blueScoreText;
    Score blueScore, redScore;
    public float finalBlueScore = 0;
    public float finalRedScore = 0;

	// Use this for initialization
	public Manager() {
        redScoreText = GameObject.Find("RedScore").GetComponent<Text>();
        blueScoreText = GameObject.Find("BlueScore").GetComponent<Text>();
        blueScore = new Score();
        redScore = new Score();
        timer = new GameTimer();
	}
	
	// Update is called once per frame
	public void update() {
        timer.updateTimer();
        updateScores();

        float rScore = redScore.getScore();
        float bScore = blueScore.getScore();
        if (timer.getTime() <= 0)
        {
            if (rScore > bScore) GameOver("red");
            else if (bScore > rScore) GameOver("blue");
            // Defaults to red winning if it is a tie
            else GameOver("red");
        }

        // Check if either team has reached a score of 0 and if they have, end the game
        if (rScore == 0) GameOver("blue");
        else if (bScore == 0) GameOver("red");
    }

    private void updateScores()
    {
        redScoreText.text = "Red Score " + redScore.getScore().ToString();
        blueScoreText.text = "Blue Score " + blueScore.getScore().ToString();
    }

    public void increaseRed(int recipeScore)
    {
        redScore.increaseScore(recipeScore);
    }

    public void increaseBlue(int recipeScore)
    {
        blueScore.increaseScore(recipeScore);
    }

    // Ends the game by loading the Game Over screen
    private void GameOver(string winningTeam)
    {
        finalBlueScore = blueScore.getScore();
        finalRedScore = redScore.getScore();

        if (winningTeam.Equals("blue"))
        {
            SceneManager.LoadScene("GameOverScreen");
        }
        else if (winningTeam.Equals("red"))
        {
            SceneManager.LoadScene("GameOverScreen");
        }
    }
}
