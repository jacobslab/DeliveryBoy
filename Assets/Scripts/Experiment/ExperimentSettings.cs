using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Reflection;
using System;
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

	public static bool sufficientItemsForDeliveryDay=true;

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

	public Button returnMenuButton;
	public Button quitButton;

	public Slider fovSlider;
	public Text fovVal;


	//LOGGING
	public static string defaultLoggingPath = ""; //SET IN RESETDEFAULTLOGGINGPATH();
	string DB3Folder = "/" + Config.BuildVersion.ToString() + "/";
	public Text defaultLoggingPathDisplay;
	public InputField loggingPathInputField;


	//build info
	public static string buildDate="";

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

		if (SceneManager.GetActiveScene ().name != "EndMenu") {
			InitLoggingPath ();
			InitMainMenuLabels ();
			DoMicTest ();
		} else {
			AttachSceneController ();
		}

		QualitySettings.vSyncCount = 1; //max framerate now = refresh rate of screen! mac = 60hz
	}
	void AttachSceneController()
	{
		GameObject sceneControl = GameObject.Find ("SceneController");
		if (sceneControl != null) {
			returnMenuButton.onClick.RemoveAllListeners ();
			returnMenuButton.onClick.AddListener (() => SceneController.Instance.LoadMainMenu ());

			quitButton.onClick.RemoveAllListeners ();
			quitButton.onClick.AddListener (() => SceneController.Instance.Quit ());
		}
	}
	void ResetDefaultLoggingPath(){
		if (Config.isSystem2) {
			defaultLoggingPath = "/Users/" + System.Environment.UserName + "/RAM_2.0/data";
		}
		else if (Config.isSYS3) {
			defaultLoggingPath = "/Users/" + System.Environment.UserName + "/RAM_3.0/data";
		}
		else if(Config.isSyncbox) {
			defaultLoggingPath = "/Users/" + System.Environment.UserName + "/RAM/data";
		}
		else{
			defaultLoggingPath = System.IO.Directory.GetCurrentDirectory() + "/TextFiles/";
		}	
	}

	public void FoVChanged()
	{
		fovVal.text = fovSlider.value.ToString("F1");
		Config.fieldOfView = fovSlider.value;
	}
	
	void InitLoggingPath(){
		ResetDefaultLoggingPath ();
		
		if(!Directory.Exists(defaultLoggingPath)) {
			Directory.CreateDirectory(defaultLoggingPath);
		}
		
		if(Config.isSyncbox || Config.isSYS3 || Config.isSystem2){ //only add the folder if it's not the demo version.
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

		buildDate = 
			new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime.ToString();
		UnityEngine.Debug.Log (buildDate);
		if (Application.loadedLevel == 0) {
			ExpNameVersion.text = Config.BuildVersion.ToString () + "/" + Config.VersionNumber + "/" + buildDate;
#if EYETRACKER
            BuildType.text = "Eyetracker ";
#else
			BuildType.text="";
#endif
            if (Config.isSyncbox) {
				BuildType.text += "Sync Box";
			} else if (Config.isSystem2) {
				BuildType.text += "System 2";
			}
			else if (Config.isSYS3) {
				BuildType.text += "SYS3.0";
			}
			else {
				BuildType.text += "Demo";
			}
#if HOSPITAL
            BuildType.text+=" (for Hospital)";
#else
            BuildType.text += " (for Scalp Lab)";
#endif

#if GERMAN
			IsGermanText.enabled = true;
#else
            IsGermanText.enabled = false;
#endif
#if FREIBURG
			BuildType.text+=" Freiburg";
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
			UnityEngine.Debug.Log("looking for a previous session!");
			int lastSessionID = Experiment.sessionID - 1;
			if(lastSessionID >= 0){
				//TODO: subtract 1 from the session directory
				UnityEngine.Debug.Log(Experiment.Instance.SessionDirectory);
				string oldSessionDirectory = Experiment.Instance.SessionDirectory.Replace ("session_" + Experiment.sessionID, "session_" + lastSessionID);
				UnityEngine.Debug.Log ("old session:" + oldSessionDirectory);
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
