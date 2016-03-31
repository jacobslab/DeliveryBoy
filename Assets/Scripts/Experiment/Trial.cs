using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//FOR USE IN TRIALCONTROLLER
public class Trial {
	Experiment exp { get { return Experiment.Instance; } }

	public Vector3 avatarStartPos;
	public Quaternion avatarStartRot;

	public Trial(){
		avatarStartPos = exp.player.controls.startPositionTransform1.position;//new Vector3 (exp.player.controls.startPositionTransform1.position.x, exp.player.transform.position.y, exp.player.controls.startPositionTransform1.z);
		avatarStartRot = exp.player.controls.startPositionTransform1.rotation;//Quaternion.Euler (0, exp.player.controls.startPositionTransform1.rotation, 0);
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

		counterTrial.avatarStartPos = exp.player.controls.startPositionTransform1.position;
		counterTrial.avatarStartRot = exp.player.controls.startPositionTransform1.rotation;

		
		return counterTrial;
	}
}