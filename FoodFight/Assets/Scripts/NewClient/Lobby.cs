using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour {

    private int countDown;
    private float startTime;
    public Transform startCanvas;
    public Transform countDownCanvas;
    public Text countDownText, lobbyText;
    public Material redBackground, blueBackground;
	public Renderer background;

    private GameObject networkClient;
    private Client network;

    void Start() {
        Screen.orientation = ScreenOrientation.Portrait;
        startTime = Time.time;
        countDown = 0;

        networkClient = GameObject.Find("Client");
        network = networkClient.GetComponent<Client>();

        lobbyText.text = network.getTeam() + " team lobby";

        if (network.getTeam().Equals("blue")) {
            background.material = blueBackground;
        } else if (network.getTeam().Equals("red")) {
            background.material = redBackground;
        }
    }

    void Update() {
        if (network.startGame) {
            startCanvas.gameObject.SetActive(false);
            countDownCanvas.gameObject.SetActive(true);
            if ((Time.time - startTime) > 1.0f) {
                countDown += 1;
                startTime = Time.time;
            }
            if (countDown == 3) {
                countDownText.text = "Go, go, go!";
            } else if (countDown == 4) {
                StartGame();
            } else {
                countDownText.text = (3 - countDown).ToString();
            }
        }
    }

    public void startGame() {
        SceneManager.LoadScene("MainScreen");
    }

    void StartGame() {
        SceneManager.LoadScene("PlayerMainScreen");
    }

}