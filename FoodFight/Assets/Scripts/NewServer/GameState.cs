public enum GameState {
  MainMenu,
  ConfigureMode, /* Sets the mode of the game (Latin or French) */
  // ConfigurePlayers, /* Sets the minimum number of players */
  AwaitingPlayers, /* At the start of the game and during the lobby */
  Countdown, /* Called at the synchronisation point before start of the game */
  GameRunning, /* During the whole game */
  EndGame /* Whenever a team wins, or at a draw */
};