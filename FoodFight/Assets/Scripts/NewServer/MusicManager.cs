using UnityEngine;

public class MusicManager : MonoBehaviour {
    private AudioSource source;
    public AudioClip gameTheme;
    private NewServer server;

    public void Start() {
        server = GameObject.Find("Server").GetComponent<NewServer>();
        source = GetComponent<AudioSource>();
        source.Play();
    }

    public void Update() {
        switch (server.gameState) {
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