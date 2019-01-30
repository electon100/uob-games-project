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
    //Prefabs
    public GameObject mush;
    public GameObject goodFood;
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

    bool checkRecipe() {
      recipe = FoodData.Instance.TryCombineIngredients(ingredientList);
      Debug.Log(recipe.Name);
      return !(string.Equals(recipe.Name, "mush"));
    }

    void displayFood() {
       bool validRecipe = checkRecipe();
       if (ingredientList.Count > 0 && !validRecipe) {
         model  = Instantiate(mush, new Vector3(0,0,0), Quaternion.identity);
       } else if (ingredientList.Count > 0 && validRecipe) {
         model  = Instantiate(goodFood, new Vector3(0,0,0), Quaternion.identity);
       }
       updateTextList();
    }

    public void serveFood() {
      //TODO serve food
    }

    public void addIngredient() {
      //Ingredient temp = new Ingredient("diced_potato", "");
      //temp.numberOfPanFlips = 35;
      //Player.currentIngred = temp;
      ingredientList.Add(Player.currentIngred);
      player.notifyServerAboutIngredientPlaced();
      ingredientList = Player.ingredientsFromStation;
      Debug.Log(ingredientList.Count);
      displayFood();
    }
}
