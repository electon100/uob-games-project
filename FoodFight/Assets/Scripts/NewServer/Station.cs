using System;
using System.IO;
using System.Collections.Generic;

public class Station {

  public string Id { get; set; }

  public List<Ingredient> Ingredients { get; }

	public Station(string id) {
		Id = id;
		Ingredients = new List<Ingredient>();
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