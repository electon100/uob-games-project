[System.Serializable]
public class RecipeDefinitions {
	public RecipeDefinition[] recipes;
}

[System.Serializable]
public class IngredientCriteria {
	public string name;
	public bool cooked, chopped;
}

[System.Serializable]
public class RecipeDefinition {
	public string name;
	public IngredientCriteria[] ingredients;
}