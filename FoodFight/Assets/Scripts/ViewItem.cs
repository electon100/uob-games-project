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
			GameObject model = (GameObject) Resources.Load(Player.currentIngred.Model, typeof(GameObject));
			Transform modelTransform = model.GetComponentsInChildren<Transform>(true)[0];
			Quaternion modelRotation = modelTransform.rotation;
			currentItem = (GameObject) Instantiate(model, new Vector3(0, 0, 0), modelRotation);
			ingredText.text = Player.currentIngred.ToString();
		}
		else {
			ingredText.text = "Nothing";
		}
	}

	// Update is called once per frame
	void Update () {

	}
}
