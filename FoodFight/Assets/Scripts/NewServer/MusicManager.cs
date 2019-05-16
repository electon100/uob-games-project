using UnityEngine;

public class MusicManager : MonoBehaviour {
    private AudioSource source;
    private AudioClip frenchTheme, latinTheme;
    private NewServer server;

    public GameObject BlueCupboard, BlueChopping, BlueFrying, BluePlating,
                      RedCupboard, RedChopping, RedFrying, RedPlating;

    private AudioClip frenchBlueCupboard, frenchBlueChopping, frenchBlueFrying, frenchBluePlating,
                      frenchRedCupboard, frenchRedChopping, frenchRedFrying, frenchRedPlating,
                      latinBlueCupboard, latinBlueChopping, latinBlueFrying, latinBluePlating,
                      latinRedCupboard, latinRedChopping, latinRedFrying, latinRedPlating;

    public void Start() {
      server = GameObject.Find("Server").GetComponent<NewServer>();
      source = GetComponent<AudioSource>();
      source.Play();

      frenchTheme = (AudioClip) Resources.Load("frenchTheme", typeof(AudioClip));
      latinTheme = (AudioClip) Resources.Load("latinTheme", typeof(AudioClip));

      frenchBlueCupboard = (AudioClip)Resources.Load("FrenchBlueCupboard", typeof(AudioClip));
      frenchBlueChopping = (AudioClip)Resources.Load("FrenchBlueChopping", typeof(AudioClip));
      frenchBlueFrying = (AudioClip)Resources.Load("FrenchBlueFrying", typeof(AudioClip));
      frenchBluePlating = (AudioClip)Resources.Load("FrenchBluePlating", typeof(AudioClip));

      frenchRedCupboard = (AudioClip)Resources.Load("FrenchRedCupboard", typeof(AudioClip));
      frenchRedChopping = (AudioClip)Resources.Load("FrenchRedChopping", typeof(AudioClip));
      frenchRedFrying = (AudioClip)Resources.Load("FrenchRedFrying", typeof(AudioClip));
      frenchRedPlating = (AudioClip)Resources.Load("FrenchRedPlating", typeof(AudioClip));

      latinBlueCupboard = (AudioClip)Resources.Load("LatinBlueCupboard", typeof(AudioClip));
      latinBlueChopping = (AudioClip)Resources.Load("LatinBlueChopping", typeof(AudioClip));
      latinBlueFrying = (AudioClip)Resources.Load("LatinBlueFrying", typeof(AudioClip));
      latinBluePlating = (AudioClip)Resources.Load("LatinBluePlating", typeof(AudioClip));

      latinRedCupboard = (AudioClip)Resources.Load("LatinRedCupboard", typeof(AudioClip));
      latinRedChopping = (AudioClip)Resources.Load("LatinRedChopping", typeof(AudioClip));
      latinRedFrying = (AudioClip)Resources.Load("LatinRedFrying", typeof(AudioClip));
      latinRedPlating = (AudioClip)Resources.Load("LatinRedPlating", typeof(AudioClip));
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
              BlueCupboard.GetComponent<AudioSource>().clip = latinBlueCupboard;
              BlueChopping.GetComponent<AudioSource>().clip = latinBlueChopping;
              BlueFrying.GetComponent<AudioSource>().clip = latinBlueFrying;
              BluePlating.GetComponent<AudioSource>().clip = latinBluePlating;

              RedCupboard.GetComponent<AudioSource>().clip = latinRedCupboard;
              RedChopping.GetComponent<AudioSource>().clip = latinRedChopping;
              RedFrying.GetComponent<AudioSource>().clip = latinRedFrying;
              RedPlating.GetComponent<AudioSource>().clip = latinRedPlating;
              break;
            case GameMode.French:
              PlayClip(frenchTheme);
              BlueCupboard.GetComponent<AudioSource>().clip = frenchBlueCupboard;
              BlueChopping.GetComponent<AudioSource>().clip = frenchBlueChopping;
              BlueFrying.GetComponent<AudioSource>().clip = frenchBlueFrying;
              BluePlating.GetComponent<AudioSource>().clip = frenchBluePlating;

              RedCupboard.GetComponent<AudioSource>().clip = frenchRedCupboard;
              RedChopping.GetComponent<AudioSource>().clip = frenchRedChopping;
              RedFrying.GetComponent<AudioSource>().clip = frenchRedFrying;
              RedPlating.GetComponent<AudioSource>().clip = frenchRedPlating;
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
