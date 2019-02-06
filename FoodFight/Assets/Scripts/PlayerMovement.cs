using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour{

    static public void movePlayer(Vector3 stationPosition, GameObject player)
    {
        player.transform.position = stationPosition;
        Debug.Log("Trying to move");

        while (player.transform.position != stationPosition)
        {
            float step = 0.1f; // calculate distance to move
            player.transform.position = Vector3.MoveTowards(player.transform.position, stationPosition, step);

            if (Vector3.Distance(player.transform.position, stationPosition) < 0.001f)
            {
                player.transform.position = stationPosition;
            }
        }

       
    }
}
