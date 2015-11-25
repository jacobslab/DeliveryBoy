using UnityEngine;
using System.Collections;

public class CameraLogTrack : LogTrack {
	
	bool lastEnabled = false;

	Camera myCamera;

	// Use this for initialization
	void Start () {
		myCamera = gameObject.GetComponent<Camera>();
		if(ExperimentSettings.isLogging){
			//initial log
			LogCamera ();
		}
	}
	
	//log on late update so that everything for that frame gets set first
	void LateUpdate () {
		if(ExperimentSettings.isLogging){
			if(lastEnabled != myCamera.enabled){
				LogCamera ();
			}
		}
	}

	void LogCamera(){
		if(myCamera != null){
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), gameObject.name + separator + "CAMERA_ENABLED" + separator + myCamera.enabled);
			lastEnabled = myCamera.enabled;
		}
	}
}
