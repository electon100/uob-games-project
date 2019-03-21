using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class Team {

  public List<ConnectedPlayer> Players { get; }

	public string Name { get; set; }

  public Color Colour { get; set; }

	public int Score { get; set; }

  public Kitchen Kitchen { get; }

  public List<Order> Orders { get; set; }

	public Team(string name, Color colour) {
		Players = new List<ConnectedPlayer>();
    Name = name;
    Colour = colour;
    Score = 0;
    Kitchen = new Kitchen();
    Orders = new List<Order>();
	}

  public bool addPlayerToTeam(ConnectedPlayer player) {
    if (!isPlayerOnTeam(player.ConnectionId))  {
      Players.Add(player);
      return true;
    }
    return false;
  }

  public void removePlayer(ConnectedPlayer player) {
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

  public bool isStationOccupied(string stationId) {
		foreach(ConnectedPlayer player in Players) {
			if (player.CurrentStation != null && player.CurrentStation.Id.Equals(stationId)) return true;
		}
    return false;
  }

  public bool isStationOccupied(Station station) {
    return isStationOccupied(station.Id);
  }

  public bool addOrder() {
    string recipeName = FoodData.Instance.getRandomRecipeName();
    Ingredient recipe = new Ingredient(recipeName, recipeName + "Prefab");
    Orders.add(recipe, new GameObject(recipeName + Orders.Count() + "Object"), 180, )
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
