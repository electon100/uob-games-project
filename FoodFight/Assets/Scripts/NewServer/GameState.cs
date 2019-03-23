enum GameState {
  MainMenu,
  ConfigureGame,
  AwaitingPlayers, /* At the start of the game and during the lobby */
  Countdown, /* Called at the synchronisation point before start of the game */
  GameRunning, /* During the whole game */
  EndGame /* Whenever a team wins, or at a draw */
};