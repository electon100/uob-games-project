using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARCupboard : MonoBehaviour {
    public List<string> ingredients = new List<string>();
    public Text foodName;

	// Use this for initialization
	void Start () {

    }

    // Update is called once per frame
    void Update () {
    }

    public void onBanana()
    {
        ingredients.Add("banana");
        foodName.text = "banana";
    }

    public void onCheese()
    {
        ingredients.Add("cheese");
        foodName.text = "cheese";
    }

    public void onTomato()
    {
        ingredients.Add("tomato");
        foodName.text = "tomato";
    }

    public void onChicken()
    {
        ingredients.Add("chicken");
        foodName.text = "chicken";
    }

    public void onBread()
    {
        ingredients.Add("bread");
        foodName.text = "bread";
    }

    public void onMeat()
    {
        ingredients.Add("meat");
        foodName.text = "meat";
    }

    public void onPepperoni()
    {
        ingredients.Add("pepperoni");
        foodName.text = "pepperoni";
    }

    List <string> getList()
    {
        return this.ingredients;
    }
}
