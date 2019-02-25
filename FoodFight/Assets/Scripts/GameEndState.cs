using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndState {

	public enum EndState {RED_WIN, BLUE_WIN, DRAW};

	private EndState winningTeam;
	private int redScore;
	private int blueScore;

	public GameEndState(EndState winningTeam, int redScore, int blueScore) {
		this.winningTeam = winningTeam;
		this.redScore = redScore;
		this.blueScore = blueScore;
	}

	public GameEndState(string winningTeam, int redScore, int blueScore) {
		if (winningTeam.Equals("red")) {
			this.winningTeam = RED_WIN;
		} else if (winningTeam.Equals("blue")) {
			this.winningTeam = BLUE_WIN;
		} else {

		}
		this.winningTeam = winningTeam;
		this.redScore = redScore;
		this.blueScore = blueScore;
	}

	public GameEndState() {
		this.winningTeam = DRAW;
		this.redScore = 0;
		this.blueScore = 0;
	}

	public EndState getWinningTeam() {
		return this.winningTeam;
	}

	public int getRedScore() {
		return this.redScore;
	}

	public int getBlueScore() {
		return this.blueScore;
	}
}
