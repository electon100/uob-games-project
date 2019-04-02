using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulatedPlayer : MonoBehaviour {

  private GameObject networkClient;
  private SimulatedClient network;

	public GameObject fadeBackground, infoPanel;
	private ClientGameState currentGameState;

  public static Ingredient currentIngred;
  public static List<Ingredient> ingredientsFromStation;
	
	// Use this for initialization
	void Start () {
		Screen.orientation = ScreenOrientation.Portrait;
		DontDestroyOnLoad(GameObject.Find("SimulatedPlayer"));
		Destroy(GameObject.Find("Player"));
		currentGameState = ClientGameState.TutorialMode;
		fadeBackground.SetActive(true);
		infoPanel.SetActive(true);
	}
	
	void Update () {

		/* Check what step of the tutorial the player is at. */
		if (currentGameState != Client.gameState) {
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
  }

	/* Checks if the player is currently holding anything */
  public static bool isHoldingIngredient() {
    return currentIngred != null;
  }

}
