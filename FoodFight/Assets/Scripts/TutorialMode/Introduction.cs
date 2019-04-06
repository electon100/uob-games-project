using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Introduction : MonoBehaviour {

	public Button nextButton, backButton, startTutorialButton;
	public Slider progressBar;
	public Image logoImg, serveImg, warningImg, chefImg, throwImg;
	public Text mainText;

	private Client networkClient;
	private readonly int totalSlides = 10;
	private int currentSlide = 0;

	void Start () {
		networkClient = GameObject.Find("Client").GetComponent<Client>();
	}

	void Update () {
		ForceSlideWithinRange(); // Just in case!
		UpdateButtonStates();
		SetProgressBarValue();
		SetMainText(getTextForSlide(currentSlide));
		ShowRelevantImages(currentSlide);
	}

	private string getTextForSlide(int slide) {
		switch(slide) {
			case 0:
				return "Welcome to Food Fight, a fast paced cooking game in which two rival kitchens battle to prove that their kitchen is the best.";
			case 1:
				return "When playing, you will be part of a team (either Red or Blue).\nYour job is work with your teammates to prove your kitchen is superior.";
			case 2:
				return "To get points, cook and serve the orders which show up on the main screen before they expire.";
			case 3:
				return "Stay on top of orders! If you miss one, you will lose points.";
			case 4:
				return "Damage the enemy kitchen's stations by throwing food at them. This prevents them from logging in for a minimum of 10 seconds.";
			// case 5:
			// 	return "Damage the enemy kitchen's stations by throwing food at them. This prevents them from logging in for a minimum of 10 seconds.";
			default:
				return "";
		}
	}

	private void ShowRelevantImages(int slide) {
		logoImg.enabled = slide == 0;
		chefImg.enabled = slide == 1;
		serveImg.enabled = slide == 2;
		warningImg.enabled = slide == 3;
		throwImg.enabled = slide == 4;
	}

	private void ForceSlideWithinRange() {
		if (currentSlide < 0 || currentSlide > totalSlides) currentSlide = 0;
	}

	private void SetMainText(string text) {
		mainText.text = text;
	}

	private bool hasNextSlide() {
		return currentSlide < totalSlides;
	}

	private bool hasPrevSlide() {
		return currentSlide > 0;
	}

	private void nextSlide() {
		if (hasNextSlide()) currentSlide++;
	}

	private void prevSlide() {
		if (hasPrevSlide()) currentSlide--;
	}

	private void SetProgressBarValue() {
		progressBar.maxValue = totalSlides;
		progressBar.value = currentSlide;
	}

	private void UpdateButtonStates() {
		SetButtonInteractable(nextButton, hasNextSlide());
		SetButtonInteractable(backButton, hasPrevSlide());
		startTutorialButton.gameObject.SetActive(currentSlide == totalSlides);
	}

	private void SetButtonInteractable(Button btn, bool interactable) {
		btn.interactable = interactable;
	}

	public void EndIntroduction() {
		networkClient.StartTutorial();
	}

	public void OnBackButtonPress() {
		prevSlide();
	}

	public void OnGotItButtonPress() {
		nextSlide();
	}

	public void OnStartTutorialPress() {
		EndIntroduction();
	}
}
