using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text;

public class Frying : MonoBehaviour {

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
	public List<Ingredient> newPanContents = new List<Ingredient>();

    /* Other */
    int panShakes = 0;

	void Start () {

		Screen.orientation = ScreenOrientation.Portrait;

		test_text.text = "Pan shakes: 0";
		lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
        shakeDetectionThreshold *= shakeDetectionThreshold;
        lowPassValue = Input.acceleration;
		originalPos = gameObject.transform.position;
		lastShake = Time.time;

        panContents = new List<Ingredient>();
    }

	void Update () {
        /* If available, add the held ingredient to the pan */
        newPanContents = Player.ingredientsFromStation;

        /* Draw ingredient models in pan */
        foreach (Ingredient ingredient in newPanContents)
        {
            if (panContents.IndexOf(ingredient) < 0)
            {
                GameObject model = (GameObject)Resources.Load(ingredient.Model, typeof(GameObject));
                Instantiate(model, new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), 85), Quaternion.Euler(-90, 0, 0));
                panContents.Add(ingredient);
            }
        }
        
        if (panContents.Count > 0) {

			/* Read accelerometer data */
			Vector3 acceleration = Input.acceleration;
			lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
			Vector3 deltaAcceleration = acceleration - lowPassValue;

			shakeIfNeeded();

			if (deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold) {
				/* Shake detected! */
				tryStartShake();
			}
		} else {
			/* TODO: What happens when the player isn't holding an ingredient */
		}
	}

	private void tryStartShake() {
		/* Make sure shake is not too soon after previous shake */
		if ((Time.time - lastShake) > minimumShakeInterval) {
			shouldShake = true;
			panShakes++;

			/* Increment the number of pan tosses of all ingredients in pan */
			foreach (Ingredient ingredient in panContents) {
				ingredient.numberOfPanFlips++;
			}

			/* Update shake text */
			test_text.text = "Pan shakes: " + panShakes;
			lastShake = Time.time;
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

    public void putIngredientInPan()
    {
        // Instantiates the ingredient and adds it to the pan contents list.
        if (Player.currentIngred != null)
        {
            panContents.Add(Player.currentIngred);
            GameObject model = (GameObject)Resources.Load(Player.currentIngred.Model, typeof(GameObject));
            Instantiate(model, new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), 85), Quaternion.Euler(-90, 0, 0));
            Debug.Log("Ingredient added to pan: " + Player.currentIngred.Name);
        }
        // Tells the server that this ingredient is put in the pan
        player = GameObject.Find("Player").GetComponent<Player>();
        player.notifyServerAboutIngredientPlaced();
    }

	public void goBack() {
        Player.currentStation = "1";
        SceneManager.LoadScene("PlayerMainScreen");
	}
}
