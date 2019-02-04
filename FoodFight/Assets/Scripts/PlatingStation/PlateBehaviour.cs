using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
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
    // Cameras
    Camera camera;
    //Final model to display on Plate
    GameObject model;

    // Use this for initialization
    void Start () {
        DontDestroyOnLoad(GameObject.Find("Player"));
        camera = GameObject.Find("Camera1").GetComponent<Camera>();
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
        GameObject food = (GameObject) Resources.Load(recipe.Name + "PlatePrefab", typeof(GameObject));
        if (food == null) {
          food = (GameObject) Resources.Load("mushPlatePrefab", typeof(GameObject));
        }
        model  = Instantiate(food, new Vector3(0,0,0), Quaternion.identity);
      } else {
        model = null;
      }
      updateTextList();
    }

    public void serveFood() {
      //TODO serve food
      if (!string.Equals(recipe.Name, "mush")) {
        ingredientList.Clear();
        Destroy(model, 0.0f);
        displayFood();
      }
    }

    public void addIngredient() {
      Ingredient temp = new Ingredient("diced_potato", "");
      temp.numberOfPanFlips = 30;
      Player.currentIngred = temp;
      if (Player.currentIngred != null) {
        ingredientList.Add(Player.currentIngred);
        player.removeCurrentIngredient();
        player.notifyServerAboutIngredientPlaced();
        ingredientList = Player.ingredientsFromStation;
        displayFood();
      }
    }
}
