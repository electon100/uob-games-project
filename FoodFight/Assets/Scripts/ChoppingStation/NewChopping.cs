using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text;

public class NewChopping : MonoBehaviour {

	public Button goBackBtn;
	public Text statusText;
	public Material successMaterial;
	public Material neutralMaterial;
	public Material issueMaterial;
	public Renderer background;

	public AudioClip chopSound;
  public AudioClip successSound;
  private AudioSource audioSource;

	private readonly float minimumChopInterval = 0.3f; // (seconds)

	private Player player;
	private float lastChop;

	void Start () {
		Screen.orientation = ScreenOrientation.Portrait;
		background.material = neutralMaterial;
		player = GameObject.Find("Player").GetComponent<Player>();
		audioSource = GetComponent<AudioSource>();

		lastChop = Time.time;
	}

	void Update () {

		if (Player.isHoldingIngredient()) {
			if (FoodData.Instance.isChoppable(Player.currentIngred)) {
				// if (FoodData.Instance)
				ChangeView("Shake phone to start chopping", neutralMaterial);

				if (DetectChop() || Input.GetKeyDown(KeyCode.DownArrow)) {
					/* Chop detected! */
					DoSingleChop();
				}

			} else {
				ChangeView("Ingredient not choppable", issueMaterial);
			}
		} else {
			ChangeView("No ingredient to chop", issueMaterial);
		}

	}

	private void DoSingleChop() {
    audioSource.PlayOneShot(chopSound);
    Player.currentIngred.numberOfChops++;
    lastChop = Time.time;
	}

	private bool DetectChop() {
    return (Time.time - lastChop) > minimumChopInterval && Input.acceleration.y > 2.0f;
  }

	private void ChangeView(string message, Material material) {
		statusText.text = message;
		background.material = material;
	}

	public void OnGoBack() {
		goBack();
	}

	private void goBack() {
		/* Notify server that player has left the station */
		Handheld.Vibrate();
		player.notifyAboutStationLeft();
		SceneManager.LoadScene("PlayerMainScreen");
	}
}
