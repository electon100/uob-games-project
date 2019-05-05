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

	public Order(string ID, Ingredient Recipe, GameObject ParentGameObject, float Timer, Transform orderPanel) {
		this.ID = ID;
		this.Recipe = Recipe;
		this.ParentGameObject = ParentGameObject;
		this.Timer = Timer;
		this.orderPanel = orderPanel;

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

	public void updateCanvas(Vector3 pos, float scale) {
		int height = (int) (200 * scale);
		float fontScale = (1.0f + (scale - 1.0f) / 2);

		backgroundTransform.sizeDelta = new Vector2((int) 250 * scale, height);

		timerTransform.sizeDelta = new Vector2((int) 400 * scale, height);
		timerText.fontSize = (int) (40 * fontScale);

		recipeNameTransform.sizeDelta = new Vector2((int) 300 * scale, height);
		recipeNameText.fontSize = (int) (60 * fontScale);

		timerTransform.localPosition = pos + new Vector3(0, - (int) fontScale * 40, 0);
		recipeNameTransform.localPosition = pos + new Vector3(0, - (int) fontScale * 80, 0);
		backgroundTransform.localPosition = pos + new Vector3(0, 0, 0);

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
	}

	public bool timerExpired() {
		return Timer < 0;
	}

	public void setTextRed() {
		timerText.color = Color.red;
		recipeNameText.color = Color.red;
	}
}
