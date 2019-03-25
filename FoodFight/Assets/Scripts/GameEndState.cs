using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndState {

	public enum EndState {RED_WIN, BLUE_WIN, DRAW};

	private EndState winningTeam;
	private int redScore, blueScore;

	public GameEndState(EndState winningTeam, int redScore, int blueScore) {
		this.winningTeam = winningTeam;
		this.redScore = redScore;
		this.blueScore = blueScore;
	}

	public GameEndState(string winningTeam, int redScore, int blueScore) {
		if (winningTeam.Equals("red")) {
			this.winningTeam = EndState.RED_WIN;
		} else if (winningTeam.Equals("blue")) {
			this.winningTeam = EndState.BLUE_WIN;
		} else {
			this.winningTeam = EndState.DRAW;
		}
		this.redScore = redScore;
		this.blueScore = blueScore;
	}

	public GameEndState() {
		this.winningTeam = EndState.DRAW;
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

	public string winningTeamStr() {
		if (winningTeam == EndState.RED_WIN) return "red";
		if (winningTeam == EndState.BLUE_WIN) return "blue";
		return "draw";
	}
}
