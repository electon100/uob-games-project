using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour {

    private int numberOfPlayers;
    private int countDown;
    private float startTime;
    public Transform startCanvas;
    public Transform countDownCanvas;
    public Text countDownText;

    private GameObject networkClient;
    private Client network;

    void Start() {
        Screen.orientation = ScreenOrientation.Portrait;
        numberOfPlayers = 0;
        startTime = Time.time;
        countDown = 0;

        networkClient = GameObject.Find("Client");
        network = networkClient.GetComponent<Client>();
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

    void StartGame()
    {
        countDownText.text = "Go, go, go!";
        SceneManager.LoadScene("PlayerMainScreen");
    }

    public void OnTwo() {
        numberOfPlayers = 2;
    }

    public void OnThree() {
        numberOfPlayers = 3;
    }

    public void OnFour() {
        numberOfPlayers = 4;
    }
}