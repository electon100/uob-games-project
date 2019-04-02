using System.Collections;
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

	/* Sound stuff */
	public AudioClip chopSound;
  public AudioClip successSound;
  private AudioSource audioSource;

	private Player player;
	private GameObject ingredientInstantiation;
	private bool ingredientChoppedStationComplete = false;

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
		player = GameObject.Find("Player").GetComponent<Player>();
		audioSource = GetComponent<AudioSource>();

		/* Load currently held ingredient into scene */
		if (Player.isHoldingIngredient()) {
			LoadHeldIngredient();
		}

		originalPos = gameObject.transform.position;
		lastChop = Time.time;
	}

	void Update () {

		KnifeMovement();

		if (ingredientChoppedStationComplete) {
			ChangeView("Ingredient chopped", successMaterial);
		} else if (Player.isHoldingIngredient()) {
			if (FoodData.Instance.isChoppable(Player.currentIngred)) {

				if (FoodData.Instance.isChopped(Player.currentIngred)) {
					ingredientChoppedStationComplete = true;
					audioSource.PlayOneShot(successSound);

					Player.currentIngred = FoodData.Instance.TryAdvanceIngredient(Player.currentIngred);
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

	private void KnifeMovement() {
    if (shouldShake) {
      float xTransform = -1 * Mathf.Sin((Time.time - lastChop) * shakeSpeed) * shakeAmount;

      if (negSinCount > 0 && posSinCount > 0 && xTransform < 0) {
        ResetKnifePosition();
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
		GameObject model = (GameObject) Resources.Load(Player.currentIngred.Model, typeof(GameObject));
		Transform modelTransform = model.GetComponentsInChildren<Transform>(true)[0];

		Quaternion modelRotation = modelTransform.rotation;
		Vector3 modelPosition = modelTransform.position;
		ingredientInstantiation = Instantiate(model, modelPosition, modelRotation);
	}

	private void DoSingleChop() {
    audioSource.PlayOneShot(chopSound);
    Player.currentIngred.numberOfChops++;
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
		player.notifyAboutStationLeft();
		SceneManager.LoadScene("PlayerMainScreen");
	}
}
