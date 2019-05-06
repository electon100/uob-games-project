using UnityEngine;

public class MusicManager : MonoBehaviour {
    private AudioSource source;
    private AudioClip frenchTheme, latinTheme;
    private NewServer server;

    public void Start() {
      server = GameObject.Find("Server").GetComponent<NewServer>();
      source = GetComponent<AudioSource>();
      source.Play();

      frenchTheme = (AudioClip) Resources.Load("frenchTheme", typeof(AudioClip));
      latinTheme = (AudioClip) Resources.Load("latinTheme", typeof(AudioClip));
    }

    public void Update() {
      switch (server.gameState) {
        case GameState.MainMenu:
          PlayClip(frenchTheme);
          break;
        case GameState.Countdown:
          AudioFadeOut();
          break;
        case GameState.GameRunning:
          switch(server.gameMode) {
            case GameMode.Latin:
              PlayClip(latinTheme);
              break;
            case GameMode.French:
              PlayClip(frenchTheme);
              break;
            default:
              break;
          }
          break;
        case GameState.EndGame:
          AudioFadeOut();
          break;
        default:
          break;
      }
    }

    private void PlayClip(AudioClip clip) {
      source.clip = clip;
      source.volume = 1;
      if (!source.isPlaying) source.Play();
    }

    public void AudioFadeOut() {
      source.volume -= 1 * Time.deltaTime;
      if (source.volume == 0) source.Stop();
    }
}