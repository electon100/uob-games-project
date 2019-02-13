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
	public AudioClip fryingSound;

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
	public List<Ingredient> panContents = new List<Ingredient>();
	private List<GameObject> panContentsObjects = new List<GameObject>();

	/* Other */

	void Start () {

		Screen.orientation = ScreenOrientation.Portrait;

		test_text.text = "Pan shakes: 0";
		lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
		shakeDetectionThreshold *= shakeDetectionThreshold;
		lowPassValue = Input.acceleration;
		originalPos = gameObject.transform.position;
		lastShake = Time.time;
		source = GetComponent<AudioSource>();

		List<Ingredient> ingredientsFromStation = Player.ingredientsFromStation;

		clearPan();

		foreach (Ingredient ingredient in ingredientsFromStation) {
			addIngredientToPan(ingredient);
		}
	}

	void Update () {
		if (panContents.Count == 1) {

			/* Read accelerometer data */
			Vector3 acceleration = Input.acceleration;
			lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
			Vector3 deltaAcceleration = acceleration - lowPassValue;

			shakeIfNeeded();

			/* For desktop tests. */
			if (Input.GetKeyDown(KeyCode.DownArrow)) {
				tryStartShake();
			}

			if (deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold) {
				/* Shake detected! */
				tryStartShake();
			}

		} else {
			if (panContents.Count == 0) {
				test_text.text = "Pan is empty";
			} else {
				test_text.text = "Combine ingredients to cook";
			}
			/* TODO: What happens when pan is empty or too full */
		}
		if (Input.GetKeyDown(KeyCode.E)) {
			foreach (Ingredient ingredient in panContents) {
				Debug.Log(ingredient.Name);
			}
		}
	}

	private void tryStartShake() {
		/* Make sure shake is not too soon after previous shake */
		if ((Time.time - lastShake) > minimumShakeInterval) {
			shouldShake = true;

			source.PlayOneShot(fryingSound);

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
			test_text.text = "No held ingredient";
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
		player = GameObject.Find("Player").GetComponent<Player>();
		player.clearIngredientsInStation(stationID);

		addIngredientToPan(combinedFood);
		player.notifyServerAboutIngredientPlaced(combinedFood);
	}

	public void pickUpIngredient()
	{
		if (panContents.Count == 1) {
			/* Set the players current ingredient to the pan contents */
			foreach (Ingredient ingredient in panContents) {
				Player.currentIngred = ingredient;
			}

			/* Clear the pan */
			clearPan();
			player = GameObject.Find("Player").GetComponent<Player>();
			player.clearIngredientsInStation(stationID);
		} else {
			/* What to do if there are more than (or fewer than) 1 ingredients in the pan*/
			Debug.Log("Unable to pick up");
			test_text.text = "Unable to pick up";
		}
	}

	private void addIngredientToPan(Ingredient ingredient)
	{
		GameObject model = (GameObject) Resources.Load(ingredient.Model, typeof(GameObject));
		GameObject inst = Instantiate(model, new Vector3(Random.Range(-14, -8), Random.Range(1, 9), 85), Quaternion.Euler(-90, 0, 0));
		panContents.Add(ingredient);
		panContentsObjects.Add(inst);
	}

	private void clearPan()
	{
		foreach (GameObject go in panContentsObjects) Destroy(go);

		panContents.Clear();
		panContentsObjects.Clear();
	}

	public void goBack()
	{
		/* TODO: Need to notify server of local updates to ingredients in pan before leaving */
		SceneManager.LoadScene("PlayerMainScreen");
	}
}
