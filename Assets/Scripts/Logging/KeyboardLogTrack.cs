using UnityEngine;
using System.Collections;

public class KeyboardLogTrack : LogTrack {

	public string[] Keys;

	// Use this for initialization
	void Start () {
	
	}

	void Update(){ //can be called in update because we are checking for input. other logtracks use LateUpdate because things like positions must be finished updating before they are logged.
		if(ExperimentSettings.isLogging){
			LogKeyboard();
		}
	}

	void LogKeyboard(){
		string keyName = "";
		for (int i = 0; i < Keys.Length; i++) {
			keyName = Keys[i];
			if (Input.GetKey (keyName.ToLower())) {
				subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "Keyboard" + separator + keyName);
			}
		}
	}
}
