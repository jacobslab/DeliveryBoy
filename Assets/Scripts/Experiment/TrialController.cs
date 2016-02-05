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

	//store rotation phase
	public Transform storeRotationTransform;
	public VisibilityToggler rotationBackgroundCube;

	int numRealTrials = 0; //used for logging trial ID's
	int numStoresVisited = 0;


	[HideInInspector] public GameObject currentDefaultObject; //current treasure chest we're looking for. assuming a one-by-one reveal.
	

	void Start(){
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
		exp.eventLogger.LogPauseEvent (isPaused);

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
					yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("Welcome to Delivery Day " + i + "/" + Config.numTestTrials + "!" + "\n\nPress [X] to continue.", true, true, false, Config.minDefaultInstructionTime));
				}

				exp.player.controls.ShouldLockControls = false;

				//DELIVERY PHASE
				yield return StartCoroutine(DoStoreDeliveryPhase(i));


				//RECALL PHASE
				//TODO: implement different kinds of recall phases
				yield return StartCoroutine(DoRecallPhase( Config.RecallType.FreeItemRecall, i));

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

		exp.eventLogger.LogLearningPhaseStarted (true);

		for (int numIterations = 0; numIterations < Config.numLearningIterations; numIterations++) {

			exp.eventLogger.LogLearningIteration (numIterations);

			List<Store> storeLearningOrder = exp.storeController.GetLearningOrderStores();

			for (int i = 0; i < storeLearningOrder.Count; i++) {
				yield return StartCoroutine(DoVisitStoreCommand(storeLearningOrder[i]));
			}

			yield return 0;
		}

		exp.eventLogger.LogLearningPhaseStarted (false);
	}

	IEnumerator DoStoreRotationPhase(){
		currentState = TrialState.rotationLearning;

		exp.eventLogger.LogRotationPhase (true);

		rotationBackgroundCube.TurnVisible (true);
		exp.mainCanvas.GetComponent<CanvasGroup> ().alpha = 0.0f;
		//for each store, move it to the rotation location, rotate it for x seconds, return it to its original location
		for (int i = 0; i < exp.storeController.stores.Length; i++) {
			Store currStore = exp.storeController.stores[i];

			exp.eventLogger.LogStoreRotationPresented(currStore, true);

			//move store
			currStore.transform.position = storeRotationTransform.position;
			currStore.transform.rotation = storeRotationTransform.rotation;

			//rotate store
			float currTime = 0.0f;

			float totalDegreesToRotate = Config.numStoreRotations * 360.0f;

			float degPerSecond = totalDegreesToRotate / Config.storeRotateTime;

			while( currTime < Config.storeRotateTime ){
				yield return 0;
				float degToRotate = degPerSecond * Time.deltaTime;
				currStore.transform.RotateAround(currStore.transform.position, Vector3.up, degToRotate);
				currTime += Time.deltaTime;
			}

			//put store back
			currStore.ResetStore();

			exp.eventLogger.LogStoreRotationPresented(currStore, false);
		}
		exp.mainCanvas.GetComponent<CanvasGroup> ().alpha = 1.0f;
		rotationBackgroundCube.TurnVisible (false);

		exp.eventLogger.LogRotationPhase (false);
	}

	IEnumerator DoVisitStoreCommand(Store storeToVisit){
		exp.eventLogger.LogStoreTarget (storeToVisit, true);

		exp.player.controls.ShouldLockControls = false;

		//show instruction at top of screen, don't wait for button, wait for collision
		
		exp.instructionsController.SetSingleInstruction ("Go to the " + storeToVisit.name, false);
		yield return StartCoroutine (exp.player.WaitForStoreCollision (storeToVisit.gameObject, Config.shouldUseWaypoints));

		exp.eventLogger.LogStoreTarget (storeToVisit, false);
	}

	IEnumerator DoStoreDeliveryPhase(int deliveryDay){

		currentState = TrialState.delivery;

		exp.eventLogger.LogDeliveryDay (deliveryDay, true);

		List<Store> deliveryStores = exp.storeController.GetRandomDeliveryStores();

		for (int i = 0; i < deliveryStores.Count; i++) {
			//start delivery timer
			deliveryTimer.StartTimer();
			//visit store
			yield return StartCoroutine(DoVisitStoreCommand(deliveryStores[i]));

			//calculate score, reset the delivery timer
			exp.scoreController.CalculateTimeBonus(deliveryTimer.GetSecondsInt());

			//if not the last delivery, deliver an item.
			if(i < deliveryStores.Count - 1){
				exp.player.controls.ShouldLockControls = true;

				if(Config.isAudioDelivery){
					yield return StartCoroutine(DeliverItemAudio());
				}
				else{
					yield return StartCoroutine(DeliverItemVisible(deliveryStores [i].name));
				}

				exp.player.controls.ShouldLockControls = false;

			}
			//reset timer
			deliveryTimer.ResetTimer();
		}

		exp.eventLogger.LogDeliveryDay (deliveryDay, false);
	}

	IEnumerator DeliverItemVisible(string toStoreName){
		//show delivered item
		GameObject itemDelivered = exp.objectController.SpawnDeliverable(Vector3.zero);
		string itemDisplayText = exp.objectController.GetDeliverableText(itemDelivered);

		string itemName = itemDelivered.GetComponent<SpawnableObject> ().GetName ();
		exp.eventLogger.LogVisibleDeliveryPresentation(itemName, true);

		yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("You delivered " + itemDisplayText + " to the " + toStoreName, true, false, false, Config.deliveryCompleteInstructionsTime));

		exp.eventLogger.LogVisibleDeliveryPresentation(itemName, false);

		Destroy(itemDelivered);
	}

	IEnumerator DeliverItemAudio(){
		GameObject playerCollisionObject = exp.player.GetCollisionObject ();

		if (playerCollisionObject != null) {
		
			//play store audio! as long as it's not the last store.
			if(playerCollisionObject.tag == "Store"){
				Store collisionStore = playerCollisionObject.GetComponent<Store>();
			
				//TRIAL LOGGER LOGS THIS IN PLAYDELIVERYAUDIO() COROUTINE
				yield return StartCoroutine(collisionStore.PlayDeliveryAudio());
			}
		
		}

		yield return 0;
	}

	IEnumerator DoRecallPhase(Config.RecallType recallType, int numRecallPhase){
		currentState = TrialState.recall;

		exp.eventLogger.LogRecallPhaseStarted (recallType, true);

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

		switch(recallType){
			case Config.RecallType.FreeItemRecall:
				yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("Recall as many delivered items as you can.", true, false, false, Config.recallTime));
				break;
			case Config.RecallType.FreeStoreRecall:
				//yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("Recall as many delivered items as you can.", true, false, false, Config.recallTime));
				break;
			case Config.RecallType.ItemCuedRecall:
				//yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("Recall as many delivered items as you can.", true, false, false, Config.recallTime));
				break;
			case Config.RecallType.StoreCuedRecall:
				//yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("Recall as many delivered items as you can.", true, false, false, Config.recallTime));
				break;
			case Config.RecallType.FinalItemRecall:
				//yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("Recall as many delivered items as you can.", true, false, false, Config.recallTime));
				break;
			case Config.RecallType.FinalStoreRecall:
				//yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("Recall as many delivered items as you can.", true, false, false, Config.recallTime));
				break;
		}

		exp.player.controls.ShouldLockControls = false;

		RecallUI.alpha = 0.0f;

		exp.eventLogger.LogRecallPhaseStarted (recallType, false);

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
