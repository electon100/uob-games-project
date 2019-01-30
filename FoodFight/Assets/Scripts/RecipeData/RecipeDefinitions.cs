[System.Serializable]
public class RecipeDefinitions {
	public RecipeDescription[] recipes;
}

[System.Serializable]
public class RecipeDescription {
	public string name;
	public IngredientCriteria[] ingredients;
}

[System.Serializable]
public class IngredientCriteria {
	public string name;
	public bool cooked, chopped;
}