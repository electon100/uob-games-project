using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ARCupboard : MonoBehaviour
{

    public static string ingredient;
    public Text foodName;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void onPotato()
    {
        ingredient = "Potato";
        foodName.text = "You picked a potato!";
    }

    public void onVegetables()
    {
        ingredient = "Vegetables";
        foodName.text = "You picked some vegetables!";
    }

    public void onMilk()
    {
        ingredient = "Milk";
        foodName.text = "You picked a bottle of milk!";
    }

    public void onEggs()
    {
        ingredient = "Eggs";
        foodName.text = "You picked some eggs!";
    }

    public void onNoodles()
    {
        ingredient = "Noodles";
        foodName.text = "You picked a bowl of noodles!";
    }

    public void goBack()
    {
        SceneManager.LoadScene("PlayerMainScreen");
    }

}