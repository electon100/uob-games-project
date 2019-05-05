using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Order {

	public string ID { get; }
	public Ingredient Recipe { get; set; }
	public GameObject ParentGameObject { get; set; }
	public float Timer { get; set; }
	public string Team { get; set; }

	private Canvas canvas;
	private Transform orderPanel;

	private GameObject recipeNameTextObject;
	private Text recipeNameText;
	private RectTransform recipeNameTransform;

	private GameObject timerTextObject;
	private Text timerText;
	private RectTransform timerTransform;

	private GameObject backgroundObject;
	private Image backgroundImage;
	private RectTransform backgroundTransform;

	public Order(string ID, Ingredient Recipe, GameObject ParentGameObject, float Timer, Transform orderPanel, string Team) {
		this.ID = ID;
		this.Recipe = Recipe;
		this.ParentGameObject = ParentGameObject;
		this.Timer = Timer;
		this.orderPanel = orderPanel;
		this.Team = Team;

		ParentGameObject.AddComponent<Canvas>();
		ParentGameObject.transform.SetParent(orderPanel.transform);
		ParentGameObject.transform.localPosition = new Vector3(0,0,0);
		ParentGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 300);

		canvas = ParentGameObject.GetComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.name = ID;

    ParentGameObject.AddComponent<CanvasScaler>();
    ParentGameObject.AddComponent<GraphicRaycaster>();

		// Background
		backgroundObject = new GameObject("backgroundObject", typeof(RectTransform));
		backgroundObject.transform.SetParent(canvas.transform);

		backgroundTransform = backgroundObject.GetComponent<RectTransform>();
		backgroundTransform.sizeDelta = new Vector2(250, 200);

		backgroundImage = backgroundObject.AddComponent<Image>();
		backgroundImage.sprite = Resources.Load("Ripped Note", typeof(Sprite)) as Sprite;

    // Timer Text Parent Object
    timerTextObject = new GameObject("timerText", typeof(RectTransform));
    timerTextObject.transform.SetParent(canvas.transform);

		// Text position
		timerTransform = timerTextObject.GetComponent<RectTransform>();
		timerTransform.localPosition = new Vector3(0, 0, 0);
		timerTransform.sizeDelta = new Vector2(400, 200);

		// Timer Text
    timerText = timerTextObject.AddComponent<Text>();
		timerText.fontSize = 40;
		timerText.color = Color.black;
		timerText.font = Resources.Load("Acids!", typeof(Font)) as Font;//Assets/Resources/Acids!.otf
		timerText.alignment = TextAnchor.UpperCenter;

		// Recipe Name Parent Object
		recipeNameTextObject = new GameObject("recipeNameText", typeof(RectTransform));
		recipeNameTextObject.transform.SetParent(canvas.transform);

		// Recipe Name position
		recipeNameTransform = recipeNameTextObject.GetComponent<RectTransform>();
		recipeNameTransform.sizeDelta = new Vector2(300, 200);

		// Recipe Name Text
		recipeNameText = recipeNameTextObject.AddComponent<Text>();
		recipeNameText.fontSize = 60;
		recipeNameText.color = Color.black;
		recipeNameText.font = Resources.Load("Acids!", typeof(Font)) as Font;
		recipeNameText.alignment = TextAnchor.UpperCenter;
		recipeNameText.text = Recipe.ToString();
		recipeNameText.horizontalOverflow = HorizontalWrapMode.Wrap;
	}

	public void updateCanvas(int index, int screenWidth, int screenHeight) {
		int side = Team.Equals("red") ? -1 : 1;

		int width;
		int height;

		// 4 gap + 2 small orders + 1 big order = screenWidth / 2
		int bigWidth = (screenWidth / 2) / 3;
		int smallWidth = (screenWidth / 2) / 4;
		int gapWidth = ((screenWidth / 2) - bigWidth - smallWidth * 2) / 4;

		int bigHeight = (screenHeight / 2) / 2;
		int smallHeight = (screenHeight / 2) / 3;

		float fontScale = 1.2f;

		if (index == 0) {
			height = bigHeight;
			width = bigWidth;
		} else {
			height = smallHeight;
			width = smallWidth;
			fontScale = 1.0f;
		}

		int timerOffset = (int) -height / 10;
		int recipeOffset = timerOffset + (int) -height / 5;

		backgroundTransform.sizeDelta = new Vector2(width, height);

		timerTransform.sizeDelta = new Vector2(width, height);
		timerText.fontSize = (int) (40 * fontScale);

		recipeNameTransform.sizeDelta = new Vector2(width, height);
		recipeNameText.fontSize = (int) (60 * fontScale);

		int posX;
		int posY = -(screenHeight / 2) + (height / 2);

		if (index == 0) {
			posX = side * (gapWidth + width / 2);
		} else {
			posX = side * ((1 + index) * gapWidth + bigWidth + (index - 1) * smallWidth + smallWidth / 2);
		}

		Vector3 position = new Vector3(posX, posY, 0);

		timerTransform.localPosition = position + new Vector3(0, timerOffset, 0);
		recipeNameTransform.localPosition = position + new Vector3(0, recipeOffset, 0);
		backgroundTransform.localPosition = position;

		if (!timerExpired()) {
			Timer -= Time.deltaTime;
			displayTime();
		} else {
			recipeNameText.color = Color.yellow;
		}
	}

	private void displayTime() {
		TimeSpan t = TimeSpan.FromSeconds(Timer);
		timerText.text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
		if (Timer <= 30) setTextRed();
	}

	public bool timerExpired() {
		return Timer < 0;
	}

	public void setTextRed() {
		timerText.color = Color.red;
		recipeNameText.color = Color.red;
	}
}
