using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Chopping : MonoBehaviour
{

    /* Initialise different screens. */
    public Transform defaultCanvas;
    public Transform startCanvas;
    public Transform warningCanvas;
    public Transform notChoppableCanvas;
    public Transform endCanvas;

    /* Output text to be displayed on screen */
    public Text chops;
    public Text outCome;
    public Text status;
    public Text notChoppable;
    public Text instructions;

    /* Sounds for up and down acceleration */
    public AudioClip chopSound;
    public AudioClip slashSound;
    public AudioClip successSound;
    private AudioSource source;

    /* Highest acceleration recorded so far */
    private float maxAcc = 0.5f;

    /* Movement stuff */
	private float shakeSpeed = 10.0f; // Speed of pan shake
	private float shakeAmount = 2f; // Amplitude of pan shake
	private bool shouldShake = false;
	private int negSinCount = 0, posSinCount = 0;
	private Vector3 originalPos;
	private float lastChop;
    private float yTransform;

    /* Private fields necessary for chopping status and sound effects */
    private bool startChopping;
    private bool isChopped;
    private int previousChopsCount;
    private Player player;
    private Ingredient currentChoppingIngred;

    private void Start()
    {
        /* Instantiates the player to access functions and sets the
        current chopping ingredient to whatever the Player's currently holding. */
        player = GameObject.Find("Player").GetComponent<Player>();

        /* Set the ingredient the player is currently holding */
        currentChoppingIngred = Player.currentIngred;

        /* Set up scene */
        source = GetComponent<AudioSource>();
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        originalPos = gameObject.transform.position;

        /* Initialise private variables */
		lastChop = Time.time;
        startChopping = true;
        startChopping = CheckIngredientValid();
        previousChopsCount = Player.currentIngred.numberOfChops;
        isChopped = false;

        /* Start the game if possible */
        StartGame();
    }

    void Update()
    {
        if (startChopping) {
            /* Constantly checks if ingredient is correctly chopped and displays success screen. */
            ChoppingStatus();

            /* Getting rid of the intro screen when player starts chopping. */
            if (Input.acceleration.y > maxAcc)
            {
                maxAcc = Input.acceleration.y;
            }

            /* For desktop tests. */
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                transform.Rotate(0, -10, 0);
                Player.currentIngred.numberOfChops++;
                lastChop = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                transform.Rotate(0, 10, 0);
            }

            /* Updates the chops count on the screen. */
            chops.text = Player.currentIngred.numberOfChops.ToString();
            /* Simulates the movement of the knife. */
            KnifeMovement();
            /* Check if the player has started the movement and increment the number of chops on the ingredient */
            CheckDownMovement();
            /* For sound effect. */
            PlayChopSound();
            /* Uncomment to make game more interesting and add sliced fingers. */
            // CheckChopSpeed();

            if ((Player.currentIngred.numberOfChops % 3) == 0) {
                transform.position = originalPos;
            }
        }
    }

    public void StartGame()
    {
        if (startChopping)
        {
            startCanvas.gameObject.SetActive(false);
            defaultCanvas.gameObject.SetActive(true);
            /* Instantiate all text info */
            outCome = GameObject.Find("OutcomeText").GetComponent<Text>();
            chops = GameObject.Find("ChopText").GetComponent<Text>();
            status = GameObject.Find("StatusText").GetComponent<Text>();

            GameObject model = (GameObject) Resources.Load(Player.currentIngred.Model, typeof(GameObject));
            Transform modelTransform = model.GetComponentsInChildren<Transform>(true)[0];

            Quaternion modelRotation = modelTransform.rotation;
            Vector3 modelPosition = modelTransform.position;
            GameObject inst = Instantiate(model, modelPosition, modelRotation);
        }
    }

    void PlayChopSound()
    {
        if ((Player.currentIngred.numberOfChops > previousChopsCount) && !isChopped) {
            source.PlayOneShot(chopSound);
            previousChopsCount = Player.currentIngred.numberOfChops;
        }
    }

    void CheckDownMovement()
    {
        if (Input.acceleration.y > 2.0f)
        {
            source.PlayOneShot(chopSound);
            Player.currentIngred.numberOfChops++;
            lastChop = Time.time;
        }
    }

    /* Checks the chopping speed and displays a red sign if it's too fast */
    void CheckChopSpeed()
    {
        if (maxAcc > 5.0f || maxAcc < -5.0f)
        {
            defaultCanvas.gameObject.SetActive(false);
            warningCanvas.gameObject.SetActive(true);
        }
        else if (maxAcc < 3.5f && maxAcc > -3.5f)
        {
            outCome.text = "CHOP HARDER!";
        }
        else
        {
            outCome.text = " ";
        }
    }

    void KnifeMovement() 
    {
        if (Input.acceleration.y > 2.0f) {
            float xTransform = -1 * Mathf.Sin((Time.time - lastChop) * shakeSpeed) * shakeAmount;

			if (negSinCount > 0 && posSinCount > 0 && xTransform < 0) {
				gameObject.transform.position = originalPos;
				negSinCount = 0; posSinCount = 0;
				shouldShake = false;
			}	else if (xTransform < 0) {
				transform.Rotate(0, xTransform, 0);
				negSinCount++;
			} else if (xTransform > 0) {
				transform.Rotate(0, xTransform, 0);
				posSinCount++;
			}
        }
    }

    void ChoppingStatus()
    {
        /* Checks if the ingredient has been chopped the right amount of times and updates it */
        if (FoodData.Instance.isChopped(Player.currentIngred))
        {
            isChopped = true;
            source.PlayOneShot(successSound);
            defaultCanvas.gameObject.SetActive(false);
            endCanvas.gameObject.SetActive(true);
            
            Player.currentIngred = FoodData.Instance.TryAdvanceIngredient(Player.currentIngred);
        }
        /* Checks if player has start chopping and istructs them to do so if not */
        if (previousChopsCount != 0) {
            instructions.gameObject.SetActive(false);
        }
    }

    private bool CheckIngredientValid()
    {
        if (Player.isHoldingIngredient())
        {
            /* Stops the minigame if ingredient cannot be chopped */
            if (FoodData.Instance.isChoppable(Player.currentIngred))
            {
                return true;
            }
            else {
                defaultCanvas.gameObject.SetActive(false);
                notChoppableCanvas.gameObject.SetActive(true);
                notChoppable.text = "Ingredient cannot be chopped";
                return false;
            }
        }
        else {
            defaultCanvas.gameObject.SetActive(false);
            notChoppableCanvas.gameObject.SetActive(true);
            notChoppable.text = "You are not holding any ingredients";
        }
        return false;
    }

    /* Sends the chopped ingredient to server and returns to the main screen */
    public void goBack()
    {
        /* Notify server that player has left the station */
        Handheld.Vibrate();
		player.notifyAboutStationLeft();
        SceneManager.LoadScene("PlayerMainScreen");
    }

    /* Returns player back to the chopping, after failing the first time */
    public void tryAgain()
    {
        warningCanvas.gameObject.SetActive(false);
        defaultCanvas.gameObject.SetActive(true);
    }

}