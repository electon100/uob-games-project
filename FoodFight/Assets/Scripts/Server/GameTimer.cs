using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour {

    // Timer variable
    private float timer;
    public Text timerText;
    public Manager manager;

    bool isStarted = false;

    public void Start() {
      timer = 1200.0f;
      isStarted = true;
      manager = GameObject.Find("Manager").GetComponent<Manager>();
    }

  	// Update is called once per frame from the server's update method
  	void Update() {
      if (timer > 0) {
        timer -= Time.deltaTime;
        displayTime();
      } else {
        manager.GameOver();
      }
  	}

    private void displayTime() {
      TimeSpan t = TimeSpan.FromSeconds(timer);
      string timerFormatted = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
      timerText.text = timerFormatted;
    }

    public float getTime() {
      return timer;
    }
}
