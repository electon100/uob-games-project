using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Chopping : MonoBehaviour {

  /* Initialise different screens. */
  public Transform defaultCanvas;
  public Transform startCanvas;
  public Transform warningCanvas;
  public Transform notChoppableCanvas;
  public Transform endCanvas;

  /* Output text to be displayed on screen */
  public Text chops;
  public Text outCome;
  public Text status;
  public Text notChoppable;
  public Text instructions;

  /* Sounds for up and down acceleration */
  public AudioClip chopSound;
  public AudioClip slashSound;
  public AudioClip successSound;
  private AudioSource source;

  /* Highest acceleration recorded so far */
  private float maxAcc = 0.5f;
  private float minimumChopInterval = 0.3f; // (seconds)

  /* Movement stuff */
  private float shakeSpeed = 15.0f; // Speed of pan shake
  private float shakeAmount = 2f; // Amplitude of pan shake
  private bool shouldShake = false;
  private int negSinCount = 0, posSinCount = 0;
  private Vector3 originalPos;
  private float lastChop;
  private float yTransform;

  /* Private fields necessary for chopping status and sound effects */
  private bool startChopping;
  private bool isChopped = false;
  private bool hasStartedChopping = false;
  private Player player;
  private Ingredient currentChoppingIngred;

  private void Start() {
    if (Client.gameState.Equals(ClientGameState.MainMode)) {
      /* Instantiates the player to access functions and sets the
      current chopping ingredient to whatever the Player's currently holding. */
      player = GameObject.Find("Player").GetComponent<Player>();

      /* Set the ingredient the player is currently holding */
      currentChoppingIngred = Player.currentIngred;
    } else {
      currentChoppingIngred = SimulatedPlayer.currentIngred;
    }

    /* Set up scene */
    source = GetComponent<AudioSource>();
    //Screen.orientation = ScreenOrientation.LandscapeLeft;
    Screen.orientation = ScreenOrientation.Portrait;
    originalPos = gameObject.transform.position;

    /* Initialise private variables */
    lastChop = Time.time;
    startChopping = CheckIngredientValid();
    isChopped = false;
    hasStartedChopping = false;

    /* Start the game if possible */
    StartGame();
  }

  void Update() {
    if (startChopping) {
      /* Constantly checks if ingredient is correctly chopped and displays success screen. */
      ChoppingStatus();

      /* Getting rid of the intro screen when player starts chopping. */
      if (Input.acceleration.y > maxAcc) {
        maxAcc = Input.acceleration.y;
      }

      if (Input.GetKeyDown(KeyCode.UpArrow)) {
        transform.Rotate(0, 10, 0);
      }

      /* Updates the chops count on the screen. */
      chops.text = currentChoppingIngred.numberOfChops.ToString();
      /* Simulates the movement of the knife. */
      KnifeMovement();

      /* Uncomment to make game more interesting and add sliced fingers. */
      // CheckChopSpeed();

      if (DetectChop()) {
        /* Chop detected! */
        DoChop();
      }

      /* For desktop tests. */
      if (Input.GetKeyDown(KeyCode.DownArrow)) {
        DoChop();
      }

      if ((currentChoppingIngred.numberOfChops % 3) == 0) {
        transform.position = originalPos;
      }
    }
  }

  public bool DetectChop() {
    return !isChopped && (Time.time - lastChop) > minimumChopInterval && Input.acceleration.y > 2.0f;
  }

  public void StartGame() {
    if (startChopping) {
      startCanvas.gameObject.SetActive(false);
      defaultCanvas.gameObject.SetActive(true);
      /* Instantiate all text info */
      outCome = GameObject.Find("OutcomeText").GetComponent<Text>();
      chops = GameObject.Find("ChopText").GetComponent<Text>();
      status = GameObject.Find("StatusText").GetComponent<Text>();

      GameObject model = (GameObject) Resources.Load(currentChoppingIngred.Model, typeof(GameObject));
      Transform modelTransform = model.GetComponentsInChildren<Transform>(true)[0];

      Quaternion modelRotation = modelTransform.rotation;
      Vector3 modelPosition = modelTransform.position;
      GameObject inst = Instantiate(model, modelPosition, modelRotation);
    }
  }

  private void DoChop() {
    source.PlayOneShot(chopSound);
    currentChoppingIngred.numberOfChops++;
    lastChop = Time.time;
    hasStartedChopping = true;
    shouldShake = true;
  }

  /* Checks the chopping speed and displays a red sign if it's too fast */
  void CheckChopSpeed() {
    if (maxAcc > 5.0f || maxAcc < -5.0f) {
      defaultCanvas.gameObject.SetActive(false);
      warningCanvas.gameObject.SetActive(true);
    } else if (maxAcc < 3.5f && maxAcc > -3.5f) {
      outCome.text = "CHOP HARDER!";
    } else {
      outCome.text = " ";
    }
  }

  void KnifeMovement() {
    if (shouldShake) {
      float xTransform = -1 * Mathf.Sin((Time.time - lastChop) * shakeSpeed) * shakeAmount;

      if (negSinCount > 0 && posSinCount > 0 && xTransform < 0) {
        gameObject.transform.position = originalPos;
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

  void ChoppingStatus() {
    /* Checks if the ingredient has been chopped the right amount of times and updates it */
    if (FoodData.Instance.isChopped(currentChoppingIngred)) {
      isChopped = true;
      source.PlayOneShot(successSound);
      defaultCanvas.gameObject.SetActive(false);
      endCanvas.gameObject.SetActive(true);

      /* Sets the player's current ingredient based on the mode */
      if (Client.gameState.Equals(ClientGameState.MainMode)) {
        Player.currentIngred = FoodData.Instance.TryAdvanceIngredient(currentChoppingIngred);
      } else {
        SimulatedPlayer.currentIngred = FoodData.Instance.TryAdvanceIngredient(currentChoppingIngred);
      }
    }
    /* Checks if player has start chopping and instructs them to do so if not */
    if (hasStartedChopping) instructions.gameObject.SetActive(false);
  }

  private bool CheckIngredientValid() {
    if (Player.isHoldingIngredient()) {
      /* Stops the minigame if ingredient cannot be chopped */
      if (FoodData.Instance.isChoppable(currentChoppingIngred)) {
        return true;
      } else {
        defaultCanvas.gameObject.SetActive(false);
        notChoppableCanvas.gameObject.SetActive(true);
        notChoppable.text = "Ingredient cannot be chopped";
        return false;
      }
    } else if (SimulatedPlayer.isHoldingIngredient()) {
      /* Stops the minigame if ingredient cannot be chopped */
      if (FoodData.Instance.isChoppable(currentChoppingIngred)) {
        return true;
      } else {
        defaultCanvas.gameObject.SetActive(false);
        notChoppableCanvas.gameObject.SetActive(true);
        notChoppable.text = "Ingredient cannot be chopped";
        return false;
      }
    } else {
      defaultCanvas.gameObject.SetActive(false);
      notChoppableCanvas.gameObject.SetActive(true);
      notChoppable.text = "You are not holding any ingredients";
    }
    return false;
  }

  /* Sends the chopped ingredient to server and returns to the main screen */
  public void goBack() {
    /* Notify server that player has left the station */
    Handheld.Vibrate();
    if (Client.gameState.Equals(ClientGameState.MainMode)) {
      player.notifyAboutStationLeft();
    } else if (currentChoppingIngred != null) { /* Set the mode to the next step of the tutorial */
      Client.gameState = ClientGameState.FryingTutorial;
    }
    SceneManager.LoadScene("PlayerMainScreen");
  }

  /* Returns player back to the chopping, after failing the first time */
  public void tryAgain() {
    warningCanvas.gameObject.SetActive(false);
    defaultCanvas.gameObject.SetActive(true);
  }

}
