using UnityEngine;
using System.Collections;

public class Animation : MonoBehaviour {
    private GameObject serverObject;
    private NewServer server;
    public GameObject logo;
    public Transform startPanel;
    private int next = 1;
    private float lastMovement;

    public void Start() {
        serverObject = GameObject.Find("Server");
        if (serverObject != null) {
            server = serverObject.GetComponent<NewServer>();
        }
        lastMovement = Time.time;
    }
    
    public void CheckClicked() {
        bool isDesktop = Input.GetMouseButtonDown(0);
        bool isMobile = (Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began);
        if (isDesktop || isMobile) {
            Ray raycast = (isDesktop) ? Camera.main.ScreenPointToRay(Input.mousePosition) :
                                        Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit raycastHit;
            if (Physics.Raycast(raycast, out raycastHit)) {
                if (server != null) { /* Server main screen */
                    server.ExitMainScreen();
                } else { /* Client main screen */
                    GoToConnect();
                }
            }
        }
    }

    public void MoveLogo() {
        RectTransform tranform = logo.GetComponent<RectTransform>();
        tranform.transform.Translate(0, next*0.5f, 0);
        if ((Time.time - lastMovement) > 1.0f) {
            next *= -1;
            lastMovement = Time.time;
        }
    }

    public void Update() {
        CheckClicked();
        MoveLogo();
    }

    public void GoToConnect() {
        if (Client.gameState.Equals(ClientGameState.MainMode)) {
            GameObject.Find("MainMenuCanvas").SetActive(false);
            Client.gameState = ClientGameState.ConnectState;
        }
    }
}