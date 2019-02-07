using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour{

    static Transform targetPosition;
    static GameObject movingPlayer;
    float speed = 1.0f;

    private void Update()
    {
        float step = speed * Time.deltaTime; // calculate distance to move
        movingPlayer.transform.position = Vector3.MoveTowards(movingPlayer.transform.position, targetPosition.position, step);

        if (Vector3.Distance(movingPlayer.transform.position, targetPosition.position) < 0.001f)
        {
            movingPlayer.transform.position = targetPosition.position;
        }
    }

    static public void movePlayer(Vector3 stationPosition, GameObject player)
    {
        movingPlayer = player;
        targetPosition.position = stationPosition;
    }
}
