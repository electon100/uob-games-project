using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewGameTimer : MonoBehaviour {

  public Text timerText;

  private NewServer server;

  private readonly float initialTimer = 300.0f;
  private readonly float countdownDuration = 4.0f;
  private float timer = 0.0f;

  public bool isStarted = false, notifiedServerOfStart = false; /* Horrible flag variable but o well */

  public void Start() {
    server = GameObject.Find("Server").GetComponent<NewServer>();
    ResetTimer();
  }

  public void ResetTimer() {
    timer = initialTimer + countdownDuration;
    isStarted = false;
    notifiedServerOfStart = false;
  }

  public void StartTimer() {
    isStarted = true;
  }

  void Update() {
    if (isStarted) {
      timer -= Time.deltaTime;
    }

    if (timer <= 0) {
      server.OnGameOver();
      ResetTimer();
    }

    if (timer > initialTimer) {
      timerText.text = ((int) (timer - initialTimer) == 0) ? "GO!" : ((int) (timer - initialTimer)).ToString();
    } else {
      displayTime();
      if (!notifiedServerOfStart) {
        server.OnGameStart();
        notifiedServerOfStart = true;
      }
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
