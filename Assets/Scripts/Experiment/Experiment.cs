using UnityEngine;
using System.Collections;
using System.IO;

public class Experiment : MonoBehaviour {

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

	//session controller
	public TrialController trialController;

	//score controller
	public ScoreController scoreController;

	//object controller
	public ObjectController objectController;

	//environment controller
	public EnvironmentController environmentController;

	//avatar
	public Player player;

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
		subjectLogfile = "TextFiles/" + ExperimentSettings.currentSubject.name + "Log";
		eegLogfile = "TextFiles/" + ExperimentSettings.currentSubject.name + "EEGLog";
		
		int logFileID = 0;
		string logFileIDString = "000";

		while(File.Exists(subjectLog.fileName) || logFileID == 0){
			//TODO: move this function somewhere else...?
			if(logFileID < 10){
				logFileIDString = "00" + logFileID;
			}
			else if (logFileID < 100){
				logFileIDString = "0" + logFileID;
			}
			else{
				logFileIDString = logFileID.ToString();
			}

			subjectLog.fileName = subjectLogfile + "_" + logFileIDString + ".txt";
			eegLog.fileName = eegLogfile + "_" + logFileIDString + ".txt";

			logFileID++;
		}
	}

	// Use this for initialization
	void Start () {
		//Config_CoinTask.Init();
		//inGameInstructionsController.DisplayText("");
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
		/*while(environmentMap.IsActive){
			yield return 0; //thus, should wait for the button press before ending the experiment
		}*/
		
		yield return StartCoroutine(ShowSingleInstruction("You have finished your trials! \nPress the button to proceed.", true, true, false, 0.0f));
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

	//TODO: move to instructions controller...
	public IEnumerator ShowSingleInstruction(string line, bool isDark, bool waitForButton, bool addRandomPostJitter, float minDisplayTimeSeconds){
		if(isDark){
			instructionsController.SetInstructionsColorful();
		}
		else{
			instructionsController.SetInstructionsTransparentOverlay();
		}
		instructionsController.DisplayText(line);

		yield return new WaitForSeconds (minDisplayTimeSeconds);

		if (waitForButton) {
			yield return StartCoroutine (WaitForActionButton ());
		}

		if (addRandomPostJitter) {
			yield return StartCoroutine(WaitForJitter ( Config_CoinTask.randomJitterMin, Config_CoinTask.randomJitterMax ) );
		}

		instructionsController.TurnOffInstructions ();
		cameraController.SetInGame();
	}
	
	public IEnumerator WaitForActionButton(){
		bool hasPressedButton = false;
		while(Input.GetAxis("Action Button") != 0f){
			yield return 0;
		}
		while(!hasPressedButton){
			if(Input.GetAxis("Action Button") == 1.0f){
				hasPressedButton = true;
			}
			yield return 0;
		}
	}

	public IEnumerator WaitForJitter(float minJitter, float maxJitter){
		float randomJitter = Random.Range(minJitter, maxJitter);
		trialController.GetComponent<TrialLogTrack>().LogWaitForJitterStarted(randomJitter);
		
		float currentTime = 0.0f;
		while (currentTime < randomJitter) {
			currentTime += Time.deltaTime;
			yield return 0;
		}

		trialController.GetComponent<TrialLogTrack>().LogWaitForJitterEnded(currentTime);
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
