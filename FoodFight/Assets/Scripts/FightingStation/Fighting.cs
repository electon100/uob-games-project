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
    startCatapultPosition = new Vector3(48.98819f, 11.10445f, -8.887868f);
  }

  void Update() {
    updateButtonStates();
  }

  private void addIngredientToThrow(Ingredient ingredient) {
    GameObject ingred = (GameObject) Resources.Load(ingredient.Model, typeof(GameObject));
    Transform ingredTransform = ingred.GetComponentsInChildren<Transform>(true)[0];
    Quaternion ingredRotation = Quaternion.Euler(-25, ingredTransform.rotation.y, ingredTransform.rotation.z);
    Vector3 ingredPosition = new Vector3(0, -19, -60);
    GameObject inst = Instantiate(ingred, ingredPosition, ingredRotation);

    throwIngredient = ingredient;
    throwIngredientGameObject = inst;

    startPosition = throwIngredientGameObject.GetComponent<Transform>().position;
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
      clearStation();
    } else {
      /* TODO: What happens when plate is empty */
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
