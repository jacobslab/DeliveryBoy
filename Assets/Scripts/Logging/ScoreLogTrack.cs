using UnityEngine;
using System.Collections;

public class ScoreLogTrack : LogTrack {

	// Use this for initialization
	void Start () {
	
	}

	public void LogBoxSwapperPoints(int scoreAdded){
		if (ExperimentSettings.isLogging) {
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), gameObject.name + separator + "SCORE_ADDED_BOX_SWAPPER" + separator + scoreAdded);
		}
	}

	public void LogTreasureOpenScoreAdded(int scoreAdded){
		if (ExperimentSettings.isLogging) {
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), gameObject.name + separator + "SCORE_ADDED_TREASURE" + separator + scoreAdded);
		}
	}

	public void LogTimeBonusAdded(int scoreAdded){
		if (ExperimentSettings.isLogging) {
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), gameObject.name + separator + "SCORE_ADDED_TIME" + separator + scoreAdded);
		}
	}

	public void LogMemoryScoreAdded(int scoreAdded){
		if (ExperimentSettings.isLogging) {
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), gameObject.name + separator + "SCORE_ADDED_MEMORY" + separator + scoreAdded);
		}
	}

	public void LogScoreReset(){
		if (ExperimentSettings.isLogging) {
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), gameObject.name + separator + "SCORE_RESET");
		}
	}
}
