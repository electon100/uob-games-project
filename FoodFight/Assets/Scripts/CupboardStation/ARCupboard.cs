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
        ingredient = new Ingredient("Potato", (GameObject) Resources.Load("PotatoesPrefab", typeof(GameObject)));
        foodName.text = "You picked a potato!";
    }

    public void onVegetables()
    {
        ingredient = new Ingredient("Vegetables", (GameObject) Resources.Load("VegetablesPrefab", typeof(GameObject)));
        foodName.text = "You picked some vegetables!";
    }

    public void onMilk()
    {
        ingredient = new Ingredient("Milk", (GameObject) Resources.Load("MilkPrefab", typeof(GameObject)));
        foodName.text = "You picked a bottle of milk!";
    }

    public void onEggs()
    {
        ingredient = new Ingredient("Eggs", (GameObject) Resources.Load("EggsPrefab", typeof(GameObject)));
        foodName.text = "You picked some eggs!";
    }

    public void onNoodles()
    {
        ingredient = new Ingredient("Noodles", (GameObject) Resources.Load("NoodlesPrefab", typeof(GameObject)));
        foodName.text = "You picked a bowl of noodles!";
    }

    public void onFlour()
    {
        ingredient = new Ingredient("Flour", (GameObject)Resources.Load("FlourPrefab", typeof(GameObject)));
        foodName.text = "You picked a bag of flour!";
    }

    public void goBack()
    {
        Player.currentStation = "0";
        SceneManager.LoadScene("PlayerMainScreen");
    }

}