using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour{

    static Vector3 targetPosition;
    static GameObject movingPlayer;
    float speed = 5.0f;

    private void Update()
    {
        if (movingPlayer) {
            float step = speed * Time.deltaTime; // calculate distance to move
            movingPlayer.transform.position = Vector3.MoveTowards(movingPlayer.transform.position, targetPosition, step);

            if (Vector3.Distance(movingPlayer.transform.position, targetPosition) < 0.001f)
            {
                movingPlayer.transform.position = targetPosition;
            }
        }
    }

    static public void movePlayer(Vector3 stationPosition, GameObject player)
    {
        movingPlayer = player;
        targetPosition = stationPosition;
    }
}
