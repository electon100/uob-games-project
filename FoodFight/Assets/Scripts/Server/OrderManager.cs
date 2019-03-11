using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour {

	public Manager manager;

	private List<Ingredient> redOrders = new List<Ingredient>();
	private List<Ingredient> blueOrders = new List<Ingredient>();

	private int maxOrders = 3;

	// Use this for initialization
	void Start () {
		manager = GameObject.Find("Manager").GetComponent<Manager>();
	}

	// Update is called once per frame
	void Update () {
		if (redOrders.Count < maxOrders) redOrders = populateOrders(redOrders);
		if (blueOrders.Count < maxOrders) blueOrders = populateOrders(blueOrders);
	}

	public void scoreRecipe(Ingredient finalRecipe, string team) {
		int score = FoodData.Instance.getScoreForIngredient(finalRecipe);

		if (team.Equals("red")) manager.increaseRed(score);
		else if (team.Equals("blue")) manager.increaseBlue(score);
	}

	private List<Ingredient> populateOrders(List<Ingredient> orders) {
		while(orders.Count < maxOrders) orders.Add(null);

		return orders;
	}
}
