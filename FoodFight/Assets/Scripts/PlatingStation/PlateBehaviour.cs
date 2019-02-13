using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;

public class PlateBehaviour : MonoBehaviour {

    public Player player;
    // All ingredients on the plate
    public List<Ingredient> ingredientList = new List<Ingredient>();
    // Text representation of ingredients on Screen
    public Text ingredientListText;
    // Holds the name of the recipe
    Ingredient recipe = null;

    //Final model to display on Plate
    GameObject model;

    // Use this for initialization
    void Start () {
        DontDestroyOnLoad(GameObject.Find("Player"));
        ingredientListText = GameObject.Find("Ingredient List").GetComponent<Text>();
        player = GameObject.Find("Player").GetComponent<Player>();
        ingredientList = Player.ingredientsFromStation;
        displayFood();
	  }

    void Update() {
      ingredientList = Player.ingredientsFromStation;
    }

    void updateTextList() {
      ingredientListText.text = "Current Ingredients:\n";

      for(int i = 0; i < ingredientList.Count; i++) {
        ingredientListText.text += ingredientList[i].Name + "\n";
      }
    }

    void checkRecipe() {
      recipe = FoodData.Instance.TryCombineIngredients(ingredientList);
      Debug.Log(recipe.Name);
    }

    void displayFood() {
      checkRecipe();
      if (ingredientList.Count > 0) {
        GameObject food = (GameObject) Resources.Load(recipe.Model, typeof(GameObject));
        if (food == null) {
          food = (GameObject) Resources.Load("mushPlatePrefab", typeof(GameObject));
        }
        model  = (GameObject) Instantiate(food, new Vector3(0,0,0), Quaternion.Euler(90, 0, 10));
      } else {
        model = null;
      }
      updateTextList();
    }

    public void serveFood() {
      if (!string.Equals(recipe.Name, "mush")) {
        ingredientList.Clear();
        Destroy(model, 0.0f);
        //int score = FoodData.Instance.getScoreForIngredient(recipe);
        player.sendScoreToServer(recipe);
        player.clearIngredientsInStation("3");
        recipe = null;
        displayFood();
      }
    }

    public void addIngredient() {
      if (Player.currentIngred != null) {
        ingredientList.Add(Player.currentIngred);
        player.notifyServerAboutIngredientPlaced(Player.currentIngred);
        player.removeCurrentIngredient();
        ingredientList = Player.ingredientsFromStation;
        displayFood();
      }
    }

    public void goBack() {
      SceneManager.LoadScene("PlayerMainScreen");
    }
}
