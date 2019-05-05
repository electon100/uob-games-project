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

  public float NextOrderTimer { get; set; }

	public Team(string name, Color colour) {
		Players = new List<ConnectedPlayer>();
    Name = name;
    Colour = colour;
    Score = 0;
    Kitchen = new Kitchen();
    Orders = new List<Order>();
    NextOrderTimer = 0;
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

  public bool addOrder(Transform mainGameCanvas) {
    string recipeName = FoodData.Instance.getRandomRecipeName();
    Ingredient recipe = new Ingredient(recipeName, recipeName + "Prefab");
    string id = recipeName + Orders.Count + Name + "Object";

    Orders.Add(new Order(id, recipe, new GameObject(id), 150, mainGameCanvas, Name));

    return true;
  }

  public void updateOrders() {
    Debug.Log(new Vector2(Screen.width, Screen.height));
    for (int i = 0; i < Orders.Count; i++) {
      Orders[i].updateCanvas(i, Screen.width, Screen.height);
    }
  }

  public int checkExpiredOrders() {
    int negativeScore = 0;
    for (int i = 0; i < Orders.Count; i++) {
      if (Orders[i].timerExpired()) {
        negativeScore += FoodData.Instance.getScoreForIngredient(Orders[i].Recipe);
        removeOrder(Orders[i]);
        NextOrderTimer *= 0.6f;
      }
    }
    return negativeScore;
  }

  private void removeOrder(Order order) {
    UnityEngine.Object.Destroy(order.ParentGameObject);
    Orders.Remove(order);
  }

  public void removeAllOrders() {
    for (int i = 0; i < Orders.Count; i++) {
      removeOrder(Orders[i]);
    }
  }

  public void scoreRecipe(Ingredient ingredient) {
    for (int i = 0; i < Orders.Count; i++) {
      if (ingredient.Name.Equals(Orders[i].Recipe.Name)) {
        Score += FoodData.Instance.getScoreForIngredient(ingredient);
        UnityEngine.Object.Destroy(Orders[i].ParentGameObject);
        Orders.Remove(Orders[i]);
        break;
      }
    }
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
