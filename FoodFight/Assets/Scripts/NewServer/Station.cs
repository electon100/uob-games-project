using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class Station {

  public string Id { get; set; }

	public float DisabledTimer { get; set; }

  public List<Ingredient> Ingredients { get; }

  public GameObject VisualDisable { get; set; }

	public Station(string id) {
		Id = id;
		Ingredients = new List<Ingredient>();
		resetTimer();
	}

	public void resetTimer() {
		DisabledTimer = 0.0f;
	}

	public bool isDisabled() {
		return DisabledTimer > 0;
	}

	public void addIngredientToStation(Ingredient ingredient) {
		Ingredients.Add(ingredient);
	}

	public void clearIngredientsInStation() {
		Ingredients.Clear();
	}

	public override string ToString() {
		string toReturn = "Station [id=" + Id + ", ingredients=";
		foreach(Ingredient ingredient in Ingredients) {
			toReturn += ", " + ingredient.ToString();
		}
		toReturn += "]";
    return toReturn;
  }

}
