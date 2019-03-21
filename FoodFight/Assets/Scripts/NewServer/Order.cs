using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Order {

	public Ingredient Recipe { get; set; }
	public GameObject ParentGameObject { get; set; }
	public float Timer { get; set; }

	private Canvas canvas;
	private GameObject orderPanel;

	private GameObject recipeNameTextObject;
	private Text recipeNameText;
	private RectTransform recipeNameTransform;

	private GameObject timerTextObject;
	private Text timerText;
	private RectTransform timerTransform;

	private GameObject recipePrefabObject;
	private RectTransform recipePrefabTransform;

	public Order(Ingredient Recipe, GameObject ParentGameObject, float Timer, GameObject orderPanel) {
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
		canvas.name = Recipe.Name + "Canvas";

    ParentGameObject.AddComponent<CanvasScaler>();
    ParentGameObject.AddComponent<GraphicRaycaster>();

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
		timerText.alignment = TextAnchor.MiddleCenter;

		// Recipe Name Parent Object
		recipeNameTextObject = new GameObject("recipeNameText", typeof(RectTransform));
		recipeNameTextObject.transform.SetParent(canvas.transform);

		// Recipe Name position
		recipeNameTransform = recipeNameTextObject.GetComponent<RectTransform>();
		recipeNameTransform.sizeDelta = new Vector2(400, 200);

		// Recipe Name Text
		recipeNameText = recipeNameTextObject.AddComponent<Text>();
		recipeNameText.fontSize = 70;
		recipeNameText.color = Color.black;
		recipeNameText.font = Resources.Load("Acids!", typeof(Font)) as Font;//Assets/Resources/Acids!.otf
		recipeNameText.alignment = TextAnchor.MiddleCenter;
		recipeNameText.text = Recipe.ToString();
		recipeNameText.horizontalOverflow = HorizontalWrapMode.Overflow;

		GameObject recipeModel = (GameObject) ((Resources.Load(Recipe.Model) == null) ? Resources.Load("chipsPrefab") : Resources.Load(Recipe.Model));
		recipePrefabObject = GameObject.Instantiate(recipeModel) as GameObject;
		recipePrefabObject.transform.SetParent(canvas.transform);

		// recipePrefab position
		recipePrefabTransform = recipePrefabObject.AddComponent<RectTransform>();
		recipePrefabTransform.sizeDelta = new Vector2(200, 200);
		recipePrefabTransform.localScale = new Vector3(200.0F, 200.0F, 200.0F);
	}

	public void updateCanvas(Vector3 pos) {
		timerTransform.localPosition = pos;
		recipeNameTransform.localPosition = pos + new Vector3(0,-50,0);
		recipePrefabTransform.localPosition = pos + new Vector3(-100,-25,-200);

		if (Timer >= 0) {
			Timer -= Time.deltaTime;
			displayTime();
		}
	}

	private void displayTime() {
		TimeSpan t = TimeSpan.FromSeconds(Timer);
		timerText.text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
	}
}
