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
	public UIScreen DeliveryUI; 
	public CanvasGroup RecallUI;

	//audio
	public AudioSource recallBeep;


	TrialLogTrack trialLogger;

	int numRealTrials = 0; //used for logging trial ID's
	int numStoresVisited = 0;


	[HideInInspector] public GameObject currentDefaultObject; //current treasure chest we're looking for. assuming a one-by-one reveal.
	

	void Start(){
		trialLogger = GetComponent<TrialLogTrack> ();
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
			yield return StartCoroutine (exp.instructionsController.PlayStartInstructions());


			//LEARNING PHASE
			if(Config.doLearningPhase){
				yield return StartCoroutine(exp.instructionsController.PlayLearningInstructions());
				yield return StartCoroutine(DoLearningPhase());
			}

			exp.player.controls.ShouldLockControls = true;
			trialLogger.LogInstructionEvent ();
			yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("You will now begin delivering items! Press [X] to start your first delivery day.", true, true, false, Config.minDefaultInstructionTime));

			for(int i = 0; i < Config.numTestTrials; i++){
				exp.player.controls.ShouldLockControls = true;

				if(i != 0){
					trialLogger.LogInstructionEvent ();
					yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("Welcome to Delivery Day " + i + "/" + Config.numTestTrials + "!", true, true, false, Config.minDefaultInstructionTime));
				}

				exp.player.controls.ShouldLockControls = false;

				//DELIVERY PHASE
				yield return StartCoroutine(DoStoreDeliveryPhase(i));


				//RECALL PHASE
				yield return StartCoroutine(DoRecallPhase(i));

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

	IEnumerator DoLearningPhase(){

		for (int numIterations = 0; numIterations < Config.numLearningIterations; numIterations++) {

			trialLogger.LogLearningPhaseStarted (numIterations);

			Building[] buildingsToVisit = exp.buildingController.GetBuildings ();

			for (int i = 0; i < buildingsToVisit.Length; i++) {
				yield return StartCoroutine(DoVisitStoreCommand(buildingsToVisit[i]));
			}

			yield return 0;
		}
	}

	IEnumerator DoVisitStoreCommand(Building buildingToVisit){
		exp.player.controls.ShouldLockControls = false;

		//show instruction at top of screen, don't wait for button, wait for collision
		trialLogger.LogInstructionEvent ();
		exp.instructionsController.SetSingleInstruction ("Go to the " + buildingToVisit.name, false);
		yield return StartCoroutine (exp.player.WaitForCollision (buildingToVisit.name));
	}

	IEnumerator DoStoreDeliveryPhase(int deliveryDay){

		trialLogger.LogDeliveryDayStarted (deliveryDay);

		List<Building> deliveryBuildings = exp.buildingController.GetRandomDeliveryBuildings();

		for (int i = 0; i < deliveryBuildings.Count; i++) {
			//visit store
			yield return StartCoroutine(DoVisitStoreCommand(deliveryBuildings[i]));

			//if not the last delivery, deliver an item.
			if(i < deliveryBuildings.Count - 1){
				exp.player.controls.ShouldLockControls = true;

				if(Config.isAudioDelivery){
					yield return StartCoroutine(DeliverItemAudio());
				}
				else{
					yield return StartCoroutine(DeliverItemVisible(deliveryBuildings [i].name));
				}

				exp.player.controls.ShouldLockControls = false;

			}
		}
	}

	IEnumerator DeliverItemVisible(string toBuildingName){
		//show delivered item
		GameObject itemDelivered = exp.objectController.SpawnDeliverable(Vector3.zero);
		string itemDisplayText = exp.objectController.GetDeliverableText(itemDelivered);
		
		trialLogger.LogDeliveryMade(itemDelivered.GetComponent<SpawnableObject>().GetName());
		
		trialLogger.LogInstructionEvent ();
		yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("You delivered " + itemDisplayText + " to the " + toBuildingName, true, false, false, Config.deliveryCompleteInstructionsTime));
		Destroy(itemDelivered);
	}

	IEnumerator DeliverItemAudio(){
		yield return 0;
	}

	IEnumerator DoRecallPhase(int numRecallPhase){
		trialLogger.LogRecallPhaseStarted ();

		RecallUI.alpha = 1.0f;

		recallBeep.Play ();
		while (recallBeep.isPlaying) {
			yield return 0;
		}

		exp.player.controls.ShouldLockControls = true;
		string recallPath = GetRecallRecordingFilePath (numRecallPhase);
		exp.audioRecorder.Record (GetRecallRecordingFilePath (numRecallPhase));

		trialLogger.LogInstructionEvent ();
		yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("Recall as many delivered items as you can.", true, false, false, Config.recallTime));

		exp.player.controls.ShouldLockControls = false;

		RecallUI.alpha = 0.0f;

		yield return 0;
	}

	string GetRecallRecordingFilePath(int deliveryDay){
		string filePath = exp.SubjectDirectory + ExperimentSettings.currentSubject.name + "_" + deliveryDay;

		return filePath;
	}

	void DestroyGameObjectList(List<GameObject> listOfGameObjects){
		int numObjects = listOfGameObjects.Count;
		for (int i = 0; i < numObjects; i++) {
			Destroy (listOfGameObjects [i]);
		}
		listOfGameObjects.Clear ();
	}
	
}
