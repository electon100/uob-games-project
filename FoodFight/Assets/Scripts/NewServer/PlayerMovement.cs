using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour{
    float speed = 10.0f;
	Vector3 targetPosition;

    private void Update()
    {
		float step = speed * Time.deltaTime; // calculate distance to move
		this.transform.position = Vector3.MoveTowards(this.transform.position, targetPosition, step);

		if (Vector3.Distance(this.transform.position, targetPosition) < 0.001f){
			this.transform.position = targetPosition;
		}
    }

    public void movePlayer(Vector3 newPosition)
    {
		targetPosition = newPosition;
    }
}
