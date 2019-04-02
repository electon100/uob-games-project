using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SimulatedClient : MonoBehaviour {

  private NFCHandler nfcHandler = new NFCHandler();

  /* Current station:
  -1 - Main Screen
  0  - Cupboard
  1  - Chopping
  2  - Frying
  3  - Plating */
  public static string currentStation = "-1";

  public Material redBackground, blueBackground;
	public Renderer background;
  public Text myScoreText, otherScoreText;


	void Start () {
		Screen.orientation = ScreenOrientation.Portrait;
    Client.gameState = ClientGameState.CupboardTutorial;
	}
	
	void Update () {

    /* For desktop testing */
    if (Input.GetKeyDown(KeyCode.R)) LogInStation("0");
    if (Input.GetKeyDown(KeyCode.T)) LogInStation("1");
    if (Input.GetKeyDown(KeyCode.Y)) LogInStation("2");
    if (Input.GetKeyDown(KeyCode.U)) LogInStation("3");
    if (Input.GetKeyDown(KeyCode.E)) {
			foreach(Ingredient ingredient in SimulatedPlayer.ingredientsInFrying) {
				Debug.Log(ingredient.Name);
			}
		} 
			

    /* Check for any NFC scans, forwarding to checkStation if present */
    string lastTag = nfcHandler.getScannedTag();
    if (lastTag != "-1" && currentStation == "-1") {
      Handheld.Vibrate();
      LogInStation(lastTag);
    }
	}

  public static void LogInStation(string stationId) {
    string currentScene = SceneManager.GetActiveScene().name;

		Debug.Log(currentScene);

		switch(stationId) {
			case "0": // Cupboard Minigame
				if (!currentScene.Equals("CupboardStation")) {
					SceneManager.LoadScene("CupboardStation");
				}
				break;
			case "1": // Chopping Minigame
				if (!currentScene.Equals("ChoppingStation")) {
					SceneManager.LoadScene("ChoppingStation");
				}
				break;
			case "2": // Frying Minigame
				if (!currentScene.Equals("FryingStation")) {
					SceneManager.LoadScene("FryingStation");
				}
				break;
			case "3": // Plating Minigame
				if (!currentScene.Equals("PlatingStation")) {
					SceneManager.LoadScene("PlatingStation");
				}
				break;
			default:
					break;
		}
  }

}
