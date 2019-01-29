using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JsonExample : MonoBehaviour {

	// Use this for initialization
	void Start () {
		RecipeDefinitions allRecipes = FoodData.Instance.GetAllRecipes;
		IngredientDefinitions allIngredients = FoodData.Instance.GetAllIngredients;

		Debug.Log("All Recipes:");

		/* Iterate through all recipes, printing their name */
		for (int i = 0; i < allRecipes.recipes.Length; i++) {
			Debug.Log(allRecipes.recipes[i].name);

			/* Iterate through all ingredients of current recipe, printing name */
			for (int j = 0; j < allRecipes.recipes[i].ingredients.Length; j++) {
				Debug.Log(allRecipes.recipes[i].ingredients[j].name);
			}
		}

		Debug.Log("All Ingredients:");

		for (int i = 0; i < allIngredients.ingredients.Length; i++) {
			Debug.Log(allIngredients.ingredients[i].name);
		}
	}

	// Update is called once per frame
	void Update () {}
}
