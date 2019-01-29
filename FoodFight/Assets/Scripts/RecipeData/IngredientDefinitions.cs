[System.Serializable]
public class IngredientDefinitions {
	public ingredientDescription[] ingredients;
}

[System.Serializable]
public class ingredientDescription {
	public string name;
	public bool choppable, cookable;
	public int correctFlips, maxFlips, correctChops;
}