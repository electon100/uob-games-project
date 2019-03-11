using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderManager : MonoBehaviour {

	public Manager manager;

	private List<Ingredient> redOrders = new List<Ingredient>();
	private List<Ingredient> blueOrders = new List<Ingredient>();

	public Text redOrderText;
	public Text blueOrderText;

	private int maxOrders = 3;

	// Use this for initialization
	void Start () {
		manager = GameObject.Find("Manager").GetComponent<Manager>();

		redOrders.Add(new Ingredient("chips", "chipsPrefab"));
		blueOrders.Add(new Ingredient("chips", "chipsPrefab"));
	}

	// Update is called once per frame
	void Update () {
		if (redOrders.Count < maxOrders) redOrders = populateOrders(redOrders);
		if (blueOrders.Count < maxOrders) blueOrders = populateOrders(blueOrders);

		displayOrders();
	}

	public void scoreRecipe(Ingredient finalRecipe, string team) {
		int score = FoodData.Instance.getScoreForIngredient(finalRecipe);

		if (team.Equals("red")) {
			for (int i = 0; i < redOrders.Count; i++) {
				if (redOrders[i].Name == finalRecipe.Name) {
					redOrders.Remove(redOrders[i]);
					manager.increaseRed(score);
					break;
				}
			}
		} else if (team.Equals("blue")) {
			for (int i = 0; i < blueOrders.Count; i++) {
				if (blueOrders[i].Name == finalRecipe.Name) {
					blueOrders.Remove(blueOrders[i]);
					manager.increaseBlue(score);
					break;
				}
			}
		}
	}

	private List<Ingredient> populateOrders(List<Ingredient> orders) {
		while(orders.Count < maxOrders) {
			string recipeName = FoodData.Instance.getRandomRecipeName();
			orders.Add(new Ingredient(recipeName, recipeName + "Prefab"));
		}

		return orders;
	}

	private void displayOrders() {
		redOrderText.text = "Red Orders:\n";
		blueOrderText.text = "Blue Orders:\n";

		for (int i = 0; i < maxOrders; i++) {
			redOrderText.text += redOrders[i].ToString() + "\n";
			blueOrderText.text += blueOrders[i].ToString() + "\n";
		}
	}
}
