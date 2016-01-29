using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class TrialController : MonoBehaviour {
	Experiment exp { get { return Experiment.Instance; } }


	public enum TrialState{
		rotationLearning,
		navigationLearning,
		delivery,
		recall
	}
	public TrialState currentState = TrialState.rotationLearning;


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

	//delivery timer
	public SimpleTimer deliveryTimer;

	//building rotation phase
	public Transform buildingRotationTransform;
	public VisibilityToggler rotationBackgroundCube;

	TrialLogTrack trialLogger;

	int numRealTrials = 0; //used for logging trial ID's
	int numStoresVisited = 0;


	[HideInInspector] public GameObject currentDefaultObject; //current treasure chest we're looking for. assuming a one-by-one reveal.
	

	void Start(){
		trialLogger = GetComponent<TrialLogTrack> ();
		rotationBackgroundCube.TurnVisible (false);
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
			yield return StartCoroutine (exp.instructionsController.PlayStartInstructions());


			//ROTATION PHASE
			if(Config.doRotationPhase){
				yield return StartCoroutine(DoStoreRotationPhase());
			}

			//LEARNING PHASE
			if(Config.doLearningPhase){
				yield return StartCoroutine(DoLearningPhase());
			}

			exp.player.controls.ShouldLockControls = true;
			yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("You will now begin delivering items! Press [X] to start your first delivery day.", true, true, false, Config.minDefaultInstructionTime));

			for(int i = 0; i < Config.numTestTrials; i++){
				exp.player.controls.ShouldLockControls = true;

				if(i != 0){
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

		currentState = TrialState.navigationLearning;

		yield return StartCoroutine(exp.instructionsController.PlayLearningInstructions());

		for (int numIterations = 0; numIterations < Config.numLearningIterations; numIterations++) {

			trialLogger.LogLearningPhaseStarted (numIterations);

			List<Building> buildingsLearningOrder = exp.buildingController.GetLearningOrderBuildings();

			for (int i = 0; i < buildingsLearningOrder.Count; i++) {
				yield return StartCoroutine(DoVisitStoreCommand(buildingsLearningOrder[i]));
			}

			yield return 0;
		}
	}

	IEnumerator DoStoreRotationPhase(){
		currentState = TrialState.rotationLearning;

		rotationBackgroundCube.TurnVisible (true);
		exp.mainCanvas.GetComponent<CanvasGroup> ().alpha = 0.0f;
		//for each building, move it to the rotation location, rotate it for x seconds, return it to its original location
		for (int i = 0; i < exp.buildingController.buildings.Length; i++) {
			Building currBuilding = exp.buildingController.buildings[i];

			//move building
			currBuilding.transform.position = buildingRotationTransform.position;
			currBuilding.transform.rotation = buildingRotationTransform.rotation;

			//rotate building
			float currTime = 0.0f;
			//float timePerRotation = Config.buildingRotateTime / (Config.numBuildingRotations);

			float totalDegreesToRotate = Config.numBuildingRotations * 360.0f;

			float degPerSecond = totalDegreesToRotate / Config.buildingRotateTime;

			while( currTime < Config.buildingRotateTime ){
				yield return 0;
				float degToRotate = degPerSecond * Time.deltaTime;
				currBuilding.transform.RotateAround(currBuilding.transform.position, Vector3.up, degToRotate);
				currTime += Time.deltaTime;
			}

			//put building back
			currBuilding.ResetBuilding();
		}
		exp.mainCanvas.GetComponent<CanvasGroup> ().alpha = 1.0f;
		rotationBackgroundCube.TurnVisible (false);
	}

	IEnumerator DoVisitStoreCommand(Building buildingToVisit){
		exp.player.controls.ShouldLockControls = false;

		//light up path
		exp.waypointController.IlluminateShortestWaypointPath (exp.player.transform.position, buildingToVisit.transform.position);
		Debug.Log (buildingToVisit.name + "Pos: " + buildingToVisit.transform.position);
		//show instruction at top of screen, don't wait for button, wait for collision
		
		exp.instructionsController.SetSingleInstruction ("Go to the " + buildingToVisit.name, false);
		yield return StartCoroutine (exp.player.WaitForObjectCollision (buildingToVisit.name));
	}

	IEnumerator DoStoreDeliveryPhase(int deliveryDay){

		currentState = TrialState.delivery;

		trialLogger.LogDeliveryDayStarted (deliveryDay);

		List<Building> deliveryBuildings = exp.buildingController.GetRandomDeliveryBuildings();

		for (int i = 0; i < deliveryBuildings.Count; i++) {
			//start delivery timer
			deliveryTimer.StartTimer();
			//visit store
			yield return StartCoroutine(DoVisitStoreCommand(deliveryBuildings[i]));

			//calculate score, reset the delivery timer
			exp.scoreController.CalculateTimeBonus(deliveryTimer.GetSecondsInt());

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
			//reset timer
			deliveryTimer.ResetTimer();
		}
	}

	IEnumerator DeliverItemVisible(string toBuildingName){
		//show delivered item
		GameObject itemDelivered = exp.objectController.SpawnDeliverable(Vector3.zero);
		string itemDisplayText = exp.objectController.GetDeliverableText(itemDelivered);
		
		trialLogger.LogDeliveryMade(itemDelivered.GetComponent<SpawnableObject>().GetName());

		yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("You delivered " + itemDisplayText + " to the " + toBuildingName, true, false, false, Config.deliveryCompleteInstructionsTime));
		Destroy(itemDelivered);
	}

	IEnumerator DeliverItemAudio(){
		GameObject playerCollisionObject = exp.player.GetCollisionObject ();

		if (playerCollisionObject != null) {
		
			//play building audio! as long as it's not the last building.
			if(playerCollisionObject.tag == "Building"){
				Building collisionBuilding = playerCollisionObject.GetComponent<Building>();
				yield return StartCoroutine(collisionBuilding.PlayDeliveryAudio());
			}
		
		}

		yield return 0;
	}

	IEnumerator DoRecallPhase(int numRecallPhase){
		currentState = TrialState.recall;

		trialLogger.LogRecallPhaseStarted ();

		RecallUI.alpha = 1.0f;

		recallBeep.Play ();
		while (recallBeep.isPlaying) {
			yield return 0;
		}

		exp.player.controls.ShouldLockControls = true;

		//record audio to a file in the session directory for the duration of the recall period
		string fileName = ExperimentSettings.currentSubject.name + "_" + numRecallPhase;
		if (ExperimentSettings.isLogging) {
			StartCoroutine (exp.audioRecorder.Record (exp.SessionDirectory + "audio", fileName, Config.recallTime));
		}

		yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("Recall as many delivered items as you can.", true, false, false, Config.recallTime));

		exp.player.controls.ShouldLockControls = false;

		RecallUI.alpha = 0.0f;

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
