using UnityEngine;
using System.Collections;

public class MouseLogTrack : LogTrack {

	// Use this for initialization
	void Start () {
	
	}

	void Update(){ //can be called in update because we are checking for input. other logtracks use LateUpdate because things like positions must be finished updating before they are logged.
		if (!ExperimentSettings.isOculus && ExperimentSettings.isLogging) {
			LogMouse ();
		}
	}

	void LogMouse(){
		//log the position
		//TODO: do a check if the mouse position is out of range.
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "Mouse" + separator + "POSITION" + separator + Input.mousePosition.x + separator + Input.mousePosition.y);

		//log a clicked object
		if(Input.GetMouseButtonDown(0)){
			Ray ray;
			RaycastHit hit;
			if(Camera.main != null){
				ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if(Physics.Raycast(ray, out hit)){
					subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "Mouse"+ separator + "CLICKED" + separator + hit.collider.gameObject);
				}
				else{
					subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "Mouse" + separator +"CLICKED" + separator + "EMPTY");
				}
			}
			else{
				Debug.Log("Camera.main is null! Can't raycast mouse position.");
			}
		}
	}
}
