using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour {

    // Timer variable
    private float timer;
    public Text timerText;

    bool isStarted = false;

    public void Start() {
      timer = 1200.0f;
      isStarted = true;
    }

  	// Update is called once per frame from the server's update method
  	public void updateTimer() {
          if (!isStarted) return;
          timer -= Time.deltaTime;
          displayTime();
  	}

    private void displayTime()
    {
        TimeSpan t = TimeSpan.FromSeconds(timer);
        string timerFormatted = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        timerText.text = "Time left " + timerFormatted;
    }

    public float getTime()
    {
        return timer;
    }
}
