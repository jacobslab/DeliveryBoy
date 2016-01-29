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
	//public InstructionsController inGameInstructionsController;
	public CameraController cameraController;

	//logging
	private string subjectLogfile; //gets set based on the current subject in Awake()
	public Logger_Threading subjectLog;
	private string eegLogfile; //gets set based on the current subject in Awake()
	public Logger_Threading eegLog;

	public string SessionDirectory { get { return sessionDirectory; } }
	private string sessionDirectory;
	private string subjectDirectory;

	//session controller
	public TrialController trialController;

	//score controller
	public ScoreController scoreController;

	//object controller
	public ObjectController objectController;

	//building controller
	public BuildingController buildingController;

	//environment controller
	public EnvironmentController environmentController;

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

		if (ExperimentSettings.isLogging) {
			InitLogging();
		}
		else if(ExperimentSettings.isReplay) {
			instructionsController.TurnOffInstructions();
			cameraController.SetInGame(); //don't use oculus for replay mode
		}

	}
	
	//TODO: move to logger_threading perhaps? *shrug*
	void InitLogging(){
		subjectDirectory = "TextFiles/" + ExperimentSettings.currentSubject.name + "/";
		sessionDirectory = subjectDirectory + "session000" + "/";;

		int sessionID = 0;
		string sessionIDString = "000";

		if(!Directory.Exists(subjectDirectory)){
			Directory.CreateDirectory(subjectDirectory);
		}
		while (Directory.Exists(sessionDirectory)) {
			if(sessionID < 10){
				sessionIDString = "00" + sessionID;
			}
			else if (sessionID < 100){
				sessionIDString = "0" + sessionID;
			}
			else{
				sessionIDString = sessionID.ToString();
			}
			
			sessionID++;
			
			sessionDirectory = subjectDirectory + "session" + sessionIDString + "/";
		}

		Directory.CreateDirectory(sessionDirectory);

		subjectLog.fileName = sessionDirectory + ExperimentSettings.currentSubject.name + "Log" + ".txt";
		eegLog.fileName = sessionDirectory + ExperimentSettings.currentSubject.name + "EEGLog" + ".txt";
	}

	// Use this for initialization
	void Start () {

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
		player.controls.ShouldLockControls = true;
		yield return StartCoroutine(instructionsController.ShowSingleInstruction("You have finished your trials! \nPress the button to proceed.", true, true, false, 0.0f));
		instructionsController.SetInstructionsColorful(); //want to keep a dark screen before transitioning to the end!
		instructionsController.DisplayText("...loading end screen...");
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
