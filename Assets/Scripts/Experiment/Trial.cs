using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//FOR USE IN TRIALCONTROLLER
public class Trial {
	Experiment exp { get { return Experiment.Instance; } }

	public Vector3 avatarStartPos;
	public Quaternion avatarStartRot;
	public List<Vector2> DefaultObjectLocationsXZ;

	public Trial(){

		int fiftyFiftyChance = Random.Range (0, 2); //will pick 1 or 0
		if (fiftyFiftyChance == 0) {
			avatarStartPos = exp.player.controls.startPositionTransform1.position;//new Vector3 (exp.player.controls.startPositionTransform1.position.x, exp.player.transform.position.y, exp.player.controls.startPositionTransform1.z);
			avatarStartRot = exp.player.controls.startPositionTransform1.rotation;//Quaternion.Euler (0, exp.player.controls.startPositionTransform1.rotation, 0);
		}
		else {
			avatarStartPos = exp.player.controls.startPositionTransform2.position;
			avatarStartRot = exp.player.controls.startPositionTransform2.rotation;
		}

		int numDefaultObjects = Config.numDefaultObjects;

		//init default and special locations
		DefaultObjectLocationsXZ = exp.objectController.GenerateOrderedDefaultObjectPositions (numDefaultObjects, avatarStartPos);

	}

	//get reflected rotation
	public Quaternion GetReflectedRotation(Quaternion rot){
		Vector3 newRot = rot.eulerAngles;
		newRot = new Vector3(newRot.x, newRot.y + 180, newRot.z);
		return Quaternion.Euler(newRot);
	}
	
	//get reflected position from the environment controller!
	//Given the center of the environment, calculate a reflected position in the environment
	public Vector3 GetReflectedPositionXZ(Vector3 pos){
		
		Vector3 envCenter = exp.environmentController.GetEnvironmentCenter ();
		
		Vector3 distanceFromCenter = pos - envCenter;
		
		float reflectedPosX = envCenter.x - distanceFromCenter.x;
		float reflectedPosZ = envCenter.z - distanceFromCenter.z;
		
		Vector3 reflectedPos = new Vector3 ( reflectedPosX, pos.y, reflectedPosZ );
		
		return reflectedPos;
	}
	
	public Trial GetCounterSelf(){
		Trial counterTrial = new Trial ();

		//counter the avatar
		if (avatarStartPos == exp.player.controls.startPositionTransform1.position) {
			counterTrial.avatarStartPos = exp.player.controls.startPositionTransform2.position;
			counterTrial.avatarStartRot = exp.player.controls.startPositionTransform2.rotation;
		} 
		else {
			counterTrial.avatarStartPos = exp.player.controls.startPositionTransform1.position;
			counterTrial.avatarStartRot = exp.player.controls.startPositionTransform1.rotation;
		}

		counterTrial.DefaultObjectLocationsXZ = new List<Vector2> ();
		//counter the object positions
		for (int i = 0; i < DefaultObjectLocationsXZ.Count; i++) {
			Vector3 currPosition = new Vector3( DefaultObjectLocationsXZ[i].x, 0, DefaultObjectLocationsXZ[i].y );
			Vector3 counteredPosition = GetReflectedPositionXZ( currPosition );

			Vector2 counteredPositionXZ = new Vector2(counteredPosition.x, counteredPosition.z);
			counterTrial.DefaultObjectLocationsXZ.Add(counteredPositionXZ);
		}

		
		return counterTrial;
	}
}