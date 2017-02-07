using UnityEngine;
using System.Collections;
using System.IO;

public class Experiment : MonoBehaviour {

	//audio recorder
	public AudioRecorder audioRecorder;

	//juice controller
	public JuiceController juiceController;

	//instructions
	public InstructionsController instructionsController;
	public InstructionsController recallInstructionsController;
	//public InstructionsController inGameInstructionsController;
	public CameraController cameraController;

	//logging
	private string subjectLogfile; //gets set based on the current subject in Awake()
	public Logger_Threading subjectLog;
	private string eegLogfile; //gets set based on the current subject in Awake()
	public Logger_Threading eegLog;
	public static int sessionID;

	public string SessionDirectory;
	public static string sessionStartedFileName = "sessionStarted.txt";

	//event logger!
	public TrialLogTrack eventLogger;

	//session controller
	public TrialController trialController;

	//score controller
	public ScoreController scoreController;

	//object controller
	public ObjectController objectController;

	//store controller
	public StoreController storeController;

	//environment controller
	public EnvironmentController environmentController;

	public GameObject eyetrackingComponents;

	//avatar
	public Player player;

	//waypoint controller
	public WaypointController waypointController;

	//Canvas
	public Canvas mainCanvas;

	//public bool isOculus = false;

	//state enum
	public ExperimentState currentState = ExperimentState.instructionsState;

	public enum ExperimentState
	{
		instructionsState,
		inExperiment,
		inExperimentOver,
	}

	//bools for whether we have started the state coroutines
	bool isRunningInstructions = false;
	bool isRunningExperiment = false;


	//EXPERIMENT IS A SINGLETON
	private static Experiment _instance;

	public static Experiment Instance{
		get{
			return _instance;
		}
	}

	void Awake(){
		if (_instance != null) {
			Debug.Log("Instance already exists!");
			return;
		}
		_instance = this;

		juiceController.Init ();

		cameraController.SetInGame();

		if (ExperimentSettings.isLogging) {
			InitLogging();
#if !EYETRACKER
            EnableEyetrackerComponents (false);
#else
            EnableEyetrackerComponents(true);
#endif
        }
		else if (ExperimentSettings.isReplay) {
			instructionsController.TurnOffInstructions();
		}

		eventLogger = GetComponent<TrialLogTrack> ();
	}
	
	//TODO: move to logger_threading perhaps? *shrug*
	void InitLogging(){

		string subjectDirectory = ExperimentSettings.defaultLoggingPath + ExperimentSettings.currentSubject.name + "/";
		if (ExperimentSettings.Instance.mySessionType == ExperimentSettings.SessionType.learningSession) {
			SessionDirectory = subjectDirectory + "session_learning" + "/";
		} else {
			SessionDirectory = subjectDirectory + "session_0" + "/";
		}

		sessionID = 0;
		string sessionIDString = "_0";
		
		if(!Directory.Exists(subjectDirectory)){
			Directory.CreateDirectory(subjectDirectory);
		}
		while (File.Exists(SessionDirectory + sessionStartedFileName)){
			sessionID++;
			
			sessionIDString = "_" + sessionID.ToString();


			if (ExperimentSettings.Instance.mySessionType == ExperimentSettings.SessionType.learningSession) {
				SessionDirectory = subjectDirectory + "session_learning_duplicate/";
				break;
			} else {
				SessionDirectory = subjectDirectory + "session" + sessionIDString + "/";
			}
		}
		
		Directory.CreateDirectory(SessionDirectory);
		
		
		//delete old files.
		if(Directory.Exists(SessionDirectory)){
			DirectoryInfo info = new DirectoryInfo(SessionDirectory);
			FileInfo[] fileInfo = info.GetFiles();
			for(int i = 0; i < fileInfo.Length; i++){
				File.Delete(fileInfo[i].ToString());
			}
		}
		else{ //if directory didn't exist, make it!
			Directory.CreateDirectory(SessionDirectory);
		}
		
		subjectLog.fileName = SessionDirectory + "log" + ".txt";
		eegLog.fileName = SessionDirectory + "eeglog" + ".txt";
	}


	//In order to increment the session, this file must be present. Otherwise, the session has not actually started.
	//This accounts for when we don't successfully connect to hardware -- wouldn't want new session folders.
	//Gets created in TrialController after any hardware has connected.
	public void CreateSessionStartedFile(){
		StreamWriter newSR = new StreamWriter (SessionDirectory + sessionStartedFileName);
	}


	// Use this for initialization
	void Start () {

	}

	public void EnableEyetrackerComponents(bool shouldEnable)
	{
		eyetrackingComponents.SetActive(shouldEnable);
	}

	// Update is called once per frame
	void Update () {
		//Proceed with experiment if we're not in REPLAY mode
		if (!ExperimentSettings.isReplay) { //REPLAY IS HANDLED IN REPLAY.CS VIA LOG FILE PARSING

			if (currentState == ExperimentState.instructionsState && !isRunningInstructions) {
				Debug.Log("running instructions");

				StartCoroutine(RunInstructions());

			}
			else if (currentState == ExperimentState.inExperiment && !isRunningExperiment) {
				Debug.Log("running experiment");
				StartCoroutine(BeginExperiment());
			}

		}
	}

	public IEnumerator RunOutOfTrials(){
		instructionsController.SetInstructionsColorful(); //want to keep a dark screen before transitioning to the end!
#if GERMAN
		instructionsController.DisplayText("Endbildschirm wird geladen...");
#else
		instructionsController.DisplayText("...loading end screen...");
#endif
		EndExperiment();

		yield return 0;
	}

	public IEnumerator RunInstructions(){
		isRunningInstructions = true;

		//IF THERE ARE ANY PRELIMINARY INSTRUCTIONS YOU WANT TO SHOW BEFORE THE EXPERIMENT STARTS, YOU COULD PUT THEM HERE...

		currentState = ExperimentState.inExperiment;
		isRunningInstructions = false;

		yield return 0;

	}


	public IEnumerator BeginExperiment(){
		isRunningExperiment = true;

		yield return StartCoroutine(trialController.RunExperiment());
		
		yield return StartCoroutine(RunOutOfTrials()); //calls EndExperiment()

		yield return 0;

	}

	public void EndExperiment(){
		Debug.Log ("Experiment Over");
		currentState = ExperimentState.inExperimentOver;
		isRunningExperiment = false;
		
		SceneController.Instance.LoadEndMenu();
	}



	public void OnExit(){ //call in scene controller when switching to another scene!
		if (ExperimentSettings.isLogging) {
			subjectLog.close ();
			eegLog.close ();
		}
	}

	void OnApplicationQuit(){
		if (ExperimentSettings.isLogging) {
			subjectLog.close ();
			eegLog.close ();
		}
	}


}
