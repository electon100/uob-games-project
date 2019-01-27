using UnityEngine;

public class Ingredient {

	public string Name { get; set; }

	public GameObject Model { get; set; }

	public int panTosses { get; set; }

	public bool isChopped { get; set; }

    public bool isCooked { get; set; }

	public bool isCookable { get; set; }

	public bool isChoppable { get; set; }


    public Ingredient(string name, GameObject model) {
		Name = name;
		Model = model;
		panTosses = 0;
		isChopped = false;
		isCookable = true;
		isChoppable = true;
	}

}
