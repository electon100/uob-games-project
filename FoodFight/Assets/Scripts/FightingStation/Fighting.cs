using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text;

public class Fighting : MonoBehaviour {

  public Button fireBtn, putBtn, clearBtn, goBackBtn;
  public Player player;

  /* Ingredient stuff */
  private Ingredient throwIngredient;
  private GameObject throwIngredientGameObject;

  void Start () {
    Screen.orientation = ScreenOrientation.Portrait;
    clearPlate();
    player = GameObject.Find("Player").GetComponent<Player>();
    foreach (Ingredient ingredient in Player.ingredientsFromStation) {
      addIngredientToThrow(ingredient);
    }
  }

  void Update() {
    updateButtonStates();
  }

  private void addIngredientToThrow(Ingredient ingredient) {
    GameObject ingred = (GameObject) Resources.Load(ingredient.Model, typeof(GameObject));
    Transform ingredTransform = ingred.GetComponentsInChildren<Transform>(true)[0];
    Quaternion ingredRotation = ingredTransform.rotation;
    Vector3 ingredPosition = ingredTransform.position + new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), 0);
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
