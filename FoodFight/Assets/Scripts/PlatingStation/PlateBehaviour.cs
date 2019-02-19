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
    public List<GameObject> ingredientObjects = new List<GameObject>();
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

      foreach(Ingredient ingredient in ingredientList) {
        ingredientListText.text += ingredient.Name + "\n";
      }
    }

    void checkRecipe() {
      recipe = FoodData.Instance.TryCombineIngredients(ingredientList);
      Debug.Log(recipe.Name);
    }

    void displayFood() {
      checkRecipe();
      Destroy(model, 0.0f);
      if (ingredientList.Count > 0) {
        GameObject food = (GameObject) Resources.Load(recipe.Model, typeof(GameObject));
        Transform modelTransform = food.GetComponentsInChildren<Transform>(true)[0];
        Quaternion modelRotation = modelTransform.rotation;
        clearPlateObjects();
        if (food == null) {
            //food = (GameObject) Resources.Load("mushPlatePrefab", typeof(GameObject));
            if (ingredientList.Count > ingredientObjects.Count)
            {
                foreach (Ingredient ingred in ingredientList)
                {
                    addIngredientToPlate(ingred);
                }
            }
        }
        model = (GameObject)Instantiate(food, modelTransform.position, modelRotation);
      } else {
        model = null;
      }
      updateTextList();
    }

    public void serveFood() {
      if (!string.Equals(recipe.Name, "mush")) {
        player.sendScoreToServer(recipe);
        clearPlate();
      }
    }

    public void addIngredient() {
      if (Player.currentIngred != null) {
        addIngredientToPlate(Player.currentIngred);
        player.notifyServerAboutIngredientPlaced(Player.currentIngred);
        player.removeCurrentIngredient();
        ingredientList = Player.ingredientsFromStation;
        displayFood();
      }
    }

    private void addIngredientToPlate(Ingredient ingredient)
    {
        GameObject ingred = (GameObject)Resources.Load(ingredient.Model, typeof(GameObject));
        Transform ingredTransform = ingred.GetComponentsInChildren<Transform>(true)[0];
        Quaternion ingredRotation = ingredTransform.rotation;
        Vector3 ingredPosition = ingredTransform.position + new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), 0);
        GameObject inst = Instantiate(ingred, ingredPosition, ingredRotation);
        ingredientObjects.Add(inst);
    }

    public void goBack() {
      SceneManager.LoadScene("PlayerMainScreen");
    }

    public void clearPlate()
    {
        ingredientList.Clear();
        player.clearIngredientsInStation("3");
        recipe = null;
        clearPlateObjects();
    }

    private void clearPlateObjects()
    {
        foreach(GameObject ingred in ingredientObjects) Destroy(ingred, 0.0f);

        ingredientObjects.Clear();
        Destroy(model, 0.0f);
    }
}
