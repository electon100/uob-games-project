using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ViewItem : MonoBehaviour {

	public Text mainText;
	public Text ingredText;

	private GameObject currentItem;

	// Use this for initialization
	void Start () {
		viewItems();
	}

	public void viewItems() {
		/* If the current item is null, instantiate it when viewing */
		if (Client.gameState.Equals(ClientGameState.MainMode)) {
			if (Player.isHoldingIngredient()) {
				GameObject model = (GameObject) Resources.Load(Player.currentIngred.Model, typeof(GameObject));
				Transform modelTransform = model.GetComponentsInChildren<Transform>(true)[0];
				Vector3 modelPosition = modelTransform.position;
				Quaternion modelRotation = modelTransform.rotation;
				currentItem = (GameObject) Instantiate(model, modelPosition, modelRotation);
				ingredText.text = Player.currentIngred.ToString();
			}
			else {
				ingredText.text = "Nothing";
			}
		} else {
			if (SimulatedPlayer.isHoldingIngredient()) {
				GameObject model = (GameObject) Resources.Load(SimulatedPlayer.currentIngred.Model, typeof(GameObject));
				Transform modelTransform = model.GetComponentsInChildren<Transform>(true)[0];
				Vector3 modelPosition = modelTransform.position;
				Quaternion modelRotation = modelTransform.rotation;
				currentItem = (GameObject) Instantiate(model, modelPosition, modelRotation);
				ingredText.text = SimulatedPlayer.currentIngred.ToString();
			}
			else {
				ingredText.text = "Nothing";
			}
		}
	}

	// Update is called once per frame
	void Update () {

	}
}
