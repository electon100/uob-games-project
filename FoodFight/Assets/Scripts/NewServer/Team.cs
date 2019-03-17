using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class Team {

  public List<ConnectedPlayer> Players { get; }

	public string Name { get; set; }

	public int Score { get; set; }

  public Kitchen Kitchen { get; }

	public Team(string name) {
		Players = new List<ConnectedPlayer>();
    Name = name;
    Score = 0;
    Kitchen = new Kitchen();
	}

  public void addPlayerToTeam(ConnectedPlayer player) {
    if (!Players.Contains(player)) Players.Add(player);
  }

  public void removePlayerFromTeam(ConnectedPlayer player) {
    if (Players.Contains(player)) Players.Remove(player);
  }
  
  public bool isPlayerOnTeam(int playerId) {
    return getPlayerForId(playerId) != null;
  }

  public ConnectedPlayer getPlayerForId(int playerId) {
    foreach(ConnectedPlayer player in Players) {
      if (player.ConnectionId == playerId) return player;
    }
    return null;
  }
  public bool isStationOccupied(Station station) {
		foreach(ConnectedPlayer player in Players) {
			if (player.CurrentStation.Id.Equals(station.Id)) return true;
		}
    return false;
  }

  public override string ToString() {
		string toReturn = "Team [name=" + Name + ", score=" + Score + ", Kitchen=" + Kitchen.ToString() + ", ConnectedPlayers=";
		foreach(ConnectedPlayer player in Players) {
			toReturn += ", " + player.ToString();
		}
		toReturn += "]";
    return toReturn;
  }
}