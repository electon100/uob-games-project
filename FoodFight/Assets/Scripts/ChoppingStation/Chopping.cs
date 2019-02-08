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
    public Image shakeImage;

    /* Sounds for up and down acceleration */
    public AudioClip downSound;
    public AudioClip upSound;
    private AudioSource source;

    /* Highest acceleration recorded so far */
    private float maxAcc = 0.5f; 

    public Player player;
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

        outCome.text = "START CHOPPING";
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
            Destroy(shakeImage);
        }

        /* For desktop tests. */
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Player.currentIngred.numberOfChops++;
        }

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
            /* Generate a warning canvas for an unchoppable ingredient. */
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
            if (FoodData.Instance.GetIngredientDescription(Player.currentIngred).choppable)
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
    
    // public void instantiateIngredientsInStation()
    // {
    //     /* If available, restore what was previously in that station */
    //     newBoardContents = Player.ingredientsFromStation;

    //     /* Needed because the list of ingredients in the stations is constatly updated, so constant drawing is avoided */
    //     foreach (Ingredient ingredient in newBoardContents)
    //     {
    //         if (boardContents.IndexOf(ingredient) < 0)
    //         {
    //             GameObject model = (GameObject)Resources.Load(ingredient.Model, typeof(GameObject));
    //             model = Instantiate(model, new Vector3(0, 0, 0), Quaternion.identity);
    //             model.transform.SetParent(startCanvas);
    //             boardContents.Add(ingredient);
    //             ingredientModels.Add(model);
    //         }
    //     }
    // }

    // public void clearChoppingBoard() {
    //     foreach (GameObject ingredient in ingredientModels) {
    //         Debug.Log(ingredient);
    //         /* If the model exists, then destroy it */
    //         if (ingredient) {
    //             Destroy (ingredient, 0.0f);
    //             Debug.Log(ingredient + "has been destroyed.");
    //         }
    //     }
    //     boardContents.Clear();
    //     player.clearIngredientsInStation("2");
    // }
}