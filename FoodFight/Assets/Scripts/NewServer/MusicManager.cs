using UnityEngine;

public class MusicManager : MonoBehaviour {
    private AudioSource source;
    private AudioClip gameTheme;
    private NewServer server;
    private string mode = "none";

    public void Start() {
        server = GameObject.Find("Server").GetComponent<NewServer>();
        source = GetComponent<AudioSource>();
        source.Play();
    }

    public void Update() {
        switch (server.gameState) {
            case GameState.AwaitingPlayers:
                if (server.gameMode.Equals(GameMode.Latin)) mode="latin";
                else if (server.gameMode.Equals(GameMode.French)) mode="french";
                gameTheme = (AudioClip) Resources.Load(mode + "Theme", typeof(AudioClip));
                break;
            case GameState.Countdown:
                AudioFadeOut();
                break;
            case GameState.GameRunning:
                source.clip = gameTheme;
                source.volume = 1;
                if (!source.isPlaying) {
                    source.Play();
                }
                break;
            case GameState.EndGame:
                AudioFadeOut();
                break;
            default:
                break;
        }
        if (server.gameState == GameState.Countdown) {
            AudioFadeOut();
        } 
    }

    public void AudioFadeOut(){
        source.volume -= 1 * Time.deltaTime;
        if (source.volume == 0) {
            source.Stop();
        }
    }
}