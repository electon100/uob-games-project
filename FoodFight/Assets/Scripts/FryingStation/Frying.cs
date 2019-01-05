using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text;

public class Frying : MonoBehaviour {

	public Text test_text;

	float accelerometerUpdateInterval = 1.0f / 60.0f;
	float lowPassKernelWidthInSeconds = 1.0f;
	float shakeDetectionThreshold = 2.0f;
	float lowPassFilterFactor;
	Vector3 lowPassValue;

	// Use this for initialization
	void Start () {
		test_text.text = "Hello!";
		lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
    shakeDetectionThreshold *= shakeDetectionThreshold;
    lowPassValue = Input.acceleration;
	}

	// Update is called once per frame
	void Update () {
		Vector3 acceleration = Input.acceleration;
    lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
    Vector3 deltaAcceleration = acceleration - lowPassValue;

    if (deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold) {
			/* Shake detected! */
			test_text.text = "Shake at " + Time.time + ".";
    }
	}

	public void goBack() {
		SceneManager.LoadScene("PlayerMainScreen");
	}
}
