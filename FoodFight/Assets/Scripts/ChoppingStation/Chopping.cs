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
    public Transform endCanvas;

    /* Output text to be displayed on screen */
    public Text yAcc;
    public Text chops;
    public Text outCome;
    public Text status;
    public Text notChoppable;

    /* Sounds for up and down acceleration */
    public AudioClip chopSound;
    private AudioSource source;

    /* Highest acceleration recorded so far */
    private float maxAcc = 0.5f; 

    /* Movement stuff */
	private float shakeSpeed = 10.0f; // Speed of pan shake
	private float shakeAmount = 1.2f; // Amplitude of pan shake
	private bool shouldShake = false;
	private int negSinCount = 0, posSinCount = 0;
	private Vector3 originalPos;
	private float lastShake;
    private float yTransform;

    private Player player;
    private Ingredient currentChoppingIngred;

    private List<GameObject> ingredientModels;

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
		lastShake = Time.time;

        /* Displays "INGREDIENT CANNOT BE CHOPPED" if appropriate */
        notChoppable = GameObject.Find("NotChoppableText").GetComponent<Text>();
        StartGame();

        /* Instantiate all text info */
        outCome = GameObject.Find("OutcomeText").GetComponent<Text>();
        chops = GameObject.Find("ChopText").GetComponent<Text>();
        yAcc = GameObject.Find("AccValText").GetComponent<Text>();
        status = GameObject.Find("StatusText").GetComponent<Text>();
        
    }

    void Update()
    {

        /* Constantly checks if ingredient is correctly chopped and displays success screen. */
        ChoppingStatus();

        /* Getting rid of the intro screen when player starts chopping. */
        if (Input.acceleration.y > maxAcc)
        {
            maxAcc = Input.acceleration.y;
            yAcc.text = maxAcc.ToString();
        }

        /* For desktop tests. */
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Player.currentIngred.numberOfChops++;
            source.PlayOneShot(chopSound);
            transform.Rotate(0, -5, 0);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            source.PlayOneShot(chopSound);
            transform.Rotate(0, 5, 0);
        }
        
        transform.position = originalPos;
        /* Check if the player has started the movement and increment the number of chops on the ingredient */
        CheckDownMovement();
        /* For sound effect. */
        CheckUpMovement();
        /* Uncomment to make game more interesting and add sliced fingers. */
        // CheckChopSpeed();
        
    }

    public void StartGame()
    {
        /* Check if the ingredient is choppable */
        bool startChopping = CheckIngredientValid();
        if (startChopping)
        {
            startCanvas.gameObject.SetActive(false);
            defaultCanvas.gameObject.SetActive(true);
        }
        else {
            notChoppable.text = "Ingredient cannot be chopped";
            /* Generate a warning canvas for an unchoppable ingredient. */
        }
    }

    void CheckUpMovement()
    {
        if (Input.acceleration.y < -3.0f)
        {
            // source.PlayOneShot(chopSound);
            transform.Rotate(0, 5, 0);
        }
    }


    void CheckDownMovement()
    {
        if (Input.acceleration.y > 3.0f)
        {
            source.PlayOneShot(chopSound);
            Player.currentIngred.numberOfChops++;
            transform.Rotate(0, -5, 0);
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

    void ChoppingStatus()
    {
        /* Checks if the ingredient has been chopped the right amount of times and updates it */
        if (FoodData.Instance.isChopped(Player.currentIngred))
        {
            defaultCanvas.gameObject.SetActive(false);
            endCanvas.gameObject.SetActive(true);
            /* Create a new list containing only this ingredient, so that we get the chopped version. */
            List<Ingredient> choppedIngredients = new List<Ingredient>();
            choppedIngredients.Add(Player.currentIngred);
            Player.currentIngred = FoodData.Instance.TryCombineIngredients(choppedIngredients);
        }
    }

    private bool CheckIngredientValid()
    {
        /* TODO: rewrite FoodData to check for a choppable ingredient */
        if (Player.currentIngred != null)
        {
            /* Stops the minigame if ingredient cannot be chopped */
            if (1 > 0)
            {
                return true;
            }
            else {
                return false;
            }
        }
        
        return false;
    }

    /* Sends the chopped ingredient to server and returns to the main screen */
    public void goBack()
    {
        // player.notifyServerAboutIngredientPlaced(currentChoppingIngred);
        SceneManager.LoadScene("PlayerMainScreen");
    }

    /* Returns player back to the chopping, after failing the first time */
    public void tryAgain()
    {
        warningCanvas.gameObject.SetActive(false);
        defaultCanvas.gameObject.SetActive(true);
    }
    
}