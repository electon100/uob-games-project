[System.Serializable]
public class IngredientDefinitions {
	public IngredientDescription[] ingredients;
}

[System.Serializable]
public class IngredientDescription {
	public string name, mode;
	public bool choppable, cookable;
	public int correctFlips, maxFlips, correctChops, score;
}
