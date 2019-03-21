﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text;

public class Frying : MonoBehaviour {

	public Button goBackBtn, putBtn, pickBtn, clearBtn, combineBtn;
	public Text test_text;
	public Material successMaterial;
	public Material neutralMaterial;
	public Material issueMaterial;
	public Renderer background;
	public Player player;
	public AudioClip fryingSound, successSound;

	/* Phone motion stuff */
	private float accelerometerUpdateInterval = 1.0f / 60.0f;
	private float lowPassKernelWidthInSeconds = 1.0f;
	private float shakeDetectionThreshold = 2.0f;
	private float lowPassFilterFactor;
	private Vector3 lowPassValue;

	/* Shaking stuff */
	private float shakeSpeed = 10.0f; // Speed of pan shake
	private float shakeAmount = 1.2f; // Amplitude of pan shake
	private bool shouldShake = false;
	private int negSinCount = 0, posSinCount = 0;
	private Vector3 originalPos;
	private float lastShake;
	private int minimumShakeInterval = 1; // (seconds)
	private AudioSource source;

	/* Ingredient stuff */
	private readonly int maxPanContents = 3;
	public List<Ingredient> panContents = new List<Ingredient>();
	private List<GameObject> panContentsObjects = new List<GameObject>();

	/* Other */
	private bool ingredientCookedStationComplete = false;
	private Material backgroundStatus;

	void Start () {

		Screen.orientation = ScreenOrientation.Portrait;

		test_text.text = "Add ingredient to start";
		lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
		shakeDetectionThreshold *= shakeDetectionThreshold;
		lowPassValue = Input.acceleration;
		originalPos = gameObject.transform.position;
		lastShake = Time.time;
		source = GetComponent<AudioSource>();

		background.material = neutralMaterial;

		List<Ingredient> ingredientsFromStation = Player.ingredientsFromStation;

		clearPan();

		player = GameObject.Find("Player").GetComponent<Player>();

		foreach (Ingredient ingredient in ingredientsFromStation) {
			addIngredientToPan(ingredient);
		}
	}

	void Update () {
		/* Ensure correct buttons are interactable */
		updateButtonStates();
		shakeIfNeeded();

		if (ingredientCookedStationComplete) {
			test_text.text = "Ingredient cooked!";
			background.material = successMaterial;
		} else {
			if (panContents.Count == 1) {

				/* Read accelerometer data */
				Vector3 acceleration = Input.acceleration;
				lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
				Vector3 deltaAcceleration = acceleration - lowPassValue;

				/* Input keydown for desktop tests. */
				if ((Input.GetKeyDown(KeyCode.DownArrow)) || deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold) {
					/* Shake detected! */
					tryStartShake();
				}

				/* Increment the number of pan tosses of all ingredients in pan */
				Ingredient ingredient = panContents[0];

				if (FoodData.Instance.isCooked(ingredient)) {
					Ingredient newIngred = FoodData.Instance.TryAdvanceIngredient(ingredient);
					if (isValidRecipe(newIngred)) {
						setPanContents(newIngred);
						source.PlayOneShot(successSound);
						ingredientCookedStationComplete = true;
					}
				}

			} else {
				/* TODO: What happens when pan is empty or too full */
				if (panContents.Count > maxPanContents) Debug.Log("Pan got too full!");
			}
		}

		if (Input.GetKeyDown(KeyCode.E)) {
			foreach (Ingredient ingredient in panContents) {
				Debug.Log(ingredient.Name);
			}
		}
	}

	/* Requires only one item in the pan */
	private void tryStartShake() {
		/* Make sure shake is not too soon after previous shake */
		if ((Time.time - lastShake) > minimumShakeInterval) {
			Ingredient ingredient = panContents[0];
			if (FoodData.Instance.isCookable(ingredient)) {
				ingredient.numberOfPanFlips++;
				lastShake = Time.time;
				shouldShake = true;
				source.PlayOneShot(fryingSound);
				test_text.text = "Pan shakes: " + ingredient.numberOfPanFlips;
			} else {
				test_text.text = "Ingredient not cookable";
				background.material = issueMaterial;
			}
			/* Increment the number of pan tosses of ingredient in pan */
		}
	}

	private bool isValidRecipe(Ingredient recipe) {
		return !string.Equals(recipe.Name, "mush");
	}

