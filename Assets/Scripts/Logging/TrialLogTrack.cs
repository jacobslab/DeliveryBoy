using UnityEngine;
using System.Collections;

public class TrialLogTrack : LogTrack {


	bool firstLog = false;

	//log on late update so that everything for that frame gets set first
	void LateUpdate () {
		//just log the environment info on the first frame
		if (ExperimentSettings.isLogging && !firstLog) {
			LogEnvironmentDimensions ();
			firstLog = true;
		}
	}

	//gets called from trial controller instead of in update!
	public void Log(int trialNumber){
		if (ExperimentSettings.isLogging) {
			LogTrial (trialNumber);
		}
	}

	//LOGGED ON THE START OF THE TRIAL.
	void LogTrial(int trialNumber){
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "Trial Info" + separator + "NUM_TRIALS" + separator + trialNumber);
	}


	//TODO: move to an experiment or an environment logger... just want to log this once at the beginning of the trials so there is a reference for all positions in the world.
	void LogEnvironmentDimensions(){
		//log center
		Vector3 envCenter = exp.environmentController.center;
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "Experiment Info" + separator + "ENV_CENTER" + separator + envCenter.x + separator + envCenter.y + separator + envCenter.z);
	
		//log walls
		Vector3 wallPos = exp.environmentController.WallsXPos.position;
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "Experiment Info" + separator + "ENV_WALL_XPOS" + separator + wallPos.x + separator + wallPos.y + separator + wallPos.z);

		wallPos = exp.environmentController.WallsXNeg.position;
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "Experiment Info" + separator + "ENV_WALL_XNEG" + separator + wallPos.x + separator + wallPos.y + separator + wallPos.z);

		wallPos = exp.environmentController.WallsZPos.position;
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "Experiment Info" + separator + "ENV_WALL_ZPOS" + separator + wallPos.x + separator + wallPos.y + separator + wallPos.z);

		wallPos = exp.environmentController.WallsZNeg.position;
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "Experiment Info" + separator + "ENV_WALL_ZNEG" + separator + wallPos.x + separator + wallPos.y + separator + wallPos.z);
		Debug.Log ("LOGGED ENV");
	}


	//TODO: move to an experiment logger
	public void LogWaitForJitterStarted(float jitter){
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), gameObject.name + separator + "RANDOM_JITTER_STARTED" + separator + jitter);
		Debug.Log ("JITTER STARTED LOGGED: " + jitter);
	}
	
	//TODO: move to an experiment logger
	public void LogWaitForJitterEnded(float jitter){
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), gameObject.name + separator + "RANDOM_JITTER_ENDED" + separator + jitter);
		Debug.Log ("JITTER ENDED LOGGED: " + jitter);
	}


	//if the UI answer selector has moved TODO: move to an answer selector logger?
	public void LogAnswerPositionMoved(bool isYesPosition, bool isRememberResponse){ //either remember response or double down response
		string answerPosition = "NO";
		if (isYesPosition) {
			answerPosition = "YES";
		}

		if(isRememberResponse){
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), gameObject.name + separator + "REMEMBER_ANSWER_MOVEMENT" + separator + answerPosition);
			Debug.Log ("REMEMBER MOVEMENT LOGGED: " + answerPosition);
		}
		else{
			//TODO: CHANGE THE "DOUBLE DOWN" TO ARE YOU SURE OR SOMETHING.
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), gameObject.name + separator + "DOUBLE_DOWN_ANSWER_MOVEMENT" + separator + answerPosition);
			Debug.Log ("DOUBLE DOWN MOVEMENT LOGGED: " + answerPosition);
		}
	}


	//THE FOLLOWING ARE EVENTS

	public void LogPauseEvent(bool isPaused){
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), gameObject.name + separator + "TASK_PAUSED" + separator + isPaused); //logged for replay
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "TASK_PAUSED" + separator + isPaused); //logged for parsing events
		Debug.Log ("Logged pause event. isPaused: " + isPaused);
	}

	public void LogInstructionEvent(){
		if (ExperimentSettings.isLogging) {
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "SHOWING_INSTRUCTIONS");
			Debug.Log ("Logged instruction event.");
		}
	}

	public void LogBeginningExplorationEvent(){
		if (ExperimentSettings.isLogging) {
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "FREE_EXPLORATION_STARTED");
			Debug.Log ("Logged exploration event.");
		}
	}


	public void LogPresentationPhase(bool isStarting){
		if (ExperimentSettings.isLogging) {
			if(isStarting){
				subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "LEARNING_PRESENTATION_PHASE_STARTED");
				Debug.Log ("Logged presentation phase started event.");
			}
			else{
				subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "LEARNING_PRESENTATION_PHASE_ENDED");
				Debug.Log ("Logged presentation phase ended event.");
			}
		}
	}

	public void LogStoreLearningPresentation(Store store, bool isStarting){
		if (ExperimentSettings.isLogging) {
			if(isStarting){
				subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "LEARNING_PRESENTATION_STORE_STARTED" + separator + store.name);
				Debug.Log ("Logged learning presentation store started event.");
			}
			else{
				subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "LEARNING_PRESENTATION_STORE_ENDED" + separator + store.name);
				Debug.Log ("Logged learning presentation store ended event.");
			}
		}
	}

	public void LogLearningIteration(int numLearningPhase){
		if (ExperimentSettings.isLogging) {
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "LEARNING_ITERATION" + separator + numLearningPhase);
			Debug.Log ("Logged learning iteration event.");
		}
	}

	public void LogLearningPhaseStarted(bool isStarting){
		if (ExperimentSettings.isLogging) {
			if(isStarting){
				subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "LEARNING_NAVIGATION_PHASE_STARTED");
				Debug.Log ("Logged learning phase started event.");
			}
			else{
				subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "LEARNING_NAVIGATION_PHASE_ENDED");
				Debug.Log ("Logged learning phase ended event.");
			}
		}
	}

	public void LogSessionStarted (int sessionNum, bool isLearningSession){ //gets logged at the start of all delivery days
		if (ExperimentSettings.isLogging) {
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "SESS_STARTED" + separator + sessionNum + separator + "IS_LEARNING" + separator + isLearningSession);
			Debug.Log ("Logged session started event.");
		}
	}

	//did this session use the LAST SESSION ITEMS LEFT TO USE FILE
	public void LogUseLastSessionItemFile(bool didUseFile){
		if (ExperimentSettings.isLogging) {
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "USED_LAST_SESSION_ITEM_FILE" + separator + didUseFile);
			Debug.Log ("Logged used last session items left to use file.");
		}
	}

	public void LogWayPoints(bool isStarting){ //gets logged at the start of all delivery days
		if (ExperimentSettings.isLogging) {
			if(isStarting){
				subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "WAYPOINTS_ON");
				Debug.Log ("Logged waypoints on event.");
			}
			else{
				subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "WAYPOINTS_OFF");
				Debug.Log ("Logged waypoints on event.");
			}
		}
	}

	public void LogDeliveryDay(int deliveryDayIndex, bool isStarting){
		if (ExperimentSettings.isLogging) {
			if(isStarting){
				subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "DELIVERY_DAY_STARTED" + separator + deliveryDayIndex);
				Debug.Log ("Logged delivery day started event.");
			}
			else{
				subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "DELIVERY_DAY_ENDED" + separator + deliveryDayIndex);
				Debug.Log ("Logged delivery day ended event.");
			}
		}
	}

	public void LogStoreStarted(Store store, bool isLearning, bool isStarting, int serialIndex){ //if it's not learning, it's a delivery!
		if (ExperimentSettings.isLogging) {
			string learningOrDelivery = "learning";
			if(!isLearning){
				learningOrDelivery = "delivery";
			}
			if(isStarting){
				subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "STORE_TARGET_STARTED" + separator + store.name + separator + learningOrDelivery + separator + serialIndex);
				Debug.Log ("Logged store started event.");
			}
			else{
				subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "STORE_TARGET_ENDED" + separator + store.name + separator + learningOrDelivery + separator + serialIndex);
				Debug.Log ("Logged store ended event.");
			}
		}
	}

	public void LogItemDelivery(string itemDelivered, Store storeDeliveredTo, int serialIndex, bool isAudio, bool isStarting){

		string audioOrVisual = "audio"; //if not isAudio, it's visual! (and maybe audio too)
		if (!isAudio) {
			audioOrVisual = "visual";
		}

		if (ExperimentSettings.isLogging) {
			if(isStarting){
				subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "ITEM_DELIVERY_STARTED" + separator + itemDelivered + separator + storeDeliveredTo.name + separator + serialIndex + separator + audioOrVisual
				                + separator + "PLAYER_POSITION" + separator + exp.player.transform.position.x + separator + exp.player.transform.position.y + separator + exp.player.transform.position.z);
				Debug.Log ("Logged item delivered started event.");
			}
			else{
				subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "ITEM_DELIVERY_ENDED" + separator + itemDelivered + separator + storeDeliveredTo.name + separator + serialIndex + separator + audioOrVisual
				                + separator + "PLAYER_POSITION" + separator + exp.player.transform.position.x + separator + exp.player.transform.position.y + separator + exp.player.transform.position.z);
				Debug.Log ("Logged item delivered ended event.");
			}
		}
	}

	
	public void LogCuedRecallPresentation (string cue, string shouldRecall, bool isItemRecall, bool isAudioCue, bool isStarting){

		string audioOrVisual = "audio"; //if not isAudio, it's visual! (and maybe audio too)
		if (!isAudioCue) {
			audioOrVisual = "visual";
		}

		string itemOrStore = "store";
		if (isItemRecall) {
			itemOrStore = "item";
		}
		
		if (ExperimentSettings.isLogging) {
			if(isStarting){
				subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "CUED_PRESENTATION_STARTED" + separator + itemOrStore + separator
				                + "SHOULD_RECALL" + separator + shouldRecall + separator + "CUE" + separator + cue + separator + audioOrVisual);
				Debug.Log ("Logged item presentation started event.");
			}
			else{
				subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "CUED_PRESENTATION_ENDED" + separator + itemOrStore + separator
				                + "SHOULD_RECALL" + separator + shouldRecall + separator + "CUE" + separator + cue + separator + audioOrVisual);
				Debug.Log ("Logged item presentation ended event.");
			}
		}
	}

	public void LogRecallPhaseStarted(Config.RecallType recallType, bool isStarted){

		string eventString = "";
		string itemOrStore = "item";

		switch (recallType) {
		case Config.RecallType.FreeItemRecall:
			eventString = "free";
			break;
		/*case Config.RecallType.FreeStoreRecall:
			eventString = "free";
			itemOrStore = "store";
			break;*/
		case Config.RecallType.CuedRecall:
			eventString = "cued";
			itemOrStore = "item_and_store";
			break;
		case Config.RecallType.FinalItemRecall:
			eventString = "final";
			break;
		case Config.RecallType.FinalStoreRecall:
			eventString = "final";
			itemOrStore = "store";
			break;
		}
		
		if(isStarted){
			eventString += "_STARTED";
		}
		else{
			eventString += "_ENDED";
		}

		if (ExperimentSettings.isLogging) {
			if(isStarted){
				subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "RECALL_PHASE_STARTED" + separator + eventString + separator + itemOrStore);

				Debug.Log ("Logged recall started event.");
			}
			else{
				subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "RECALL_PHASE_ENDED" + separator + eventString + separator + itemOrStore);

				Debug.Log ("Logged recall ended event.");
			}
		}
	}

	public void LogRecording(bool isStarted){
		if (isStarted) {
			subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "AUDIO_RECORDING_STARTED");
			
			Debug.Log ("Logged .wav recording started.");
		} 
		else {
			subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Trial Event" + separator + "AUDIO_RECORDING_ENDED");
			
			Debug.Log ("Logged .wav recording ended.");
		}
	}

}