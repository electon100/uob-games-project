using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Introduction : MonoBehaviour {

	public Button nextButton, backButton, startTutorialButton;
	public Slider progressBar;
	public Image logoImg, serveImg, warningImg, chefImg, throwImg, exitImg;
	public Text mainText;

	private readonly int totalSlides = 6;

	private Client networkClient;
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
			case 0: return "Welcome to Food Fight, a fast paced cooking game in which two rival kitchens battle to prove that they are the best.";
			case 1: return "Your job is to work with your teammates to prove your kitchen is superior. Remember - good communication is key.";
			case 2: return "To get points, cook and serve the orders which show up on the main screen before they expire.";
			case 3: return "Stay on top of orders! If you miss one, you will lose points.";
			case 4: return "Damage the enemy kitchen's stations by throwing food at them. This prevents them from logging in for 10 seconds.";
			case 5: return "Only 1 person can use each station at any time. If you have finished at a station, be sure to leave so your teammates can use it!";
			case 6: return "Now you are ready to play the tutorial.\n\nGood luck!";
			default: return "";
		}
	}

	private void ShowRelevantImages(int slide) {
		logoImg.enabled = slide == 0 || slide == totalSlides;
		chefImg.enabled = slide == 1;
		serveImg.enabled = slide == 2;
		warningImg.enabled = slide == 3;
		throwImg.enabled = slide == 4;
		exitImg.enabled = slide == 5;
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

	private bool isOnFirstSlide() {
		return currentSlide == 0;
	}

	private void SetProgressBarValue() {
		progressBar.maxValue = totalSlides;
		progressBar.value = currentSlide;
	}

	private void UpdateButtonStates() {
		SetButtonInteractable(nextButton, hasNextSlide());
		SetButtonInteractable(backButton, hasPrevSlide() || isOnFirstSlide());
		startTutorialButton.gameObject.SetActive(currentSlide == totalSlides);
	}

	private void SetButtonInteractable(Button btn, bool interactable) {
		btn.interactable = interactable;
	}

	public void EndIntroduction() {
		networkClient.StartTutorial();
	}

	public void OnBackButtonPress() {
		if (hasPrevSlide()) prevSlide();
		else if (isOnFirstSlide()) networkClient.OnGameReset();
	}

	public void OnGotItButtonPress() {
		nextSlide();
	}

	public void OnStartTutorialPress() {
		EndIntroduction();
	}
}
