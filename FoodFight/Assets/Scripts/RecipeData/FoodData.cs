using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed class FoodData {

	private static FoodData instance = null;
	private static readonly object padlock = new object();

	private static RecipeDefinitions allRecipes;
	private static IngredientDefinitions allIngredients;

	public RecipeDefinitions GetAllRecipes {
		get {
			return allRecipes;
		}
	}

	public IngredientDefinitions GetAllIngredients {
		get {
			return allIngredients;
		}
	}

	FoodData() {
		/* Read recipe data from JSON file */
		string recipeFilePath = Application.dataPath + "/Data/recipe.json";
		string recipeJSON = File.ReadAllText(recipeFilePath);

		/* Read ingredient data from JSON file */
		string ingredientFilePath = Application.dataPath + "/Data/ingredients.json";
		string ingredientJSON = File.ReadAllText(ingredientFilePath);

		/* Parse recipe JSON data */
		allRecipes = JsonUtility.FromJson<RecipeDefinitions>(recipeJSON);

		/* Parse ingredient JSON data */
		allIngredients = JsonUtility.FromJson<IngredientDefinitions>(ingredientJSON);
	}

	public static FoodData Instance {
    get {
			lock (padlock) {
				if (instance == null) instance = new FoodData();
				return instance;
			}
		}
	}
}
