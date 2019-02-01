using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Chopping : MonoBehaviour
{

    // Initialise different screens
    public Transform defaultCanvas;
    public Transform startCanvas;
    public Transform warningCanvas;
    public Transform endCanvas;

    public Player player;

    private float maxAcc = 0.5f;  //The highest acceleration recorded so far
    private int chopCount = 0;    //number of chops
    //private bool gameStarted = false;

    // Output text to be displayed on screen
    public Text yAcc;      //only for testing
    public Text chops;
    public Text outCome;
    public Text status;
    public Image shakeImage;

    // Sounds to accompany up and down acceleration
    public AudioClip downSound;
    public AudioClip upSound;

    public GameObject blood;
    private AudioSource source;

    private static Ingredient currentIngred;
    public List<Ingredient> boardContents = new List<Ingredient>();
    public List<Ingredient> newBoardContents = new List<Ingredient>();
    List<Ingredient> choppedIngredients = new List<Ingredient>();

    private void Start()
    {
        currentIngred = Player.currentIngred;
        //set up scene
        source = GetComponent<AudioSource>();
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        //call to centre block(knife) to the middle of the screen
        InvokeRepeating("CenterKnife", 0f, 5.0f);

        //setting up ingredient

        CheckIngredientValid();

        Time.timeScale = 0;

        /* Needed because the list of ingredients in the stations is constatly updated, so constant drawing is avoided */
        boardContents = new List<Ingredient>();
    }

    void Update()
    {

        ChoppingStatus();

        MoveKnife();

        if (Input.acceleration.y > maxAcc)
        {
            maxAcc = Input.acceleration.y;
            yAcc.text = maxAcc.ToString();
            Destroy(shakeImage);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Player.currentIngred.numberOfChops++;
        }

        CheckDownMovement();
        CheckUpMovement();
        CheckChopSpeed();
        instantiateIngredientsInStation();
    }

    public void StartGame()
    {
        if (defaultCanvas.gameObject.activeInHierarchy == false)
        {
            startCanvas.gameObject.SetActive(false);
            defaultCanvas.gameObject.SetActive(true);
            Time.timeScale = 1;
        }
    }

    void CenterKnife()
    {
        transform.position = new Vector3(0, 0, 0);
        source.PlayOneShot(downSound);
    }

    void MoveKnife()
    {
        if (Input.acceleration.y > 3.0f || Input.acceleration.y < -3.0f)
        {
            transform.Translate(0, Input.acceleration.y * 0.5f, 0);
        }
    }

    void CheckUpMovement()
    {
        if (Input.acceleration.y < -3.0f)
        {
            source.PlayOneShot(downSound);
        }
    }

    void CheckDownMovement()
    {
        /* Check if the player has started the movement and increment the number of chops on the ingredient */
        if (Input.acceleration.y > 3.0f)
        {
            source.PlayOneShot(downSound);
            Player.currentIngred.numberOfChops++;
            chops.text = Player.currentIngred.numberOfChops.ToString();
        }
        
    }

    /* Checks the chopping speed and displays a red sign if it's too fast */
    void CheckChopSpeed()
    {
        if (maxAcc > 5.0f || maxAcc < -5.0f)
        {
            defaultCanvas.gameObject.SetActive(false);
            warningCanvas.gameObject.SetActive(true);
            Time.timeScale = 0;
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
            Time.timeScale = 0;
            choppedIngredients.Add(Player.currentIngred);
            FoodData.Instance.TryCombineIngredients(choppedIngredients);
        }
    }

    void CheckIngredientValid()
    {
        if (currentIngred != null)
        {
            /* Stops the minigame if ingredient cannot be chopped */
            if (FoodData.Instance.GetIngredientDescription(currentIngred).choppable)
            {
                Time.timeScale = 0;
            }
        }
    }

    /* Sends the chopped ingredient to server and returns to the main screen */
    public void goBack()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        player.notifyServerAboutIngredientPlaced();
        SceneManager.LoadScene("PlayerMainScreen");
    }

    /* Returns player back to the chopping, after failing the first time */
    public void tryAgain()
    {
        endCanvas.gameObject.SetActive(false);
        defaultCanvas.gameObject.SetActive(true);
    }

    public void instantiateIngredientsInStation()
    {
        /* If available, restore what was previously in that station */
        newBoardContents = Player.ingredientsFromStation;

        /* Needed because the list of ingredients in the stations is constatly updated, so constant drawing is avoided */
        foreach (Ingredient ingredient in newBoardContents)
        {
            if (boardContents.IndexOf(ingredient) < 0)
            {
                GameObject model = (GameObject)Resources.Load(ingredient.Model, typeof(GameObject));
                model = Instantiate(model, new Vector3(0, 0, 0), Quaternion.identity);
                model.transform.SetParent(startCanvas);
                boardContents.Add(ingredient);
            }
        }
    }

    public void putIngredient()
    {
        if (currentIngred != null)
        {
            GameObject model = (GameObject)Resources.Load(currentIngred.Model, typeof(GameObject));
            model = Instantiate(model, new Vector3(0, 0, 0), Quaternion.identity);
            model.transform.SetParent(startCanvas);
        }
    }
}