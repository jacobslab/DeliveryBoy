using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CanvasGroupLogTrack : LogTrack {
	
	bool firstLog = false; //should make an initial log

	CanvasGroup myCanvasGroup;
	float currentAlpha;

	
	// Use this for initialization
	void Awake () {
		myCanvasGroup = GetComponent<CanvasGroup> ();
	}
	
	//log on late update so that everything for that frame gets set first
	void LateUpdate () {
		if (myCanvasGroup == null) {
			Debug.Log("Canvas group is null!");
		}
		if(ExperimentSettings.isLogging && ( currentAlpha != myCanvasGroup.alpha || !firstLog) ){ //if the text has changed, log it!
			firstLog = true;
			LogAlpha ();
		}
	}
	
	void LogAlpha(){
		currentAlpha = myCanvasGroup.alpha;

		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), gameObject.name + separator + "CANVAS_GROUP_ALPHA" + separator + currentAlpha );
	}
}
