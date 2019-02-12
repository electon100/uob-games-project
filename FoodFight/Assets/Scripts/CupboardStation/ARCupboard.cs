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
        Screen.orientation = ScreenOrientation.Portrait;
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
        goBackButton.SetActive(true);
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
        foodName.text = "You picked a bowl of noodles!";
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

    public void goBack()
    {
        Player.currentIngred = ingredient;
        SceneManager.LoadScene("PlayerMainScreen");
    }

}