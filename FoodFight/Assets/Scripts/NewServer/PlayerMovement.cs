using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour{
    float speed = 10.0f;
	private Vector3 targetPosition;
	public Vector3 startPosition;
	bool needsMoving;

	private void Start(){
		needsMoving = false;
	}

    private void Update()
    {
		if (needsMoving){
			float step = speed * Time.deltaTime; // calculate distance to move
			this.transform.position = Vector3.MoveTowards(this.transform.position, targetPosition, step);

			if (Vector3.Distance(this.transform.position, targetPosition) < 0.001f){
				this.transform.position = targetPosition;
				needsMoving = false;
			}
		}
    }

    public void movePlayer(Vector3 newPosition)
    {
		if (newPosition == new Vector3(0, 0, 0)){
			targetPosition = startPosition;
		}
		else{
			targetPosition = newPosition;
		}
		needsMoving = true;
    }
}
