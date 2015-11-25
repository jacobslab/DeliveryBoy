using UnityEngine;
using System.Collections;

//IN SOME CASES, MAY BE EASIER TO UNENABLE CHILD OBJECTS THAN TO GO THROUGH ALL RENDERERS IN THE VISBILITY TOGGLER.
//THUS, WE NEED TO BE ABLE TO LOG THAT IN ADDITION TO JUST USING THE VISIBILITY TOGGLER & LOGGIING ISVISIBLE.
//example: SelectObjectUI.cs.
public class EnableChildrenLogTrack : LogTrack {

	// Use this for initialization
	void Start () {
	
	}

	public void LogChildrenEnabled ( bool areEnabled ) {
		if (ExperimentSettings.isLogging) {
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), gameObject.name + separator + "ENABLED_CHILDREN" + separator + areEnabled);
		}
	}
}