	private void setPanContents(Ingredient ingredient) {
		clearStation();

		addIngredientToPan(ingredient);
		player.notifyServerAboutIngredientPlaced(ingredient);
	}

	/* Manages the sinusoidal movement of the pan */
	private void shakeIfNeeded() {
		if (shouldShake) {
			float xTransform = -1 * Mathf.Sin((Time.time - lastShake) * shakeSpeed) * shakeAmount;

			if (negSinCount > 0 && posSinCount > 0 && xTransform < 0) {
				gameObject.transform.position = originalPos;
				negSinCount = 0; posSinCount = 0;
				shouldShake = false;
			}	else if (xTransform < 0) {
				transform.Translate(0, 0, xTransform);
				negSinCount++;
			} else if (xTransform > 0) {
				transform.Translate(0, 0, xTransform);
				posSinCount++;
			}
		}
	}

	public void placeHeldIngredientInPan() {
		/* Add ingredient */
		if (Player.isHoldingIngredient()) {
			if (panContents.Count < maxPanContents) {
				addIngredientToPan(Player.currentIngred);

				/* Notify server that player has placed ingredient */
				player.notifyServerAboutIngredientPlaced(Player.currentIngred);

				Player.removeCurrentIngredient();
			} else {
				test_text.text = "Pan is full";
				background.material = issueMaterial;
			}
		} else {
			/* TODO: What happens when player is not holding an ingredient */
			test_text.text = "No held ingredient";
		}
	}

	public void combineIngredientsInPan()	{
		if (panContents.Count > 1) {
			/* Try and combine the ingredients */
			Ingredient combinedFood = FoodData.Instance.TryCombineIngredients(panContents);

			if (!isValidRecipe(combinedFood)) {
				test_text.text = "Ingredients do not combine";
				background.material = issueMaterial;
			} else {
				/* Set the pan contents to the new combined recipe */
				setPanContents(combinedFood);
				test_text.text = "Ingredients combined";
			}
		} else {
			test_text.text = "No ingredients to combine";
		}
	}

	public void clearStation() {
		clearPan();
		test_text.text = "Add ingredient to start";
		background.material = neutralMaterial;
		player.clearIngredientsInStation();
	}

	public void pickUpIngredient() {
		if (panContents.Count == 1) {
			/* Set the players current ingredient to the pan contents */
			foreach (Ingredient ingredient in panContents) {
				Player.currentIngred = ingredient;
			}

			/* Clear the station */
			clearStation();
		} else {
			/* What to do if there are more than (or fewer than) 1 ingredients in the pan*/
			test_text.text = "Unable to pick up";
		}
	}

	private void addIngredientToPan(Ingredient ingredient) {
		GameObject model = (GameObject) Resources.Load(ingredient.Model, typeof(GameObject));
		Transform modelTransform = model.GetComponentsInChildren<Transform>(true)[0];

		Quaternion modelRotation = modelTransform.rotation;
		Vector3 modelPosition = modelTransform.position;
		GameObject inst = Instantiate(model, modelPosition, modelRotation);
		panContents.Add(ingredient);
		panContentsObjects.Add(inst);
		if (panContents.Count > 1) {
			test_text.text = "Combine ingredients to cook";
			background.material = issueMaterial;
		} else {
			test_text.text = "Shake phone to cook";
			background.material = neutralMaterial;
		}
	}

	private void updateButtonStates() {
		setButtonInteractable(putBtn, Player.isHoldingIngredient() && panContents.Count < maxPanContents);
		setButtonInteractable(clearBtn, panContents.Count > 0);
		setButtonInteractable(pickBtn, panContents.Count == 1);
		setButtonInteractable(combineBtn, panContents.Count > 1);
	}

	private void setButtonInteractable(Button btn, bool interactable) {
		btn.interactable = interactable;
	}

	private void clearPan()	{
		foreach (GameObject go in panContentsObjects) Destroy(go);

		panContents.Clear();
		panContentsObjects.Clear();
	}

	public void goBack() {
		/* TODO: Need to notify server of local updates to ingredients in pan before leaving */
		/* Notify server that player has left the station */
		Handheld.Vibrate();
		player.notifyAboutStationLeft();
		SceneManager.LoadScene("PlayerMainScreen");
	}
}
