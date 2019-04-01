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
    private Player player;
    private RaycastHit hit;
    private Ray ray;

    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        DontDestroyOnLoad(GameObject.Find("Player"));
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
                onPotato();
                break;
            case "Vegetables":
                onVegetables();
                break;
            case "Milk":
                onMilk();
                break;
            case "Eggs":
                onEggs();
                break;
            case "Noodles":
                onNoodles();
                break;
            case "Flour":
                onFlour();
                break;
            case "Chicken":
                onChicken();
                break;
            case "SoySauce":
                onSoySauce();
                break;
            case "Steak":
                onSteak();
                break;
            case "GreenPeas":
                onPeas();
                break;
            case "Duck":
                onDuck();
                break;
            case "Shrimp":
                onShrimp();
                break;
            case "Rice":
                onRice();
                break;
            default:
                break;
        }
        /* Sets the player's current ingredient to that item */
        Player.currentIngred = ingredient;
    }

    public void toggleButtons() {
        backArrow.SetActive(true);
    }

    public void onPotato()
    {
        toggleButtons();
        ingredient = new Ingredient("potato", "potatoPrefab");
        foodName.text = "You picked a potato!";
    }

    public void onVegetables()
    {
        toggleButtons();
        ingredient = new Ingredient("mixed_vegetables", "mixed_vegetablesPrefab");
        foodName.text = "You picked some vegetables!";
    }

    public void onMilk()
    {
        toggleButtons();
        ingredient = new Ingredient("milk", "milkPrefab");
        foodName.text = "You picked a bottle of milk!";
    }

    public void onEggs()
    {
        toggleButtons();
        ingredient = new Ingredient("eggs", "eggsPrefab");
        foodName.text = "You picked some eggs!";
    }

    public void onNoodles()
    {
        toggleButtons();
        ingredient = new Ingredient("noodles", "noodlesPrefab");
        foodName.text = "You picked some noodles!";
    }

    public void onFlour()
    {
        toggleButtons();
        ingredient = new Ingredient("flour", "flourPrefab");
        foodName.text = "You picked a bag of flour!";
    }

    public void onChicken()
    {
        toggleButtons();
        ingredient = new Ingredient("chicken", "chickenPrefab");
        foodName.text = "You picked some chicken!";
    }

    public void onSoySauce()
    {
        toggleButtons();
        ingredient = new Ingredient("soy_sauce", "soy_saucePrefab");
        foodName.text = "You picked some soy sauce!";
    }

    public void onSteak()
    {
        toggleButtons();
        ingredient = new Ingredient("raw_steak", "raw_steakPrefab");
        foodName.text = "You picked a raw steak!";
    }

    public void onPeas()
    {
        toggleButtons();
        ingredient = new Ingredient("peas", "peasPrefab");
        foodName.text = "You picked some peas!";
    }

    public void onDuck()
    {
        toggleButtons();
        ingredient = new Ingredient("duck", "duckPrefab");
        foodName.text = "You picked some duck!";
    }

    public void onShrimp()
    {
        toggleButtons();
        ingredient = new Ingredient("shrimp", "shrimpPrefab");
        foodName.text = "You picked some shrimps!";
    }

    public void onRice()
    {
        toggleButtons();
        ingredient = new Ingredient("rice", "ricePrefab");
        foodName.text = "You picked some rice!";
    }

    /* Notify server that player has left the station */
    public void goBack()
    {
        Handheld.Vibrate();
		player = GameObject.Find("Player").GetComponent<Player>();
		player.notifyAboutStationLeft();
        SceneManager.LoadScene("PlayerMainScreen");
    }

    /* Reset canvas if player has picked something by mistake. */
    public void pickAgain() {
        goBackButtonBig.SetActive(false);
        backArrow.SetActive(false);
        foodName.text = "";
        imageTargetCupboard.SetActive(true);
        imageTargetFridge.SetActive(true);
    }

}