using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text;

public class PlateBehaviour : MonoBehaviour {

    private readonly string stationID = "3";

    public Player player;

    /* Text representation of ingredients on Screen */
    public Text ingredientListText;

    /* Ingredient stuff */
    private List<Ingredient> plateContents = new List<Ingredient>();
  	private List<GameObject> plateContentsObjects = new List<GameObject>();

    void Start () {

      Screen.orientation = ScreenOrientation.Portrait;

      clearPlate();

      player = GameObject.Find("Player").GetComponent<Player>();

      foreach (Ingredient ingredient in Player.ingredientsFromStation) {
        addIngredientToPlate(ingredient);
      }

	  }

    void Update() {
      /* Try and combine the ingredients */
      Ingredient combinationAttempt = getWorkingRecipe();

      /* If the combined result is a valid recipe (not mush) */
      if (isValidRecipe(combinationAttempt)) {
        /* Set the pan contents to the new combined recipe */
				clearStation();
				addIngredientToPlate(combinationAttempt);
				player.notifyServerAboutIngredientPlaced(combinationAttempt);
      }

      updateTextList();
    }

    private void addIngredientToPlate(Ingredient ingredient) {
      GameObject ingred = (GameObject) Resources.Load(ingredient.Model, typeof(GameObject));
      Transform ingredTransform = ingred.GetComponentsInChildren<Transform>(true)[0];
      Quaternion ingredRotation = ingredTransform.rotation;
      Vector3 ingredPosition = ingredTransform.position + new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), 0);
      GameObject inst = Instantiate(ingred, ingredPosition, ingredRotation);

      plateContents.Add(ingredient);
      plateContentsObjects.Add(inst);
	  }

    private void clearStation() {
      clearPlate();
      player.clearIngredientsInStation(stationID);
    }

    private void clearPlate() {
      foreach (GameObject go in plateContentsObjects) Destroy(go);

      plateContents.Clear();
      plateContentsObjects.Clear();
    }

    public void placeHeldIngredientInPlate() {
      /* Add ingredient */
      if (Player.currentIngred != null) {
        addIngredientToPlate(Player.currentIngred);

        /* Notify server that player has placed ingredient */
        player.notifyServerAboutIngredientPlaced(Player.currentIngred);

        Player.removeCurrentIngredient();
      } else {
        /* TODO: What happens when player is not holding an ingredient */
      }
    }

    void updateTextList() {
      ingredientListText.text = "Current Ingredients:\n";

      foreach(Ingredient ingredient in plateContents) {
        ingredientListText.text += ingredient.ToString() + "\n";
      }
    }

    private Ingredient getWorkingRecipe() {
      return FoodData.Instance.TryCombineIngredients(plateContents);
    }

    private bool isValidRecipe(Ingredient recipe) {
      return !string.Equals(recipe.Name, "mush");
    }

    public void serveFood() {
      Ingredient workingRecipe = getWorkingRecipe();
      if (isValidRecipe(workingRecipe)) {
        player.sendScoreToServer(workingRecipe);
        clearPlate();
      }
    }

    public void goBack() {
      /* Notify server that player has left the station */
      player.notifyAboutStationLeft(stationID);
      SceneManager.LoadScene("PlayerMainScreen");
    }
}
