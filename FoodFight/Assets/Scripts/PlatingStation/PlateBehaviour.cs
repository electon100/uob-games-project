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
    // Ingredient the player is holding
    string newIngredient;
    // All ingredients on the plate
    List<string> ingredients = new List<string>();
    List<Ingredient> ingredientList;
    // Holds the name of the recipe
    string recipe = null;
    // The text list of ingredients to be displayed
    Text ingList;

    // Cameras
    Camera cameraEmpty;

    //Plates
    public GameObject mush;

    GameObject model;

    // Use this for initialization
    void Start () {

        // Get all camera objects
        cameraEmpty = GameObject.Find("Camera1").GetComponent<Camera>();

        ingList = GameObject.Find("Ingredient List").GetComponent<Text>();

        ingredientList = new List<Ingredient>() { new Ingredient() };

        displayFood();

	}

    bool checkRecipe() {
        return false;
        //TODO call API to check valid recipe

    }

    void displayFood() {
       if (ingredientList.Count > 0 && !checkRecipe()) {

         //Vector3 newPosition = new Vector3(spawnPoint.transform.position.x,spawnPoint.transform.position.y,spawnPoint.transform.position.z);
         //mush.position = newPosition;
        model  = Instantiate(mush, new Vector3(0,0,0), Quaternion.identity);
       } else if (ingredientList.Count > 0 && checkRecipe()) {
         //TODO put good prefab in
       }
    }

    public void serveFood() {
    }

    public void addIngredient() {
        //player = GameObject.Find("Player").GetComponent<Player>();
        //player.notifyServerAboutIngredientPlaced();
        //ingredientList = Player.ingredientsFromStation;
        displayFood();
    }
}
