using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour {

    // Timer variable
    private float timer;
    private float initialTimer = 1200f;
    public Text timerText;
    public Manager manager;

    public bool isCountDown = false;
    public bool isStarted = false;

    public void Start() {
      timer = 1200.0f;
      manager = GameObject.Find("Manager").GetComponent<Manager>();
    }

    public void StartTimer() {
      timer = 1204.0f;
      isCountDown = true;
    }

  	// Update is called once per frame from the server's update method
  	void Update() {
      if (timer > 0 && isStarted) {
        timer -= Time.deltaTime;
        displayTime();
      } else if (isStarted && timer <= 0) {
        manager.GameOver();
      } else if (timer > 0 && isCountDown) {
        timer -= Time.deltaTime;
        timerText.text = ((int) (timer - initialTimer)).ToString();
      } else if (timer == initialTimer) {
        isCountDown = false;
        isStarted = true;
      }
  	}

    private void displayTime() {
      TimeSpan t = TimeSpan.FromSeconds(timer);
      string timerFormatted = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
      timerText.text = "Time left " + timerFormatted;
    }

    public float getTime() {
      return timer;
    }
}
