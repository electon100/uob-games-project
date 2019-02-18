using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ViewItem : MonoBehaviour {

	public GameObject mainPanel;
	public Text mainText;
	public Text ingredText;

	private GameObject currentItem;

	// Use this for initialization
	void Start () {
		viewItems();
	}

	public void viewItems() {
		/* If the current item is null, instantiate it when viewing */
		if (Player.isHoldingIngredient()) {
			/* TODO: Sort out the scaling in unity I hate it I hate it */
			// GameObject model = (GameObject) Resources.Load(Player.currentIngred.Model, typeof(GameObject));
			// currentItem = (GameObject) Instantiate(model, new Vector3(45, 60, -200), Quaternion.identity);
			// currentItem.transform.SetParent(mainPanel.transform);
			// currentItem.transform.localScale = new Vector3(500.0f, 500.0f, 500.0f);
			ingredText.text = Player.currentIngred.Name;
		}
		ingredText.text = "Nothing";
	}

	// Update is called once per frame
	void Update () {

	}
}
