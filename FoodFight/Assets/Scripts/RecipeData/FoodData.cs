
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed class FoodData {

	private readonly string ingredientsFileName = "test_ingredients"; /* The name of the JSON file containing ingredients */
	private readonly string recipesFileName = "test_recipes"; /* The name of the JSON file containing recipes */

	private static FoodData instance = null;
	private static readonly object padlock = new object();

	private static RecipeDefinitions allRecipes;
	private static IngredientDefinitions allIngredients;

	public string mode = " ";

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

	/* Determines whether the given ingredient is chopped */
	public bool isChopped(Ingredient ingredient) {
		if (ingredient == null) return false;
		IngredientDescription desc = GetIngredientDescription(ingredient);
		return desc == null ? false : desc.choppable && (ingredient.numberOfChops >= desc.correctChops);
	}

	/* Determines whether the given ingredient is cooked */
	public bool isCooked(Ingredient ingredient) {
		if (ingredient == null) return false;
		IngredientDescription desc = GetIngredientDescription(ingredient);
		return desc == null ? false : desc.cookable && (ingredient.numberOfPanFlips >= desc.correctFlips);
	}

	/* Determines whether the given ingredient is choppable */
	public bool isChoppable(Ingredient ingredient) {
		if (ingredient == null) return false;
		IngredientDescription desc = GetIngredientDescription(ingredient);
		return desc != null && desc.choppable;
	}

	/* Determines whether the given ingredient is cookable */
	public bool isCookable(Ingredient ingredient) {
		if (ingredient == null) return false;
		IngredientDescription desc = GetIngredientDescription(ingredient);
		return desc != null && desc.cookable;
	}

	/* Gets a random recipe name*/
	public string getRandomRecipeName() {
		int numIngredients = allIngredients.ingredients.Length;
		bool rightMode = false;
		IngredientDescription recipe = allIngredients.ingredients[Random.Range(0, numIngredients - 1)];

		if (mode == recipe.mode) rightMode = true;

		while (!rightMode) {
			recipe = allIngredients.ingredients[Random.Range(0, numIngredients - 1)];
			if (mode == recipe.mode) rightMode = true;
		}

		return recipe.name;
	}

	/* Gets the score corresponding to an ingredient */
	public int getScoreForIngredient(Ingredient ingredient) {
		IngredientDescription desc = GetIngredientDescription(ingredient);
		return desc != null ? desc.score : 0;
	}

	/* Determines whether the input ingredient matches the provided criteria */
	public bool MatchesCriteria(Ingredient ingredient, IngredientCriteria criteria) {
		/* Grab corresponding ingredient description (if available) */
		IngredientDescription desc = GetIngredientDescription(ingredient);
		bool cooked = false, chopped = false;

		if (desc != null) {
			/* Determine ingredient status based on ingredient description */
			chopped = desc.choppable && (ingredient.numberOfChops >= desc.correctChops);
			cooked = desc.cookable && (ingredient.numberOfPanFlips >= desc.correctFlips);

			bool nameMatches = string.Equals(desc.name, criteria.name);

			/* Check ingredient status against criteria */
			if (nameMatches && criteria.cooked == cooked && criteria.chopped == chopped) return true;
		}

		return false;
	}

	/* Returns the description corresponding to an ingredient, if possible */
	public IngredientDescription GetIngredientDescription(Ingredient ingredient) {
		if (ingredient != null) {
			/* Iterate through all ingredient descriptions, finding and returning one with a matchign name */
			for (int i = 0; i < allIngredients.ingredients.Length; i++) {
				IngredientDescription testIngredient = allIngredients.ingredients[i];
				if (string.Equals(ingredient.Name, testIngredient.name)) return testIngredient;
			}
		}
			return null;
	}

	public Ingredient TryAdvanceIngredient(Ingredient ingredient) {
		return TryCombineIngredients(new List<Ingredient>{ingredient});
	}

	public Ingredient TryCombineIngredients(List<Ingredient> ingredients) {

		/* Iterate through all possible recipes */
		for (int r = 0; r < allRecipes.recipes.Length; r++) {
			RecipeDescription recipe = allRecipes.recipes[r];
			bool allIngredientsMatch = false;

			/* Check whether ingredients and recipe criteria list match in length */
			if (recipe.ingredients.Length == ingredients.Count) {

				/* Assume all ingredients match to begin */
				allIngredientsMatch = true;

				/* Iterate through all recipe criteria, assuming the criteria is not met */
				for (int j = 0; j < recipe.ingredients.Length; j++) {
					bool criteriaMatched = false;

					/* Iterate through all input ingredients, testing whether any of them match the criteria */
					for (int k = 0; k < ingredients.Count; k++) {
						if (MatchesCriteria(ingredients[k], recipe.ingredients[j])) criteriaMatched = true;
					}

					/* If no ingredients match the criteria, the recipe is not met */
					if (!criteriaMatched) allIngredientsMatch = false;
				}
			}

			/* If a recipe is met, return the combined ingredient */
			if (allIngredientsMatch) return new Ingredient(recipe.name, recipe.name + "Prefab");
		}

		/* Return mush if no matching recipe is found */
		return new Ingredient("mush", "mushPrefab");
	}

	FoodData() {
		/* Read recipe data from JSON file */
		TextAsset recipeFile = (TextAsset) Resources.Load(recipesFileName, typeof(TextAsset));
		string recipeJSON = recipeFile.ToString();

		/* Read ingredient data from JSON file */
		TextAsset ingredientFile = (TextAsset) Resources.Load(ingredientsFileName, typeof(TextAsset));
		string ingredientJSON = ingredientFile.ToString();

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
