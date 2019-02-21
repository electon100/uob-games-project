using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndState {

	private string winningTeam;
	private int redScore;
	private int blueScore;

	public GameEndState(string winningTeam, int redScore, int blueScore) {
		this.winningTeam = winningTeam;
		this.redScore = redScore;
		this.blueScore = blueScore;
	}

	public GameEndState() {
		this.winningTeam = "red";
		this.redScore = 0;
		this.blueScore = 0;
	}

	public string getWinningTeam() {
		return this.winningTeam;
	}

	public int getRedScore() {
		return this.redScore;
	}

	public int getBlueScore() {
		return this.blueScore;
	}
}
