using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text;

public class Frying : MonoBehaviour {

	private readonly string stationID = "1";

	public Text test_text;
	public Player player;

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

	/* Ingredient stuff */

	/* List of ingredients in the pan, with current shake count applied.
		  -> Can be used externally to retrieve ingredients from pan */
	public List<Ingredient> panContents = new List<Ingredient>();
	private List<GameObject> panContentsObjects = new List<GameObject>();

    /* Other */
	// private bool isHobOn = false;

	void Start () {

		Screen.orientation = ScreenOrientation.Portrait;

		test_text.text = "Pan shakes: 0";
		lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
		shakeDetectionThreshold *= shakeDetectionThreshold;
		lowPassValue = Input.acceleration;
		originalPos = gameObject.transform.position;
		lastShake = Time.time;

		List<Ingredient> ingredientsFromStation = Player.ingredientsFromStation;
		// List<Ingredient> ingredientsFromStation = new List<Ingredient>();

		/* Create test ingredients */
		// Ingredient noodles = new Ingredient("noodles", "noodlesPrefab");
		// Ingredient veg = new Ingredient("chopped_mixed_vegetables", "chopped_mixed_vegetablesPrefab");
		// Ingredient chicken = new Ingredient("diced_chicken", "EggsPrefab");

		// Player.currentIngred = noodles;

		/* Add ingredients to list */
		// ingredientsFromStation.Add(noodles);
		// ingredientsFromStation.Add(chicken);
		// ingredientsFromStation.Add(veg);

		clearPan();

		foreach (Ingredient ingredient in ingredientsFromStation) {
			addIngredientToPan(ingredient);
		}
		test_text.text = "Last shake: " + lastShake;
	}

	void Update () {
		if (panContents.Count == 1) {

			/* Read accelerometer data */
			Vector3 acceleration = Input.acceleration;
			lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
			Vector3 deltaAcceleration = acceleration - lowPassValue;

			/* For desktop tests. */
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				/* Increment the number of pan tosses of all ingredients in pan */
				foreach (Ingredient ingredient in panContents) {
					ingredient.numberOfPanFlips++;
					if (FoodData.Instance.isCooked(ingredient)) {
						test_text.text = "Ingredient cooked!";
					} else {
						/* Update shake text */
						test_text.text = "Pan shakes: " + ingredient.numberOfPanFlips;
					}
					lastShake = Time.time;
				}
			}

			shakeIfNeeded();

			if (deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold) {
				/* Shake detected! */
				tryStartShake();
			}

		} else {
			/* TODO: What happens when pan is empty */
		}
		if (Input.GetKeyDown(KeyCode.E))
        {
            foreach (Ingredient ingredient in panContents) {
				Debug.Log(ingredient.Name);
			}
        }
	}

	private void tryStartShake() {
		/* Make sure shake is not too soon after previous shake */

		// test_text.text = "Time.time: " + Time.time;

		if ((Time.time - lastShake) > minimumShakeInterval) {
			shouldShake = true;

			/* Increment the number of pan tosses of all ingredients in pan */
			foreach (Ingredient ingredient in panContents) {
				ingredient.numberOfPanFlips++;
				if (FoodData.Instance.isCooked(ingredient)) {
					test_text.text = "Ingredient cooked!";
				} else {
					/* Update shake text */
					test_text.text = "Pan shakes: " + ingredient.numberOfPanFlips;
				}
				lastShake = Time.time;
			}

		}
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

	public void placeHeldIngredientInPan()
	{
		/* Add ingredient */
		if (Player.currentIngred != null) {
			addIngredientToPan(Player.currentIngred);

			/* Notify server that player has placed ingredient */
			player = GameObject.Find("Player").GetComponent<Player>();
			player.notifyServerAboutIngredientPlaced(Player.currentIngred);

			player.removeCurrentIngredient();
		} else {
			/* TODO: What happens when player is not holding an ingredient */
		}
	}

	public void combineIngredientsInPan()
	{
		/* Try and combine the ingredients */
		Ingredient combinedFood = FoodData.Instance.TryCombineIngredients(panContents);

		/* Set the pan contents to the new combined recipe */
		clearPan();
		/* I had to transfer this here because we don't wanna delete the ingredients
		from the server on Start. */
		player.clearIngredientsInStation(stationID);
		addIngredientToPan(combinedFood);

		player = GameObject.Find("Player").GetComponent<Player>();
		player.notifyServerAboutIngredientPlaced(combinedFood);
	}

	private void addIngredientToPan(Ingredient ingredient)
	{
		GameObject model = (GameObject) Resources.Load(ingredient.Model, typeof(GameObject));
		GameObject inst = Instantiate(model, new Vector3(Random.Range(-5, 5), Random.Range(-5, 5), 85), Quaternion.Euler(0, 0, 0));
		panContents.Add(ingredient);
		panContentsObjects.Add(inst);
	}

	private void clearPan()
	{
		foreach (GameObject go in panContentsObjects) Destroy(go);

		panContents.Clear();
		panContentsObjects.Clear();

		player = GameObject.Find("Player").GetComponent<Player>();
	}

	public void goBack()
	{
		foreach (Ingredient ingredient in panContents) {
			Player.currentIngred = ingredient;
		}
		SceneManager.LoadScene("PlayerMainScreen");
	}
}
