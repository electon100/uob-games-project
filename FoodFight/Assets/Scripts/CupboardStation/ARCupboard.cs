using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ARCupboard : MonoBehaviour
{
    public static Ingredient ingredient;
    public Text foodName;
    public GameObject imageTargetCupboard;
    public GameObject imageTargetFridge;
    public GameObject goBackButton;
    public GameObject goBackButtonBig;
    public GameObject backArrow;
    public GameObject infoPanel;
    public GameObject fadeBackground;
    public Text infoText;
    private Player player;
    private RaycastHit hit;
    private Ray ray;

    void Start()
    {
      Screen.orientation = ScreenOrientation.Portrait;
      if (Client.gameState.Equals(ClientGameState.MainMode)) {
        DontDestroyOnLoad(GameObject.Find("Player"));
        infoPanel.SetActive(false);
        fadeBackground.SetActive(false);
      } else {
        DontDestroyOnLoad(GameObject.Find("SimulatedPlayer"));
      }
    }

    void Update()
    {
        bool isDesktop = Input.GetMouseButtonDown(0);
        bool isMobile = (Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began);
        if (isDesktop || isMobile) {
            Ray raycast = (isDesktop) ? Camera.main.ScreenPointToRay(Input.mousePosition) :
                                        Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit raycastHit;
            if (Physics.Raycast(raycast, out raycastHit)) {
                checkItemPressed(raycastHit.transform.gameObject.name);
            }
        }

    }

    public void checkItemPressed(string itemName) {
        /* Assigns the variable of the item picked */
        switch(itemName) {
            case "Potatoes":
                ingredient = new Ingredient("potato", "potatoPrefab");
                foodName.text = "You picked a potato!";
                break;
            case "Tortillas":
                ingredient = new Ingredient("tortillas", "tortillasPrefab");
                foodName.text = "You picked some tortillas!";
                break;
            case "Vegetables":
                ingredient = new Ingredient("mixed_vegetables", "mixed_vegetablesPrefab");
                foodName.text = "You picked some vegetables!";
                break;
            case "Onion":
                ingredient = new Ingredient("onion", "onionPrefab");
                foodName.text = "You picked some onions!";
                break;
            case "Milk":
                ingredient = new Ingredient("milk", "milkPrefab");
                foodName.text = "You picked a bottle of milk!";
                break;
            case "Eggs":
                ingredient = new Ingredient("eggs", "eggsPrefab");
                foodName.text = "You picked some eggs!";
                break;
            case "Noodles":
                ingredient = new Ingredient("noodles", "noodlesPrefab");
                foodName.text = "You picked some noodles!";
                break;
            case "Flour":
                ingredient = new Ingredient("flour", "flourPrefab");
                foodName.text = "You picked a bag of flour!";
                break;
            case "Chicken":
                ingredient = new Ingredient("chicken", "chickenPrefab");
                foodName.text = "You picked some chicken!";
                break;
            case "SoySauce":
                ingredient = new Ingredient("soy_sauce", "soy_saucePrefab");
                foodName.text = "You picked some soy sauce!";
                break;
            case "Steak":
                ingredient = new Ingredient("raw_steak", "raw_steakPrefab");
                foodName.text = "You picked a raw steak!";
                break;
            case "GreenPeas":
                ingredient = new Ingredient("peas", "peasPrefab");
                foodName.text = "You picked some peas!";
                break;
            case "Shrimp":
                ingredient = new Ingredient("shrimp", "shrimpPrefab");
                foodName.text = "You picked some shrimps!";
                break;
            case "Rice":
                ingredient = new Ingredient("rice", "ricePrefab");
                foodName.text = "You picked some rice!";
                break;
            case "Cheese":
                ingredient = new Ingredient("cheese", "cheesePrefab");
                foodName.text = "You picked some cheese!";
                break;
            case "Calamari":
                ingredient = new Ingredient("raw_calamari", "raw_calamariPrefab");
                foodName.text = "You picked some raw calamari!";
                break;        
            case "Oil":
                ingredient = new Ingredient("oil", "oilPrefab");
                foodName.text = "You picked some oil!";
                break;
            case "ChocolateSauce":
                ingredient = new Ingredient("chocolate_sauce", "chocolate_saucePrefab");
                foodName.text = "You picked some chocolate sauce!";
                break;        
            default:
                break;
        }
        /* Sets the player's current ingredient to that item */
        if (Client.gameState.Equals(ClientGameState.MainMode)) {
          Player.currentIngred = ingredient;
        } else { /*Entered in tutorial mode */
          if (!ingredient.Name.Equals("potato")) {
            infoPanel.SetActive(true);
            fadeBackground.SetActive(true);
            imageTargetCupboard.SetActive(false);
            imageTargetFridge.SetActive(false);
            foodName.text = "";
            infoText.text = "That's not a potato! \n Try one more time.";
          } else {
            SimulatedPlayer.currentIngred = ingredient;
            backArrow.SetActive(true);
          }
        }
    }

    /* Notify server that player has left the station */
    public void goBack()
    {
        Handheld.Vibrate();
        if (Client.gameState.Equals(ClientGameState.MainMode)) {
          player = GameObject.Find("Player").GetComponent<Player>();
          player.notifyAboutStationLeft();
          SceneManager.LoadScene("PlayerMainScreen");
        } else if (FoodData.Instance.isChoppable(ingredient)) { /* Set the mode to the next step of the tutorial */
          Client.gameState = ClientGameState.ChoppingTutorial;
          SceneManager.LoadScene("PlayerMainScreen");
        } else if (ingredient.Name != "potato") {
           infoPanel.SetActive(true);
          fadeBackground.SetActive(true);
          imageTargetCupboard.SetActive(false);
          imageTargetFridge.SetActive(false);
          infoText.text = "Make sure you grab \n a potato before leaving!";
        } 
    }

    /* Reset canvas if player has picked something by mistake. */
    public void pickAgain() {
        goBackButtonBig.SetActive(false);
        backArrow.SetActive(false);
        foodName.text = "";
        imageTargetCupboard.SetActive(true);
        imageTargetFridge.SetActive(true);
    }

    public void GotIt() {
      infoPanel.SetActive(false);
      fadeBackground.SetActive(false);
      imageTargetCupboard.SetActive(true);
      imageTargetFridge.SetActive(true);
    }
}
