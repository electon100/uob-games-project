using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text;

public class Frying : MonoBehaviour {

	public Text test_text;

	/* Phone motion stuff */
	float accelerometerUpdateInterval = 1.0f / 60.0f;
	float lowPassKernelWidthInSeconds = 1.0f;
	float shakeDetectionThreshold = 2.0f;
	float lowPassFilterFactor;
	Vector3 lowPassValue;

	/* Shaking stuff */
	float shakeSpeed = 10.0f; // Speed of pan shake
	float shakeAmount = 1.2f; // Amplitude of pan shake
	bool shouldShake = false;
	int negSinCount = 0, posSinCount = 0;
	Vector3 originalPos;
	private float lastShake;
	private int minimumShakeInterval = 1; // (seconds)

	/* Other */
	int panShakes = 0;

	void Start () {
		test_text.text = "Pan shakes: 0";
		lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
    shakeDetectionThreshold *= shakeDetectionThreshold;
    lowPassValue = Input.acceleration;
		originalPos = gameObject.transform.position;
		lastShake = Time.time;
	}

	void Update () {
		Vector3 acceleration = Input.acceleration;
    lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
    Vector3 deltaAcceleration = acceleration - lowPassValue;

		shakeIfNeeded();

    if (deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold) {
			/* Shake detected! */
			tryStartShake();
    }
	}

	private void tryStartShake() {
		if ((Time.time - lastShake) > minimumShakeInterval) {
			shouldShake = true;
			panShakes++;
			test_text.text = "Pan shakes: " + panShakes;
			lastShake = Time.time;
		}
	}

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

	public void goBack() {
		SceneManager.LoadScene("PlayerMainScreen");
	}
}
