using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text;

public class Fighting : MonoBehaviour {

  public Button fireBtn, putBtn, clearBtn, goBackBtn;
  public GameObject grabText;
  public Player player;

  /* Ingredient stuff */
  private Ingredient throwIngredient;
  private GameObject throwIngredientGameObject;

  private Vector3 startPosition;
  private Vector3 startCatapultPosition;
  public GameObject catapult;

  private bool startedThrowing = false;

  void Start () {
    Screen.orientation = ScreenOrientation.Portrait;
    clearPlate();
    player = GameObject.Find("Player").GetComponent<Player>();
    foreach (Ingredient ingredient in Player.ingredientsFromStation) {
      addIngredientToThrow(ingredient);
    }
    startPosition = throwIngredientGameObject.GetComponent<Transform>().position;
    startCatapultPosition = catapult.GetComponent<Transform>().position;
  }

  void Update() {
    updateButtonStates();
    if (startedThrowing) {
      throwingAnimation();
    }
  }

  private void addIngredientToThrow(Ingredient ingredient) {
    GameObject ingred = (GameObject) Resources.Load(ingredient.Model, typeof(GameObject));
    Transform ingredTransform = ingred.GetComponentsInChildren<Transform>(true)[0];
    Quaternion ingredRotation = Quaternion.Euler(-25, ingredTransform.rotation.y, ingredTransform.rotation.z);
    Vector3 ingredPosition = new Vector3(0, -19, -60);
    GameObject inst = Instantiate(ingred, ingredPosition, ingredRotation);

    throwIngredient = ingredient;
    throwIngredientGameObject = inst;
  }

  public void clearStation() {
    clearPlate();
    player.clearIngredientsInStation();
  }

  private void clearPlate() {
    if (throwIngredientGameObject != null) Destroy(throwIngredientGameObject);
    throwIngredient = null;
  }

  public void placeHeldIngredient() {
    /* Add ingredient */
    if (Player.isHoldingIngredient()) {
      addIngredientToThrow(Player.currentIngred);
      player.notifyServerAboutIngredientPlaced(Player.currentIngred);
      Player.removeCurrentIngredient();
    } else {
      /* TODO: What happens when player is not holding an ingredient */
    }
  }

  public void throwFood() {
    if (throwIngredient != null) {
      startedThrowing = true;
      grabText.gameObject.SetActive(true);
      player.sendThrowToServer(throwIngredient);
    } else {
      /* TODO: What happens when plate is empty */
    }
  }

  public void throwingAnimation() {
    throwIngredientGameObject.GetComponent<Transform>().position = startPosition + new Vector3(0.0f, (Time.time)*5.0f, Mathf.Sin(Time.time)*5.0f);
    startPosition = throwIngredientGameObject.GetComponent<Transform>().position;
    if (catapult.GetComponent<Transform>().rotation.x < 0.5f) {
      catapult.GetComponent<Transform>().Rotate(new Vector3((Time.time)*5.0f, 0.0f, 0.0f));
    }
  }

  public void goBack() {
    /* Notify server that player has left the station */
    Handheld.Vibrate();
    player.notifyAboutStationLeft();
    SceneManager.LoadScene("PlayerMainScreen");
  }

  private void updateButtonStates() {
		setButtonInteractable(putBtn, Player.isHoldingIngredient() && throwIngredient == null);
		setButtonInteractable(clearBtn, throwIngredient != null);
		setButtonInteractable(fireBtn, throwIngredient != null);
	}

	private void setButtonInteractable(Button btn, bool interactable) {
		btn.interactable = interactable;
	}
}
