using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewGameTimer : MonoBehaviour {

  private GameObject timerTextObject;
  private RectTransform timerTransform;
  private Text timerText;

  private GameObject timerSignObject;
  private RectTransform timerSignTransform;
  private Image timerSignImage;

  private NewServer server;

  private readonly float initialTimer = 360.0f;
  private readonly float countdownDuration = 4.0f;
  private float timer = 0.0f;

  public bool isStarted = false, notifiedServerOfStart = false; /* Horrible flag variable but o well */

  public void Start() {
    server = GameObject.Find("Server").GetComponent<NewServer>();

    ResetTimer();
    initialiseUI();
  }

  public void ResetTimer() {
    timer = initialTimer + countdownDuration;
    isStarted = false;
    notifiedServerOfStart = false;
  }

  public void StartTimer() {
    isStarted = true;
  }

  void initialiseUI() {
    timerSignObject = new GameObject("GameTimerSignObject", typeof(RectTransform));
    timerSignObject.transform.SetParent(GameObject.Find("Timer").transform);

    // Sign position
    timerSignTransform = timerSignObject.GetComponent<RectTransform>();

    // Sign Image
    timerSignImage = timerSignObject.AddComponent<Image>();
    timerSignImage.sprite = Resources.Load("Timer Sign", typeof(Sprite)) as Sprite;

    timerTextObject = new GameObject("GameTimerObject", typeof(RectTransform));
    timerTextObject.transform.SetParent(GameObject.Find("Timer").transform);

    // Text position
    timerTransform = timerTextObject.GetComponent<RectTransform>();

    // Timer Text
    timerText = timerTextObject.AddComponent<Text>();
    timerText.fontSize = 100;
    timerText.color = Color.black;
    timerText.font = Resources.Load("Acids!", typeof(Font)) as Font;//Assets/Resources/Acids!.otf
    timerText.alignment = TextAnchor.MiddleCenter;
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

    if (timer <= 30.0f) {
      timerText.color = Color.red;
    }

    updateUI();
  }

  void updateUI() {
    int width;
    int height;

    width = (int) (Screen.width / 6);
    height = (int) (width / 1.5f);

    timerText.fontSize = (int) (height / 2);

    timerTransform.sizeDelta = new Vector2(width, height);
    timerSignTransform.sizeDelta = new Vector2(width, height);

    timerSignTransform.localPosition = new Vector3(0, (int) (Screen.height / 2 - height / 2), 0);
    timerTransform.localPosition = new Vector3(0, (int) (Screen.height / 2 - height / 2 - height / 4), 0);
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
