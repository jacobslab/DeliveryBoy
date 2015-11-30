using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class TrialController : MonoBehaviour {
	Experiment exp { get { return Experiment.Instance; } }

	//hardware connection
	bool isConnectingToHardware = false;

	//paused?!
	public static bool isPaused = false;

	//UI
	public CanvasGroup PauseUI;
	public CanvasGroup ConnectionUI;

	TrialLogTrack trialLogger;

	bool isPracticeTrial = false;
	int numRealTrials = 0; //used for logging trial ID's

	int numStoresVisited = 0;

	Trial currentTrial;
	Trial practiceTrial;

	[HideInInspector] public GameObject currentDefaultObject; //current treasure chest we're looking for. assuming a one-by-one reveal.

	List<List<Trial>> ListOfTrialBlocks;

	void Start(){
		InitTrials ();
		trialLogger = GetComponent<TrialLogTrack> ();
	}

	void InitTrials(){
		ListOfTrialBlocks = new List<List<Trial>> ();

		int numTestTrials = Config.numTestTrials;

		int numTrialsPerBlock = (int)(Config.trialBlockDistribution [0] + Config.trialBlockDistribution [1]);

		if (numTestTrials % numTrialsPerBlock != 0) {
			Debug.Log("CANNOT EXECUTE THIS TRIAL DISTRIBUTION");
		}

		int numTrialBlocks = numTestTrials / numTrialsPerBlock;
		for (int i = 0; i < numTrialBlocks; i++) {
			ListOfTrialBlocks.Add(GenerateTrialBlock());
		}

		if(Config.doPracticeTrial){
			practiceTrial = new Trial();	//2 special objects for practice trial
		}

	}

	List<Trial> GenerateTrialBlock(){
		List<Trial> trialBlock = new List<Trial> ();

		for (int i = 0; i < Config.numTestTrials / 2; i++) { //divide by two because we're adding a regular and a counterbalanced trial

			Trial trial = new Trial();
			Trial counterTrial = trial.GetCounterSelf();
			
			trialBlock.Add(trial);
			trialBlock.Add(counterTrial);
		}

		return trialBlock;

	}

	Trial PickRandomTrial(List<Trial> trialBlock){
		if (trialBlock.Count > 0) {
			int randomTrialIndex = Random.Range (0, trialBlock.Count);
			Trial randomTrial = trialBlock [randomTrialIndex];

			trialBlock.RemoveAt (randomTrialIndex);
			return randomTrial;
		} 
		else {
			Debug.Log("No more trials left!");
			return null;
		}
	}


	
	void Update(){
		if(!isConnectingToHardware){
			GetPauseInput ();
		}
	}

	bool isPauseButtonPressed = false;
	void GetPauseInput(){
		//if (Input.GetAxis ("Pause Button") > 0) {
		if(Input.GetKeyDown(KeyCode.B) || Input.GetKey(KeyCode.JoystickButton2)){ //B JOYSTICK BUTTON TODO: move to input manager.
			Debug.Log("PAUSE BUTTON PRESSED");
			if(!isPauseButtonPressed){
				isPauseButtonPressed = true;
				Debug.Log ("PAUSE OR UNPAUSE");
				TogglePause (); //pause
			}
		} 
		else{
			isPauseButtonPressed = false;
		}
	}

	public void TogglePause(){
		isPaused = !isPaused;
		trialLogger.LogPauseEvent (isPaused);

		if (isPaused) {
			//exp.player.controls.Pause(true);
			PauseUI.alpha = 1.0f;
			Time.timeScale = 0.0f;
		} 
		else {
			Time.timeScale = 1.0f;
			//exp.player.controls.Pause(false);
			//exp.player.controls.ShouldLockControls = false;
			PauseUI.alpha = 0.0f;
		}
	}


	//FILL THIS IN DEPENDING ON EXPERIMENT SPECIFICATIONS
	public IEnumerator RunExperiment(){
		if (!ExperimentSettings.isReplay) {
			exp.player.controls.ShouldLockControls = true;

			if(ExperimentSettings.isSystem2 || ExperimentSettings.isSyncbox){
				yield return StartCoroutine( WaitForEEGHardwareConnection() );
			}
			else{
				ConnectionUI.alpha = 0.0f;
			}

			//show instructions for exploring, wait for the action button
			trialLogger.LogInstructionEvent();
			yield return StartCoroutine (exp.ShowSingleInstruction (Config.initialInstructions1, true, true, false, Config.minInitialInstructionsTime));
			yield return StartCoroutine (exp.ShowSingleInstruction (Config.initialInstructions2, true, true, false, Config.minInitialInstructionsTime));

			//let player explore until the button is pressed again
			//trialLogger.LogBeginningExplorationEvent();
			//yield return StartCoroutine (exp.WaitForActionButton ());
			
			//get the number of blocks so far -- floor half the number of trials recorded
			int totalTrialCount = ExperimentSettings.currentSubject.trials;
			numRealTrials = totalTrialCount;
			if (Config.doPracticeTrial) {
				if (numRealTrials >= 2) { //otherwise, leave numRealTrials at zero.
					numRealTrials -= 1; //-1 for practice trial
				}
			}

			
			//run practice trials
			if(Config.doPracticeTrial){
				isPracticeTrial = true;
			}
			
			if (isPracticeTrial) {

				yield return StartCoroutine (RunTrial ( practiceTrial ));

				Debug.Log ("PRACTICE TRIALS COMPLETED");
				totalTrialCount += 1;
				isPracticeTrial = false;
			}


			//RUN THE REST OF THE BLOCKS
			for( int i = 0; i < ListOfTrialBlocks.Count; i++){
				List<Trial> currentTrialBlock = ListOfTrialBlocks[i];
				while (currentTrialBlock.Count > 0) {
					Trial nextTrial = PickRandomTrial (currentTrialBlock);

					yield return StartCoroutine (RunTrial ( nextTrial ));

					totalTrialCount += 1;

					Debug.Log ("TRIALS COMPLETED: " + totalTrialCount);
				}

				//FINISHED A TRIAL BLOCK, SHOW UI

				exp.scoreController.Reset();

				Debug.Log ("TRIAL Block: " + i);
			}

			yield return 0;
		}
		
	}

	IEnumerator WaitForEEGHardwareConnection(){
		isConnectingToHardware = true;

		ConnectionUI.alpha = 1.0f;
		if(ExperimentSettings.isSystem2){
			while(!TCPServer.Instance.isConnected || !TCPServer.Instance.canStartGame){
				Debug.Log("Waiting for system 2 connection...");
				yield return 0;
			}
		}
		else if (ExperimentSettings.isSyncbox){
			while(!SyncboxControl.Instance.isUSBOpen){
				Debug.Log("Waiting for sync box to open...");
				yield return 0;
			}
		}
		ConnectionUI.alpha = 0.0f;
		isConnectingToHardware = false;
	}
	

	//INDIVIDUAL TRIALS -- implement for repeating the same thing over and over again
	//could also create other IEnumerators for other types of trials
	IEnumerator RunTrial(Trial trial){

		currentTrial = trial;

		if (isPracticeTrial) {
			trialLogger.Log (-1, currentTrial.DefaultObjectLocationsXZ.Count);
			Debug.Log("Logged practice trial.");
		} 
		else {
			trialLogger.Log (numRealTrials, currentTrial.DefaultObjectLocationsXZ.Count);
			numRealTrials++;
			Debug.Log("Logged trial #: " + numRealTrials);
		}


		//START NAVIGATION
		trialLogger.LogTrialNavigationStarted ();

		//unlock avatar controls
		exp.player.controls.ShouldLockControls = false;

		//wait for player to collect all default objects
		int numDefaultObjectsToCollect = currentTrial.DefaultObjectLocationsXZ.Count;
		while (numStoresVisited < numDefaultObjectsToCollect) {
			yield return 0;
		}

		//reset num default objects collected
		numStoresVisited = 0;

		//lock player movement
		exp.player.controls.ShouldLockControls = true;



		//jitter before the first object is shown
		yield return StartCoroutine(exp.WaitForJitter(Config.randomJitterMin, Config.randomJitterMax));

		//show instructions for location selection 
		trialLogger.LogRecallPhaseStarted();
		
		//increment subject's trial count
		ExperimentSettings.currentSubject.IncrementTrial ();

	}

	IEnumerator ShowFeedback(List<int> specialObjectOrder, List<Vector3> chosenPositions, List<bool> rememberResponses, List<bool> areYouSureResponses){

		yield return 0;
	}
	
	void DestroyGameObjectList(List<GameObject> listOfGameObjects){
		int numObjects = listOfGameObjects.Count;
		for (int i = 0; i < numObjects; i++) {
			Destroy (listOfGameObjects [i]);
		}
		listOfGameObjects.Clear ();
	}
	
}
