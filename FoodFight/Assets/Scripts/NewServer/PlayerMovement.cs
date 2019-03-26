using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour{
    float speed = 10.0f;
	private Vector3 targetPosition;
	private Vector3 targetRotation;
	public Vector3 startPosition;
	bool needsMoving;
	float rotationSpeed = 100f;

	private void Start(){
		needsMoving = false;
	}

    private void Update()
    {
		if (needsMoving){
			float step = speed * Time.deltaTime; // calculate distance to move
			this.transform.position = Vector3.MoveTowards(this.transform.position, targetPosition, step);
			this.transform.LookAt(targetPosition);
			// this.Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
			// var q = Quaternion.LookRotation(targetPosition - this.transform.position);
 			// this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, q, rotationSpeed * Time.deltaTime);

			if (Vector3.Distance(this.transform.position, targetPosition) < 0.001f){
				this.transform.position = targetPosition;
				needsMoving = false;
				this.transform.LookAt(new Vector3(-targetPosition.x, targetPosition.y, targetPosition.z));
				// targetRotation = new Vector3(-targetPosition.x, targetPosition.y, targetPosition.z);
				// this.transform.LookAt(new Vector3(-targetPosition.x, targetPosition.y, targetPosition.z));

			}
		}
		// else if(this.transform.position == targetPosition){
		// 	var q = Quaternion.LookRotation(targetPosition - this.transform.position);
 		// 	this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, q, rotationSpeed * Time.deltaTime);
		// }
    }

    public void movePlayer(Vector3 newPosition)
    {
		if (newPosition == new Vector3(0, 0, 0)){
			targetPosition = startPosition;
		}
		else{
			targetPosition = newPosition;
		}
		// targetRotation = targetPosition;
		needsMoving = true;
    }
}
