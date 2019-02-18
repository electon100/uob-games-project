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
    public GameObject goBackButton;
    public GameObject goBackButtonBig;

    // Use this for initialization
    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        DontDestroyOnLoad(GameObject.Find("Player"));
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void toggleButtons() {
        goBackButton.SetActive(false);
        goBackButtonBig.SetActive(true);
        GameObject.Find("ImageTarget").SetActive(false);
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
        ingredient = new Ingredient("mixed_vegetables", "vegetablesPrefab");
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

    public void goBack()
    {
        Player.currentIngred = ingredient;
        SceneManager.LoadScene("PlayerMainScreen");
    }

}