﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text;

public class NewChopping : MonoBehaviour {

	/* Scene stuff */
	public Button goBackBtn;
	public Text statusText;
	public Material successMaterial;
	public Material neutralMaterial;
	public Material issueMaterial;
	public Renderer background;
	public GameObject infoPanel;
	public GameObject fadeBackground;

	/* Sound stuff */
	public AudioClip chopSound;
  public AudioClip successSound;
  private AudioSource audioSource;

	private Player player;
	private GameObject ingredientInstantiation;
	private bool ingredientChoppedStationComplete = false;
	private Ingredient ingredientToChop;

	private readonly float minimumChopInterval = 0.25f; // seconds

	/* Movement stuff */
  private float shakeSpeed = 20.0f; // Speed of knife shake
  private float shakeAmount = 5f; // Amplitude of knife shake
  private bool shouldShake = false;
  private int negSinCount = 0, posSinCount = 0;
  private Vector3 originalPos;
  private float lastChop;
  private float yTransform;


	void Start () {
		Screen.orientation = ScreenOrientation.Portrait;
		background.material = neutralMaterial;
		audioSource = GetComponent<AudioSource>();

		/* Set up scene based on mode */
		if (Client.gameState.Equals(ClientGameState.MainMode)) {
			player = GameObject.Find("Player").GetComponent<Player>();
		} else {
			infoPanel.SetActive(true);
			fadeBackground.SetActive(true);
		}
		/* Load currently held ingredient into scene */
		if (Player.isHoldingIngredient() || SimulatedPlayer.isHoldingIngredient()) {
			GetIngredientFromPlayers();
			LoadHeldIngredient();
		}

		originalPos = gameObject.transform.position;
		lastChop = Time.time;
	}

	void Update () {

		KnifeMovement();

		if (ingredientChoppedStationComplete) {
			ChangeView("Ingredient chopped", successMaterial);
		} else if (Player.isHoldingIngredient() || SimulatedPlayer.isHoldingIngredient()) {
			if (FoodData.Instance.isChoppable(ingredientToChop)) {

				if (FoodData.Instance.isChopped(ingredientToChop)) {
					ingredientChoppedStationComplete = true;
					audioSource.PlayOneShot(successSound);

					ingredientToChop = FoodData.Instance.TryAdvanceIngredient(ingredientToChop);

					if (Client.gameState.Equals(ClientGameState.MainMode)) {
						Player.currentIngred = ingredientToChop;
					} else {
						SimulatedPlayer.currentIngred = ingredientToChop;
					}
			
					LoadHeldIngredient();
				}

				ChangeView("Shake phone to start chopping", neutralMaterial);

				if (DetectChop() || Input.GetKeyDown(KeyCode.DownArrow)) {
					/* Chop detected! */
					ResetKnifePosition();
					DoSingleChop();
				}

			} else {
				ChangeView("Ingredient not choppable", issueMaterial);
			}
		} else {
			ChangeView("No ingredient to chop", issueMaterial);
		}

	}

	private void GetIngredientFromPlayers() {
		if (!ingredientChoppedStationComplete) {
			/* Check if game is running in tutorial mode */
			if (Client.gameState.Equals(ClientGameState.MainMode)) {
				ingredientToChop = Player.currentIngred;
			} else {
				ingredientToChop = SimulatedPlayer.currentIngred;
			}
		}
	}
	private void KnifeMovement() {
    if (shouldShake) {
      float xTransform = -1 * Mathf.Sin((Time.time - lastChop) * shakeSpeed) * shakeAmount;

      if (negSinCount > 0 && posSinCount > 0 && xTransform < 0) {
        // ResetKnifePosition();
        negSinCount = 0; posSinCount = 0;
        shouldShake = false;
      }	else if (xTransform < 0) {
        transform.Rotate(0, xTransform, 0);
        negSinCount++;
      } else if (xTransform > 0) {
        transform.Rotate(0, xTransform, 0);
        posSinCount++;
      }
    }
  }

	private void ResetKnifePosition() {
		gameObject.transform.position = originalPos;
	}

	private void LoadHeldIngredient() {
		if (ingredientInstantiation != null) Destroy(ingredientInstantiation);
		GameObject model = (GameObject) Resources.Load(ingredientToChop.Model, typeof(GameObject));
		Transform modelTransform = model.GetComponentsInChildren<Transform>(true)[0];

		Quaternion modelRotation = modelTransform.rotation;
		Vector3 modelPosition = modelTransform.position;
		ingredientInstantiation = Instantiate(model, modelPosition, modelRotation);
	}

	private void DoSingleChop() {
    audioSource.PlayOneShot(chopSound);
    ingredientToChop.numberOfChops++;
    lastChop = Time.time;
		shouldShake = true;
	}

	private bool DetectChop() {
    return (Time.time - lastChop) > minimumChopInterval && Input.acceleration.y > 2.0f;
  }

	private void ChangeView(string message, Material material) {
		statusText.text = message;
		background.material = material;
	}

	public void OnGoBack() {
		goBack();
	}

	private void goBack() {
		/* Notify server that player has left the station */
		Handheld.Vibrate();
		if (Client.gameState.Equals(ClientGameState.MainMode)) {
			player.notifyAboutStationLeft();
		} else if (ingredientChoppedStationComplete) { /* Advance to the next step of the tutorial */
			Client.gameState = ClientGameState.FryingTutorial;
		}
		SceneManager.LoadScene("PlayerMainScreen");
	}

	public void GotIt() {
		infoPanel.SetActive(false);
		fadeBackground.SetActive(false);
	}
}
