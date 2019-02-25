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


      player = GameObject.Find("Player").GetComponent<Player>();

      clearPlate();

      foreach (Ingredient ingredient in Player.ingredientsFromStation) {
        addIngredientToPan(ingredient);
      }

      // displayFood();
	  }

    void Update() {
      // ingredientList = Player.ingredientsFromStation;
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

        player.removeCurrentIngredient();

        /* Try combine! */
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

    void checkRecipe() {
      recipe = FoodData.Instance.TryCombineIngredients(ingredientList);
      Debug.Log(recipe.Name);
    }

    public void serveFood() {
      if (!string.Equals(recipe.Name, "mush")) {
        player.sendScoreToServer(recipe);
        clearPlate();
      }
    }

    public void goBack() {
      /* Notify server that player has left the station */
      player.notifyAboutStationLeft(stationID);
      SceneManager.LoadScene("PlayerMainScreen");
    }
}
