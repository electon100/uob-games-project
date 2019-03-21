using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderManager : MonoBehaviour {

	//public Manager manager;
	public GameObject orderPanel;

	private List<Order> redOrders = new List<Order>();
	private List<Order> blueOrders = new List<Order>();

	public Text redOrderText;
	public Text blueOrderText;

	private int maxOrders = 3;

	// Use this for initialization
	void Start () {
		//manager = GameObject.Find("Manager").GetComponent<Manager>();

		redOrders.Add(new Order(new Ingredient("chips", "chipsPrefab"), new GameObject(), 120, orderPanel));
		blueOrders.Add(new Order(new Ingredient("chips", "chipsPrefab"), new GameObject(), 120, orderPanel));
	}

	// Update is called once per frame
	void Update () {
		while (redOrders.Count < maxOrders) addOrder(ref redOrders);
		while (blueOrders.Count < maxOrders) addOrder(ref blueOrders);

		displayOrders();
	}

	public void scoreRecipe(Ingredient finalRecipe, string team) {
		int score = FoodData.Instance.getScoreForIngredient(finalRecipe);

		bool isOrder = false;

		if (team.Equals("red")) {
			for (int i = 0; i < redOrders.Count; i++) {
				if (redOrders[i].Recipe.Name == finalRecipe.Name) {
					redOrders.Remove(redOrders[i]);
					isOrder = true;
					break;
				}
			}
			//manager.increaseRed(score * ((isOrder) ? 1 : 0));
		} else if (team.Equals("blue")) {
			for (int i = 0; i < blueOrders.Count; i++) {
				if (blueOrders[i].Recipe.Name == finalRecipe.Name) {
					blueOrders.Remove(blueOrders[i]);
					isOrder = true;
					break;
				}
			}
			//manager.increaseBlue(score * ((isOrder) ? 1 : 0));
		}

	}

	private void addOrder(ref List<Order> orders) {
		string recipeName = FoodData.Instance.getRandomRecipeName();
		orders.Add(new Order(new Ingredient(recipeName, recipeName + "Prefab"), new GameObject(recipeName + "ParentObject"), 120, orderPanel));
	}

	private void displayOrders() {
		redOrderText.text = "Orders:\n";
		blueOrderText.text = "Orders:\n";

		for (int i = 0; i < maxOrders; i++) {
			//Vector3 zeroVec = new Vector3(0,0,0);
			//redOrders[i].updateCanvas(zeroVec);
			//blueOrders[i].updateCanvas(zeroVec);
			redOrders[i].updateCanvas(new Vector3(-200, -200 - (i*120), 0));
			blueOrders[i].updateCanvas(new Vector3(200, -200 - (i*120), 0));
		}
	}
}
