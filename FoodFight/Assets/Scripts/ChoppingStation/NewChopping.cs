using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text;

public class NewChopping : MonoBehaviour {

	/* Scene stuff */
	public Button goBackBtn, clearBtn;
	public GameObject confirmationCanvas;
	public Text statusText;
	public Text infoText;
	public Material successMaterial;
	public Material neutralMaterial;
	public Material issueMaterial;
	public Renderer background;
	public GameObject infoPanel;
	public GameObject fadeBackground;
	public GameObject tapAnimation;
	public GameObject choppingImage;
	public GameObject backArrow;

	/* Sound stuff */
	public AudioClip chopSound;
  public AudioClip successSound;
  private AudioSource audioSource;

	private Player player;
	private GameObject ingredientInstantiation;
	private Ingredient ingredientToChop;
	private bool ingredientChoppedStationComplete = false;

	private readonly float minimumChopInterval = 0.25f; // seconds
	private readonly float minimumAcceleration = 0.15f;

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
			if (Player.ingredientsFromStation.Count > 0) SetIngredientToChop(Player.ingredientsFromStation[0]);
		} else {
			SetIngredientToChop(SimulatedPlayer.ingredientInChopping);
		}

		tapAnimation.SetActive(!Client.gameState.Equals(ClientGameState.MainMode) && ingredientToChop == null);
		infoPanel.SetActive(!Client.gameState.Equals(ClientGameState.MainMode) && ingredientToChop != null);
		fadeBackground.SetActive(!Client.gameState.Equals(ClientGameState.MainMode) && ingredientToChop != null);

		originalPos = gameObject.transform.position;
		lastChop = Time.time;
	}

	void Update () {

		updateButtonStates();
		KnifeMovement();
		checkForBoardTap();

		if (ingredientChoppedStationComplete) {
			ChangeView("Ingredient chopped", successMaterial);
		} else if (ingredientToChop != null) {
			if (FoodData.Instance.isChoppable(ingredientToChop)) {

				if (FoodData.Instance.isChopped(ingredientToChop)) {
					ingredientChoppedStationComplete = true;
					audioSource.PlayOneShot(successSound);

					Ingredient choppedIngredient = FoodData.Instance.TryAdvanceIngredient(ingredientToChop);
					SetIngredientToChop(choppedIngredient);
					tapAnimation.SetActive(Client.gameState.Equals(ClientGameState.ChoppingTutorial));
					return;
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
			ChangeView("Place ingredient on board to start", neutralMaterial);
		}

	}

	private Ingredient GetPlayerIngredient() {
		/* Check if game is running in tutorial mode */
		if (Client.gameState.Equals(ClientGameState.MainMode)) {
			return Player.currentIngred;
		} else {
			return SimulatedPlayer.currentIngred;
		}
	}

	private void SetPlayerIngredient(Ingredient ingredient) {
		/* Check if game is running in tutorial mode */
		if (Client.gameState.Equals(ClientGameState.MainMode)) {
			Player.currentIngred = ingredient;
		} else {
			SimulatedPlayer.currentIngred = ingredient;
		}
	}

	private void SetIngredientToChop(Ingredient ingredient) {
		clearStation();
		if (Client.gameState.Equals(ClientGameState.MainMode)) {
			player.notifyServerAboutIngredientPlaced(ingredient);
		} else {
			SimulatedPlayer.ingredientInChopping = ingredient;
		}
		ingredientToChop = ingredient;
		if (ingredient != null) LoadIngredient(ingredient);
	}

	private bool canPlaceHeldIngredient() {
		return !ingredientChoppedStationComplete && isPlayerHoldingIngredient() && ingredientToChop == null;
	}

	private void placeHeldIngredientOnBoard() {
		if (Client.gameState.Equals(ClientGameState.MainMode)) { /* Main mode */
			if (Player.isHoldingIngredient()) { /* Main mode */
				if (ingredientToChop == null) {
					SetIngredientToChop(Player.currentIngred);
					Player.removeCurrentIngredient();
				} else {
					Debug.Log("Already an item on the chopping board");
				}
			} else {
				Debug.Log("No held item to add to board");
			}
		} else { /* Tutorial mode */
			if (SimulatedPlayer.isHoldingIngredient()) {
				if (ingredientToChop == null) {
					SetIngredientToChop(SimulatedPlayer.currentIngred);
					SimulatedPlayer.ingredientInChopping = SimulatedPlayer.currentIngred;
					SimulatedPlayer.removeCurrentIngredient();
					tapAnimation.SetActive(ingredientToChop == null);
					infoPanel.SetActive(ingredientToChop != null);
					fadeBackground.SetActive(ingredientToChop != null);
				} else {
					Debug.Log("Already an item on the chopping board");
				}
			} else {
				Debug.Log("No held item to add to board");
			}
		}
	}

	private void pickUpIngredient() {
		if (ingredientToChop != null) {
			if (Client.gameState.Equals(ClientGameState.MainMode)) { /* Main mode */
				/* Set the players current ingredient to the pan contents */
				Player.currentIngred = ingredientToChop;
			} else { /* Tutorial mode */
				/* Set the players current ingredient to the pan contents */
				SimulatedPlayer.currentIngred = ingredientToChop;
				SimulatedPlayer.ingredientInChopping = null;
				tapAnimation.SetActive(!ingredientChoppedStationComplete);
				backArrow.SetActive(ingredientChoppedStationComplete);
			}
			/* Clear the station */
			clearStation();
		} else {
			/* What to do if there are more than (or fewer than) 1 ingredients in the pan*/
			Debug.Log("No ingredient to pick up");
		}
	}

	private void clearStation() {
		if (ingredientInstantiation != null) Destroy(ingredientInstantiation);
		ingredientToChop = null;
		if (Client.gameState.Equals(ClientGameState.MainMode)) {
			player.clearIngredientsInStation();
		} else {
			SimulatedPlayer.ingredientInChopping = null;
		}
	}

	private void checkForBoardTap() {
		/* https://stackoverflow.com/a/38566276 */
		bool isDesktop = Input.GetMouseButtonDown(0);
		bool isMobile = (Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began);
		if (isDesktop || isMobile) {
			Ray raycast = (isDesktop) ? Camera.main.ScreenPointToRay(Input.mousePosition) :
																	Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
			RaycastHit raycastHit;
			if (Physics.Raycast(raycast, out raycastHit)) {
				if (!raycastHit.collider.name.Equals("Background") && !(infoPanel.active) && !(confirmationCanvas.active)) { // <-- Requires ingredient prefabs to have colliders (approx) within board bounds
					/* Board was tapped! */
					if (canPlaceHeldIngredient()) {
						placeHeldIngredientOnBoard();
					} else if (ingredientToChop != null) {
						pickUpIngredient();
						/* TODO status text */
					}
				}
			}
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

	private bool isPlayerHoldingIngredient() {
		return GetPlayerIngredient() != null;
	}

	private void ResetKnifePosition() {
		gameObject.transform.position = originalPos;
	}

	private void LoadIngredient(Ingredient ingredient) {
		if (ingredientInstantiation != null) Destroy(ingredientInstantiation);
		GameObject model = (GameObject) Resources.Load(ingredient.Model, typeof(GameObject));
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
    return (Time.time - lastChop) > minimumChopInterval && Input.acceleration.y > minimumAcceleration;
  }

	private void ChangeView(string message, Material material) {
		statusText.text = message;
		background.material = material;
	}

	private void updateButtonStates() {
		setButtonInteractable(clearBtn, ingredientToChop != null);
	}

	private void setButtonInteractable(Button btn, bool interactable) {
		btn.interactable = interactable;
	}

	public void OnGoBack() {
		goBack();
	}

	public void OnConfirmClear() {
		if (Client.gameState.Equals(ClientGameState.MainMode)) confirmationCanvas.SetActive(true);
	}

	public void OnConfirmNo() {
		confirmationCanvas.SetActive(false);
	}

	public void OnConfirmYes() {
		confirmationCanvas.SetActive(false);
		clearStation();
	}

	private void goBack() {
		/* Notify server that player has left the station */
		Handheld.Vibrate();
		if (Client.gameState.Equals(ClientGameState.MainMode)) {
			player.notifyAboutStationLeft();
			SceneManager.LoadScene("PlayerMainScreen");
		} else {
			if (ingredientChoppedStationComplete) {
				if (ingredientToChop != null) {
					infoPanel.SetActive(true);
					fadeBackground.SetActive(true);
					choppingImage.SetActive(false);
					tapAnimation.SetActive(true);
					infoText.text = "Oops! \n You forgot to pick up \n the ingredient!";
				} else {
					Client.gameState = ClientGameState.FryingTutorial;
					SceneManager.LoadScene("PlayerMainScreen");
				}
			}
		}
	}

	public void GotIt() {
		infoPanel.SetActive(false);
		fadeBackground.SetActive(false);
	}
}
