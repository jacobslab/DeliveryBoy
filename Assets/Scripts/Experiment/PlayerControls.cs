using UnityEngine;
using System.Collections;

public class PlayerControls : MonoBehaviour{

	Experiment exp  { get { return Experiment.Instance; } }


	public bool ShouldLockControls = false;

	public Transform TiltableTransform;
	public Transform startPositionTransform1;

	float RotationSpeed = 50.0f;
	
	//float maxTimeToMove = 3.75f; //seconds to move across the furthest field distance
	//float minTimeToMove = 1.5f; //seconds to move across the closest field distance

	// Use this for initialization
	void Start () {
		//when in replay, we don't want physics collision interfering with anything
		if(ExperimentSettings.isReplay){
			GetComponent<Collider>().enabled = false;
		}
		else{
			GetComponent<Collider>().enabled = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (exp.currentState == Experiment.ExperimentState.inExperiment) {

			ShouldLockControls = false;

			if(!ShouldLockControls){
				GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY; // TODO: on collision, don't allow a change in angular velocity?

				//sets velocities
				GetInput ();
			}
			else{
				GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
				SetTilt(0.0f, 1.0f);
			}
		}
	}


    void GetInput()
    {
        //VERTICAL
        float verticalAxisInput = Input.GetAxis("Vertical");
        if (verticalAxisInput > 0.0f) { //EPSILON should be accounted for in Input Settings "dead zone" parameter

            GetComponent<Rigidbody>().velocity = transform.forward * verticalAxisInput * Config.driveSpeed; //since we are setting velocity based on input, no need for time.delta time component

        }
        else if (verticalAxisInput < 0.0f)
        {
            GetComponent<Rigidbody>().velocity = transform.forward * verticalAxisInput * Config.reverseSpeed;
        }
		else{
			GetComponent<Rigidbody>().velocity = Vector3.zero;
		}

		//HORIZONTAL
		float horizontalAxisInput = Input.GetAxis ("Horizontal");
		if (Mathf.Abs (horizontalAxisInput) > 0.0f) { //EPSILON should be accounted for in Input Settings "dead zone" parameter

			float percent = horizontalAxisInput / 1.0f;
			Turn (percent * RotationSpeed * Time.deltaTime); //framerate independent!
		} 
		else {
			if(!TrialController.isPaused){

				//resets the player back to center if the game gets paused on a tilt
				//NOTE: after pause is glitchy on keyboard --> unity seems to be retaining some of the horizontal axis input despite there being none. fine with controller though.

				float zTiltBack = 0.2f;
				float zTiltEpsilon = 2.0f * zTiltBack;
				float currentZRot = TiltableTransform.rotation.eulerAngles.z;
				if(currentZRot > 180.0f){
					currentZRot = -1.0f*(360.0f - currentZRot);
				}

				if(currentZRot > zTiltEpsilon){
					TiltableTransform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, currentZRot - zTiltBack);
				}
				else if (currentZRot < -zTiltEpsilon){
					TiltableTransform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, currentZRot + zTiltBack);
				}
				else{
					SetTilt(0.0f, 1.0f);
				}
			}
		}

	}

	void Move( float amount ){
		transform.position += transform.forward * amount;
	}
	
	void Turn( float amount ){
		transform.RotateAround (transform.position, Vector3.up, amount );
		SetTilt (amount, Time.deltaTime);
	}

	//based on amount difference of y rotation, tilt in z axis
	void SetTilt(float amountTurned, float turnTime){
		if (!TrialController.isPaused) {
			if (Config.isAvatarTilting) {
				float turnRate = 0.0f;
				if (turnTime != 0.0f) {
					turnRate = amountTurned / turnTime;
				}
			
				float tiltAngle = turnRate * Config.turnAngleMult;
			
				tiltAngle *= -1; //tilt in opposite direction of the difference
				TiltableTransform.rotation = Quaternion.Euler (transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, tiltAngle);	
			}
		}
	}

	//will set start position and rotation
	public void GoToStartPosition(){
		transform.position = startPositionTransform1.position;
		transform.rotation = startPositionTransform1.rotation;
	}

	public IEnumerator SmoothMoveTo(Vector3 targetPosition, Quaternion targetRotation, float timeToTravel){

		SetTilt (0.0f, 1.0f);

		//stop collisions
		GetComponent<Collider> ().enabled = false;


		Quaternion origRotation = transform.rotation;
		Vector3 origPosition = transform.position;

		//float travelDistance = (origPosition - targetPosition).magnitude;

		float tElapsed = 0.0f;

		//DEBUG
		float totalTimeElapsed = 0.0f;

		while(tElapsed < timeToTravel){
			totalTimeElapsed += Time.deltaTime;

			//tElapsed += (Time.deltaTime * moveAndRotateRate);

			tElapsed += Time.deltaTime;

			float percentageTime = tElapsed / timeToTravel;

			//will spherically interpolate the rotation for config.spinTime seconds
			transform.rotation = Quaternion.Slerp(origRotation, targetRotation, percentageTime); //SLERP ALWAYS TAKES THE SHORTEST PATH.
			transform.position = Vector3.Lerp(origPosition, targetPosition, percentageTime);


			yield return 0;
		}
		
		//Debug.Log ("TOTAL TIME ELAPSED FOR SMOOTH MOVE: " + totalTimeElapsed);

		transform.rotation = targetRotation;
		transform.position = targetPosition;

		//enable collisions again
		GetComponent<Collider> ().enabled = true;

		yield return 0;
	}

	public IEnumerator RotateTowardSpecialObject(GameObject target){
		Quaternion origRotation = transform.rotation;
		Vector3 targetPosition = new Vector3 (target.transform.position.x, transform.position.y, target.transform.position.z);
		transform.LookAt(targetPosition);
		Quaternion desiredRotation = transform.rotation;
		
		float angleDifference = origRotation.eulerAngles.y - desiredRotation.eulerAngles.y;
		angleDifference = Mathf.Abs (angleDifference);
		if (angleDifference > 180.0f) {
			angleDifference = 360.0f - angleDifference;
		}


		float rotationSpeed = 0.03f;
		float totalTimeToRotate = angleDifference * rotationSpeed;

		//rotate to look at target
		transform.rotation = origRotation;

		float tElapsed = 0.0f;
		while (tElapsed < totalTimeToRotate){
			tElapsed += (Time.deltaTime );
			float turnPercent = tElapsed / totalTimeToRotate;

			float beforeRotY = transform.rotation.eulerAngles.y; //y angle before the rotation

			//will spherically interpolate the rotation
			transform.rotation = Quaternion.Slerp(origRotation, desiredRotation, turnPercent); //SLERP ALWAYS TAKES THE SHORTEST PATH.

			float angleRotated = transform.rotation.eulerAngles.y - beforeRotY;
			SetTilt(angleRotated, Time.deltaTime);

			yield return 0;
		}
		
		
		
		transform.rotation = desiredRotation;
		
		//Debug.Log ("TIME ELAPSED WHILE ROTATING: " + tElapsed);
	}

	//returns the angle between the facing angle of the player and an XZ position
	public float GetYAngleBetweenFacingDirAndObjectXZ ( Vector2 objectPos ){

		Quaternion origRotation = transform.rotation;
		Vector3 origPosition = transform.position;

		float origYRot = origRotation.eulerAngles.y;

		transform.position = new Vector3( objectPos.x, origPosition.y, objectPos.y );
		transform.RotateAround(origPosition, Vector3.up, -origYRot);

		Vector3 rotatedObjPos = transform.position;


		//put player back in orig position
		transform.position = origPosition;

		transform.LookAt (rotatedObjPos);


		float yAngle = transform.rotation.eulerAngles.y;

		if(yAngle > 180.0f){
			yAngle = 360.0f - yAngle; //looking for shortest angle no matter the angle
			yAngle *= -1; //give it a signed value
		}

		transform.rotation = origRotation;

		return yAngle;

	}


	
}
