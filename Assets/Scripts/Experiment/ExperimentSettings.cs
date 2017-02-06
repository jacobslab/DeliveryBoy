using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class ExperimentSettings : MonoBehaviour { //should be in main menu AND experiment


	Experiment exp { get { return Experiment.Instance; } }



	private static Subject _currentSubject;

	public static Subject currentSubject{ 
		get{ return _currentSubject; } 
		set{ 
			_currentSubject = value;
			//fileName = "TextFiles/" + _currentSubject.name + "Log.txt";
		}
	}

	//subject selection controller
	public SubjectSelectionController subjectSelectionController;

	//TOGGLES
	public static bool isOculus = false;
	public static bool isReplay = false;
	public static bool isLogging = true; //if not in replay mode, should log things! or can be toggled off in main menu.

	public Toggle oculusToggle; //only exists in main menu -- make sure to null check
	public Toggle loggingToggle; //only exists in main menu -- make sure to null check

	public Transform RecallTypeInputParent;


	//SESSION TYPE TOGGLES
	public Toggle learningSessToggle;
	public Toggle delivSessToggle;

	public enum SessionType
	{
		learningSession,
		deliverySession
	}

	public SessionType mySessionType = SessionType.learningSession;

	//NUM DELIV DAY TOGGLES
	public static int numDelivDays = 4; //in main menu, it's 4 by default!
	public Toggle fourDayToggle;
	public Toggle sixDayToggle;
	public Toggle oneDayToggle;


	public Text endCongratsText;
	public Text endScoreText;
	public Text endSessionText;

	public Image micTestIndicator;

	public GameObject nonPilotOptions;
	public bool isRelease { get { return GetIsRelease (); } }


	//LOGGING
	public static string defaultLoggingPath = ""; //SET IN RESETDEFAULTLOGGINGPATH();
	string DB3Folder = "/" + Config.BuildVersion.ToString() + "/";
	public Text defaultLoggingPathDisplay;
	public InputField loggingPathInputField;


	//SINGLETON
	private static ExperimentSettings _instance;
	
	public static ExperimentSettings Instance{
		get{
			return _instance;
		}
	}
	
	void Awake(){
		
		if (_instance != null) {
			Debug.Log("Instance already exists!");
			Destroy(transform.gameObject);
			return;
		}
		_instance = this;

		InitLoggingPath ();
		InitMainMenuLabels ();
		DoMicTest ();

		QualitySettings.vSyncCount = 1; //max framerate now = refresh rate of screen! mac = 60hz
	}

	void ResetDefaultLoggingPath(){
		if (Config.isSystem2) {
			defaultLoggingPath = "/Users/" + System.Environment.UserName + "/RAM_2.0/data/";
		} else if(Config.isSyncbox) {
			defaultLoggingPath = "/Users/" + System.Environment.UserName + "/RAM/data/";
		}
		else{
			defaultLoggingPath = System.IO.Directory.GetCurrentDirectory() + "/TextFiles/";
		}	
	}
	
	void InitLoggingPath(){
		ResetDefaultLoggingPath ();
		
		if(!Directory.Exists(defaultLoggingPath)) {
			Directory.CreateDirectory(defaultLoggingPath);
		}
		
		if(Config.isSyncbox || Config.isSystem2){ //only add the folder if it's not the demo version.
			defaultLoggingPath += DB3Folder; //DB3Folder uses the build version!
		}
		
		if(!Directory.Exists(defaultLoggingPath)){ //if that TH folder doesn't exist, make it!
			Directory.CreateDirectory(defaultLoggingPath);
		}
		
		if (defaultLoggingPathDisplay != null) {
			defaultLoggingPathDisplay.text = defaultLoggingPath;
		}

		/*ResetDefaultLoggingPath ();
		
		if(Directory.Exists(defaultLoggingPath)){
			if (Config.BuildVersion == Config.Version.DBoy3_1) {
				defaultLoggingPath += DB3Folder;
			} 
			
			if(!Directory.Exists(defaultLoggingPath)){ //if that TH folder doesn't exist, make it!
				Directory.CreateDirectory(defaultLoggingPath);
			}
		}
		else{
			defaultLoggingPath = "TextFiles/";
		}
		
		if (defaultLoggingPathDisplay != null) {
			defaultLoggingPathDisplay.text = defaultLoggingPath;
		}*/
	}

	public Text ExpNameVersion;
	public Text BuildType;
	public Text IsGermanText;
	void InitMainMenuLabels(){
		if (Application.loadedLevel == 0) {
			ExpNameVersion.text = Config.BuildVersion.ToString () + "/" + Config.VersionNumber;
#if EYETRACKER
            BuildType.text = "Eyetracker ";
#endif
            if (Config.isSyncbox) {
				BuildType.text += "Sync Box";
			} else if (Config.isSystem2) {
				BuildType.text += "System 2";
			} else {
				BuildType.text += "Demo";
			}

#if GERMAN
			IsGermanText.enabled = true;
#else
			IsGermanText.enabled = false;
#endif
		}
	}

	void DoMicTest(){
		if (micTestIndicator != null) {
			if (AudioRecorder.CheckForRecordingDevice ()) {
				micTestIndicator.color = Color.green;
			} else {
				micTestIndicator.color = Color.red;
			}
		}
	}

	public string GetStoreItemFilePath(bool isCurrentSession){
		if (isCurrentSession) {
			return Experiment.Instance.SessionDirectory + ExperimentSettings.currentSubject.name + "_STORE_AUDIO_LEFT_" + Experiment.sessionID + ".txt";
		} 
		else { //looking for a previous session!
			int lastSessionID = Experiment.sessionID - 1;
			if(lastSessionID >= 0){
				//TODO: subtract 1 from the session directory
				string oldSessionDirectory = Experiment.Instance.SessionDirectory.Replace ("_" + Experiment.sessionID, "_" + lastSessionID);
				return oldSessionDirectory + ExperimentSettings.currentSubject.name + "_STORE_AUDIO_LEFT_" + lastSessionID + ".txt";
			}
			else{
				Debug.Log("CAN'T ACCESS A NEGATIVE SESSION");
				return null;
			}
		}
	}

	// Use this for initialization
	void Start () {
		SetOculus();
		//SetSystem2();
		//SetSyncBox();
		if(Application.loadedLevelName == "EndMenu"){
			if(currentSubject != null){
				endCongratsText.text = "Congratulations " + currentSubject.name + "!";
				endScoreText.text = currentSubject.score.ToString();
				endSessionText.text = currentSubject.trials.ToString();
			}
			else{
				Debug.Log("Current subject is null!");
			}
		}
	}

	bool GetIsRelease(){
		if (nonPilotOptions.activeSelf) {
			return false;
		}
		return true;
	}

	public void SetReplayTrue(){
		isReplay = true;
		isLogging = false;
		loggingToggle.isOn = false;
	}


	public void SetReplayFalse(){
		isReplay = false;
		//shouldLog = true;
	}

	public void SetLogging(){
		if(isReplay){
			isLogging = false;
		}
		else{
			if(loggingToggle){
				isLogging = loggingToggle.isOn;
				Debug.Log("should log?: " + isLogging);
			}
		}

	}

	public void SetOculus(){
		if(oculusToggle){
			isOculus = oculusToggle.isOn;
		}
	}

	public void SetDeliveryDayNum(int num){

		switch (num) {
		case 1:
			if(oneDayToggle.isOn){ //only set things if this toggle was just turned on!
				numDelivDays = num;
				oneDayToggle.isOn = true;
				sixDayToggle.isOn = false;
				fourDayToggle.isOn = false;
			}
			break;
		case 4:
			if(fourDayToggle.isOn){
				numDelivDays = num;
				oneDayToggle.isOn = false;
				sixDayToggle.isOn = false;
				fourDayToggle.isOn = true;
			}
			break;
		case 6:
			if(sixDayToggle.isOn){
				numDelivDays = num;
				oneDayToggle.isOn = false;
				sixDayToggle.isOn = true;
				fourDayToggle.isOn = false;
			}
			break;
		}
	}

	public void SetSessionType(int type){

		switch (type) {
		case 0: //learning
			if(learningSessToggle.isOn){ //have to check if the toggle is on or we'll get an infinite loop of changing toggles!
				mySessionType = SessionType.learningSession;
				delivSessToggle.isOn = false; //set the other toggle to false
			}
			else{ //in case we toggled off the learning toggle...
				mySessionType = SessionType.deliverySession;
				delivSessToggle.isOn = true; //set the other toggle to false
			}
			break;
		case 1: //delivery
			if(delivSessToggle.isOn){//have to check if the toggle is on or we'll get an infinite loop of changing toggles!
				mySessionType = (SessionType)System.Enum.Parse(typeof(SessionType), type.ToString());
				learningSessToggle.isOn = false; //set the other toggle to false
			}
			else{ //in case we toggled off the deliv toggle...
				mySessionType = SessionType.learningSession;
				learningSessToggle.isOn = true; //set the other toggle to false
			}
			break;
		}

		Debug.Log("session type changed to: " + mySessionType.ToString());
	}

	public void SetTrialRecallTypes(){
		NumericInputTextLimiter[] recallTextInputs = RecallTypeInputParent.GetComponentsInChildren<NumericInputTextLimiter> ();
		for (int i = 0; i < recallTextInputs.Length; i++) {
			if(i < numDelivDays){
				recallTextInputs[i].UpdateText();
				int recallTextTypeInt = int.Parse (recallTextInputs[i].myNumericTextField.text);

				Config.RecallTypesAcrossTrials[i] = (Config.RecallType)recallTextTypeInt;
				
			}
		}

	}

	/*public void SetSystem2(){
		if(system2Toggle){
			isSystem2 = system2Toggle.isOn;
		}
	}

	public void SetSyncBox(){
		if(syncboxToggle){
			isSyncbox = syncboxToggle.isOn;
		}
	}*/
	
}
