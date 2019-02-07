using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour{

    static Dictionary<GameObject, Vector3> movingPlayers;

    float speed = 10.0f;

    private void Start()
    {
        movingPlayers = new Dictionary<GameObject, Vector3>();
    }

    private void Update()
    {
        if (movingPlayers.Count > 0) {
            List<GameObject> toBeDestroyed = new List<GameObject>();
            foreach (KeyValuePair<GameObject, Vector3> movingPlayer in movingPlayers)
            {
                float step = speed * Time.deltaTime; // calculate distance to move
                movingPlayer.Key.transform.position = Vector3.MoveTowards(movingPlayer.Key.transform.position, movingPlayer.Value, step);

                if (Vector3.Distance(movingPlayer.Key.transform.position, movingPlayer.Value) < 0.001f)
                {
                    movingPlayer.Key.transform.position = movingPlayer.Value;
                    toBeDestroyed.Add(movingPlayer.Key);
                }
            }
            foreach (GameObject player in toBeDestroyed)
            {
                movingPlayers.Remove(player);
            }
        }
    }

    static public void movePlayer(Vector3 stationPosition, GameObject player)
    {
        movingPlayers.Add(player, stationPosition);
    }
}
