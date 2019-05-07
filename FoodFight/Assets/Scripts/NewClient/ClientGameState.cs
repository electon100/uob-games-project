public enum ClientGameState {
    TutorialMode, /* When someone presses tutorial mode */
    ConnectState, /* When a player connects for the first time */
    JoinState, /* When a player is already connected, but current game has been reset */
    MainMode, /* Normal playing mode */
    RecipeIntro, /* Introduces the user to the recipe */
    CupboardTutorial, /* First tutorial to show how to pick up ingredients */
    ChoppingTutorial, /* Second tutorial to show how to chop ingredients */
    FryingTutorial, /* Third tutorial to show how to fry ingredients */
    PlatingTutorial, /* Forth tutorial to show how to plate or throw ingredients */
    EndTutorial
};