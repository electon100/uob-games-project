using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Manager : MonoBehaviour {

    public GameTimer timer;

    // Scoring
    public Text redScoreText;
    public Text blueScoreText;
    Score blueScore, redScore;
    public float finalBlueScore = 0;
    public float finalRedScore = 0;

    public static GameEndState gameEndState;

    public bool gameOver = false;

    public Server server;
    public NetManager netManager;

    public void Start() {
      blueScore = new Score();
      redScore = new Score();

      server = GameObject.Find("Server").GetComponent<Server>();
      netManager = GameObject.Find("NetManager").GetComponent<NetManager>();
      timer = GameObject.Find("GameTimer").GetComponent<GameTimer>();
    }

  	// Update is called once per frame
  	public void Update() {
        if (!gameOver) {
          updateScores();

          float rScore = redScore.getScore();
          float bScore = blueScore.getScore();
          if (timer.getTime() <= 0) GameOver();

          // Check if either team has reached a score of 0 and if they have, end the game
          if (rScore <= 0) GameOver();
          else if (bScore <= 0) GameOver();

          if (Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.E)) GameOver();
        }
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
    private void GameOver() {
        finalBlueScore = blueScore.getScore();
        finalRedScore = redScore.getScore();

        GameEndState.EndState winningTeam;

        if (finalRedScore > finalBlueScore) {
          winningTeam = GameEndState.EndState.RED_WIN;
        } else if (finalBlueScore > finalRedScore) {
          winningTeam = GameEndState.EndState.BLUE_WIN;
        } else {
          winningTeam = GameEndState.EndState.DRAW;
        }

        gameEndState = new GameEndState(winningTeam, (int) finalRedScore, (int) finalBlueScore);

        gameOver = true;

        SceneManager.LoadScene("GameOverScreen");
    }

    public string getEndGameString() {
      return gameEndState.winningTeamStr() + "$" + gameEndState.getRedScore() + "$" + gameEndState.getBlueScore();
    }
}
