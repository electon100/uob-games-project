using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text;

public class PlateBehaviour : MonoBehaviour {

  public Button serveBtn, throwBtn, clearBtn, goBackBtn;
  public Player player;

  /* Text representation of ingredients on Screen */
  public Text ingredientListText, statusText;

  /* Ingredient stuff */
	private readonly int maxPlateContents = 3;
  private List<Ingredient> plateContents = new List<Ingredient>();
  private List<GameObject> plateContentsObjects = new List<GameObject>();

  void Start () {

    Screen.orientation = ScreenOrientation.Portrait;
    statusText.enabled = false;

    clearPlate();

    player = GameObject.Find("Player").GetComponent<Player>();

    foreach (Ingredient ingredient in Player.ingredientsFromStation) {
      addIngredientToPlate(ingredient);
    }
  }

  void Update() {
    updateTextList();
    updateButtonStates();
    checkForPlateTap();

    /* Try and combine the ingredients */
    Ingredient combinationAttempt = getWorkingRecipe();

    /* If the combined result is a valid recipe (not mush) */
    if (isValidRecipe(combinationAttempt)) {
      /* Set the pan contents to the new combined recipe */
      clearStation();
      addIngredientToPlate(combinationAttempt);
      player.notifyServerAboutIngredientPlaced(combinationAttempt);
    }
  }

  private void addIngredientToPlate(Ingredient ingredient) {
    GameObject ingred = (GameObject) Resources.Load(ingredient.Model, typeof(GameObject));
    Transform ingredTransform = ingred.GetComponentsInChildren<Transform>(true)[0];
    Quaternion ingredRotation = ingredTransform.rotation;
    Vector3 ingredPosition = ingredTransform.position + new Vector3(Random.Range(-1, 1), Random.Range(-1, 1) + 17, 0);
    GameObject inst = Instantiate(ingred, ingredPosition, ingredRotation);

    plateContents.Add(ingredient);
    plateContentsObjects.Add(inst);
  }

  public void clearStation() {
    clearPlate();
    player.clearIngredientsInStation();
  }

  private void clearPlate() {
    foreach (GameObject go in plateContentsObjects) Destroy(go);

    plateContents.Clear();
    plateContentsObjects.Clear();
  }

  public void placeHeldIngredientInPlate() {
    /* Add ingredient */
    if (Player.isHoldingIngredient()) {
      if (plateContents.Count < maxPlateContents) {
        addIngredientToPlate(Player.currentIngred);

        /* Notify server that player has placed ingredient */
        player.notifyServerAboutIngredientPlaced(Player.currentIngred);

        Player.removeCurrentIngredient();
			} else {
        /* TODO: What happens plate is full */
			}
    } else {
      /* TODO: What happens when player is not holding an ingredient */
    }
  }

  void updateTextList() {
    ingredientListText.text = "Current Ingredients:\n";

    foreach(Ingredient ingredient in plateContents) {
      ingredientListText.text += ingredient.ToString() + "\n";
    }
  }

  private Ingredient getWorkingRecipe() {
    return FoodData.Instance.TryCombineIngredients(plateContents);
  }

  private bool isValidRecipe(Ingredient recipe) {
    return !string.Equals(recipe.Name, "mush");
  }

  public void serveFood() {
    if (plateContents.Count == 1) {
      Ingredient recipe = getWorkingRecipe();

      foreach (Ingredient ingredient in plateContents) {
        recipe = ingredient;
      }

      if (isValidRecipe(recipe)) {
        player.sendScoreToServer(recipe);
        clearStation();
      }
    }
  }

  public void throwFood() {
    if (plateContents.Count == 1) {
      Ingredient recipe = getWorkingRecipe();

      foreach (Ingredient ingredient in plateContents) {
        recipe = ingredient;
      }

      if (isValidRecipe(recipe)) {
        player.sendThrowToServer(recipe);
        clearStation();
        statusText.enabled = true;
      }
    }
  }

	public void pickUpIngredient() {
		if (plateContents.Count == 1) {
			/* Set the players current ingredient to the pan contents */
			foreach (Ingredient ingredient in plateContents) {
				Player.currentIngred = ingredient;
			}

			/* Clear the station */
			clearStation();
		} else {
			/* What to do if there are more than (or fewer than) 1 ingredients in the plate*/
		}
	}

  private void checkForPlateTap() {
		/* https://stackoverflow.com/a/38566276 */
		bool isDesktop = Input.GetMouseButtonDown(0);
		bool isMobile = (Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began);
		if (isDesktop || isMobile) {
			Ray raycast = (isDesktop) ? Camera.main.ScreenPointToRay(Input.mousePosition) :
																	Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
			RaycastHit raycastHit;
			if (Physics.Raycast(raycast, out raycastHit)) {
				if (!raycastHit.collider.name.Equals("Background")) { // <-- Requires ingredient prefabs to have colliders (approx) within plate bounds
				// if (raycastHit.collider.name.Equals("Plate")) { // <-- Requires ingredient prefabs not to have colliders!
					/* Plate was tapped! */
					if (canPlaceHeldIngredient()) {
						placeHeldIngredientInPlate();
					} else if (plateContents.Count == 1) {
						pickUpIngredient();
					}
				}
			}
		}
	}

  private bool canPlaceHeldIngredient() {
		return Player.isHoldingIngredient() && plateContents.Count < maxPlateContents;
	}

  public void goBack() {
    /* Notify server that player has left the station */
    Handheld.Vibrate();
    player.notifyAboutStationLeft();
    SceneManager.LoadScene("PlayerMainScreen");
  }

  private void updateButtonStates() {
		setButtonInteractable(clearBtn, plateContents.Count > 0);
		setButtonInteractable(serveBtn, plateContents.Count == 1);
    setButtonInteractable(throwBtn, plateContents.Count == 1);
	}

	private void setButtonInteractable(Button btn, bool interactable) {
		btn.interactable = interactable;
	}
}
