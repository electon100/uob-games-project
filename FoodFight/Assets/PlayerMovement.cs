using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class PlayerMovement {

    static float speed = 1.0f;

    static public void movePlayer(Vector3 stationPosition, GameObject player)
    {
        player.transform.position = stationPosition;  

        while (player.transform.position != stationPosition)
        {
            float step = speed * Time.deltaTime; // calculate distance to move
            player.transform.position = Vector3.MoveTowards(player.transform.position, stationPosition, step);

            if (Vector3.Distance(player.transform.position, stationPosition) < 0.001f)
            {
                player.transform.position = stationPosition;
            }
        }

       
    }
}
