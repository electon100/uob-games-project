[System.Serializable]
public class IngredientDefinitions {
	public IngredientDescription[] ingredients;
}

[System.Serializable]
public class IngredientDescription {
	public string name;
	public bool choppable, cookable, orderable;
	public int correctFlips, maxFlips, correctChops, score;
}
