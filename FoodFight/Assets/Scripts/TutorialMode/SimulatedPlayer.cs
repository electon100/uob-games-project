using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SimulatedPlayer : MonoBehaviour {

  private GameObject networkClient;
  private SimulatedClient network;

	public GameObject fadeBackground, infoPanel, mainModeButton, player;
	private ClientGameState currentGameState;

  public static Ingredient currentIngred;
  public static List<Ingredient> ingredientsInFrying = new List<Ingredient>();
	public static List<Ingredient> ingredientsInPlating = new List<Ingredient>();
	
	// Use this for initialization
	void Start () {
		Screen.orientation = ScreenOrientation.Portrait;
		DontDestroyOnLoad(GameObject.Find("SimulatedPlayer"));
		currentGameState = ClientGameState.TutorialMode;
		fadeBackground.SetActive(true);
		infoPanel.SetActive(true);
		Destroy(GameObject.Find("Player"));
	}
	
	void Update () {
		/* Check what step of the tutorial the player is at. */
		string currentScene = SceneManager.GetActiveScene().name;
		if ((currentGameState != Client.gameState) && (currentScene == "PlayerMainScreen")) {
			switch(Client.gameState) {
				case ClientGameState.TutorialMode:
					break;
				case ClientGameState.CupboardTutorial:
					fadeBackground = GameObject.Find("FadeBackgroundImage");
					infoPanel = GameObject.Find("InfoPanel");
					break;
				case ClientGameState.ChoppingTutorial:
					fadeBackground = GameObject.Find("FadeBackgroundImage");
					infoPanel = GameObject.Find("InfoPanel");
					GameObject.Find("InfoText").GetComponent<Text>().text = "Now log into the chopping station \n to chop the ingredient.";
					break;
				case ClientGameState.FryingTutorial:
					fadeBackground = GameObject.Find("FadeBackgroundImage");
					infoPanel = GameObject.Find("InfoPanel");
					GameObject.Find("InfoText").GetComponent<Text>().text = "Now log into the frying station \n to fry the ingredient.";
					break;
				case ClientGameState.PlatingTutorial:
					fadeBackground = GameObject.Find("FadeBackgroundImage");
					infoPanel = GameObject.Find("InfoPanel");
					GameObject.Find("InfoText").GetComponent<Text>().text = "Now log into the plating station \n to serve the food.";
					break;
				case ClientGameState.EndTutorial:
					fadeBackground = GameObject.Find("FadeBackgroundImage");
					infoPanel = GameObject.Find("InfoPanel");
					GameObject.Find("InfoText").GetComponent<Text>().text = "Tutorial completed! \n You got 70 points!";
					GameObject.Find("MyScore").GetComponent<Text>().text = "Your team: \n 70";
					break;
				default:
					break;
			}
			currentGameState = Client.gameState;
		}
	}

	/* On click of the Got It button */
  public void GotIt() {
    GameObject.Find("FadeBackgroundImage").SetActive(false);
    GameObject.Find("InfoPanel").SetActive(false);
		if (Client.gameState.Equals(ClientGameState.EndTutorial)) {
			mainModeButton.SetActive(true);
			fadeBackground.SetActive(true);
		}
  }

	/* Checks if the player is currently holding anything */
  public static bool isHoldingIngredient() {
    return currentIngred != null;
  }

  /* Resets the player's current ingredient */
  public static void removeCurrentIngredient() {
    currentIngred = null;
  }

	public void GoBackToMainMode() {
		if (Client.gameState.Equals(ClientGameState.EndTutorial)) {
			Client.gameState = ClientGameState.MainMode;
			Destroy(GameObject.Find("SimulatedClient"));
			SceneManager.LoadScene("PlayerStartScreen");
		}
	}

	/* Ignore the code below, I forgot to get nfc-s so had to create buttons for stations - xoxo, Sisi */
  public void goToCupboard() {
    SimulatedClient.LogInStation("0");
  }

  public void goToChopping() {
    SimulatedClient.LogInStation("1");
  }

  public void goToFrying() {
    SimulatedClient.LogInStation("2");
  }

  public void goToPlating() {
    SimulatedClient.LogInStation("3");
  }
}
