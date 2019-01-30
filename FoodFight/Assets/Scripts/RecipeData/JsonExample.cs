using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JsonExample : MonoBehaviour {

	// Use this for initialization
	void Start () {

		/* Create ingredients list */
		List<Ingredient> ingredients = new List<Ingredient>();

		/* Create ingredients */
		Ingredient noodles = new Ingredient("noodles", "noodlesPrefab");
		Ingredient veg = new Ingredient("chopped_mixed_vegetables", "chopped_mixed_vegetablesPrefab");
		Ingredient chicken = new Ingredient("diced_chicken", "diced_chickenPrefab");

		/* Add ingredients to list */
		ingredients.Add(noodles);
		ingredients.Add(veg);
		ingredients.Add(chicken);

		/* Try and combine ingredients. Hopefully make stir_fry_mix */
		Ingredient recipe = FoodData.Instance.TryCombineIngredients(ingredients);

		Debug.Log(recipe.Name);
	}

	void logAll() {
		RecipeDefinitions allRecipes = FoodData.Instance.GetAllRecipes;
		IngredientDefinitions allIngredients = FoodData.Instance.GetAllIngredients;

		Debug.Log("All Recipes:");

		/* Iterate through all recipes, printing their name */
		for (int i = 0; i < allRecipes.recipes.Length; i++) {
			Debug.Log(allRecipes.recipes[i].name + " is made with:");
			Debug.Log("-----");
			/* Iterate through all ingredients of current recipe, printing name */
			for (int j = 0; j < allRecipes.recipes[i].ingredients.Length; j++) {
				Debug.Log(allRecipes.recipes[i].ingredients[j].name);
			}
			Debug.Log("-----");
		}

		Debug.Log("All Ingredients:");

		for (int i = 0; i < allIngredients.ingredients.Length; i++) {
			Debug.Log(allIngredients.ingredients[i].name);
		}
	}

	// Update is called once per frame
	void Update () {}
}
