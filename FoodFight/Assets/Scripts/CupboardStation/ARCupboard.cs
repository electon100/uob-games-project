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

    // Use this for initialization
    void Start()
    {
        DontDestroyOnLoad(GameObject.Find("Player"));
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void onPotato()
    {
        ingredient = new Ingredient("potato", "potatoPrefab");
        foodName.text = "You picked a potato!";
    }

    public void onVegetables()
    {
        ingredient = new Ingredient("mixed_vegetables", "vegetablesPrefab");
        foodName.text = "You picked some vegetables!";
    }

    public void onMilk()
    {
        ingredient = new Ingredient("milk", "milkPrefab");
        foodName.text = "You picked a bottle of milk!";
    }

    public void onEggs()
    {
        ingredient = new Ingredient("eggs", "eggsPrefab");
        foodName.text = "You picked some eggs!";
    }

    public void onNoodles()
    {
        ingredient = new Ingredient("noodles", "noodlesPrefab");
        foodName.text = "You picked a bowl of noodles!";
    }

    public void onFlour()
    {
        ingredient = new Ingredient("flour", "flourPrefab");
        foodName.text = "You picked a bag of flour!";
    }

    public void goBack()
    {
        Player.currentIngred = ingredient;
        Debug.Log("AR says: " + ingredient.Name + " " + ingredient.Model);
        Player.currentStation = "0";
        SceneManager.LoadScene("PlayerMainScreen");
    }

}