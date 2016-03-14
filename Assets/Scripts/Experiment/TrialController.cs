using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class TrialController : MonoBehaviour {
	Experiment exp { get { return Experiment.Instance; } }

	List<Store> orderedStores;
	List<string> orderedItemsDelivered;

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
		orderedStores = new List<Store>();
		orderedItemsDelivered = new List<string>();
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

			if(Config.isSystem2 || Config.isSyncbox){
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


			exp.eventLogger.LogSessionStarted(Experiment.sessionID);

			exp.player.controls.ShouldLockControls = true;
			yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("You will now begin delivering items! Press [X] to start your first delivery day.", true, true, false, Config.minDefaultInstructionTime));

			for(int i = 0; i < Config.numTestTrials; i++){
				exp.player.controls.ShouldLockControls = true;

				if(i != 0){
					yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("Welcome to Delivery Day " + i + "/" + Config.numTestTrials + "!" + "\n\nPress [X] to continue.", true, true, false, Config.minDefaultInstructionTime));
				}

				exp.player.controls.ShouldLockControls = false;

				//DELIVERY DAY
				orderedStores.Clear();
				orderedItemsDelivered.Clear();
				yield return StartCoroutine(DoStoreDeliveryPhase(i));


				//RECALL
				//TODO: implement different kinds of recall phases
				Config.RecallType recallType = Config.RecallTypesAcrossTrials[i];
				yield return StartCoroutine(DoRecallPhase( recallType, i));

			}


			yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("You have finished your deliveries! \n\n Please press (X) to continue on to final recall.", true, true, false, Config.minDefaultInstructionTime));


			//FINAL RECALL
			if(Config.doFinalItemRecall){
				yield return StartCoroutine(DoRecallPhase(Config.RecallType.FinalItemRecall, Config.numTestTrials));
			}

			if(Config.doFinalStoreRecall){
				yield return StartCoroutine(DoRecallPhase(Config.RecallType.FinalStoreRecall, Config.numTestTrials + 1));
			}



			yield return 0;
		}
		
	}

	IEnumerator WaitForEEGHardwareConnection(){
		isConnectingToHardware = true;

		ConnectionUI.alpha = 1.0f;
		if(Config.isSystem2){
			while(!TCPServer.Instance.isConnected || !TCPServer.Instance.canStartGame){
				Debug.Log("Waiting for system 2 connection...");
				yield return 0;
			}
		}
		else if (Config.isSyncbox){
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
				yield return StartCoroutine(DoVisitStoreCommand(storeLearningOrder[i], true, -1));
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
			currStore.SetVisualsForRotation();
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

	IEnumerator DoVisitStoreCommand(Store storeToVisit, bool isLearning, int numDeliveryToday){ //if it's not learning, it's a delivery!
		exp.eventLogger.LogStoreStarted (storeToVisit, isLearning, true, numDeliveryToday);

		exp.player.controls.ShouldLockControls = false;

		//show instruction at top of screen, don't wait for button, wait for collision
		
		exp.instructionsController.SetSingleInstruction ("Go to the " + storeToVisit.name, false);
		yield return StartCoroutine (exp.player.WaitForStoreCollision (storeToVisit.gameObject));

		exp.eventLogger.LogStoreStarted (storeToVisit, isLearning, false, numDeliveryToday);
	}

	IEnumerator DoStoreDeliveryPhase(int deliveryDay){

		currentState = TrialState.delivery;

		exp.eventLogger.LogDeliveryDay (deliveryDay, true);

		List<Store> deliveryStores = exp.storeController.GetRandomDeliveryStores();

		for (int numDelivery = 0; numDelivery < deliveryStores.Count; numDelivery++) {
			//start delivery timer
			deliveryTimer.StartTimer();
			//visit store
			yield return StartCoroutine(DoVisitStoreCommand(deliveryStores[numDelivery], false, numDelivery));

			//calculate score, reset the delivery timer
			exp.scoreController.CalculateTimeBonus(deliveryTimer.GetSecondsInt());

			//if not the last delivery, deliver an item.
			if(numDelivery < deliveryStores.Count - 1){
				exp.player.controls.ShouldLockControls = true;

				if(Config.isAudioDelivery){
					yield return StartCoroutine(DeliverItemAudio(numDelivery));
				}
				else{
					yield return StartCoroutine(DeliverItemVisible(deliveryStores [numDelivery], numDelivery));
				}

				exp.player.controls.ShouldLockControls = false;

			}
			//reset timer
			deliveryTimer.ResetTimer();
		}

		exp.eventLogger.LogDeliveryDay (deliveryDay, false);
	}

	IEnumerator DeliverItemVisible(Store toStore, int numDelivery){
		//show delivered item
		GameObject itemDelivered = exp.objectController.SpawnDeliverable(Vector3.zero);
		string itemDisplayText = exp.objectController.GetDeliverableText(itemDelivered);

		string itemName = itemDelivered.GetComponent<SpawnableObject> ().GetName ();
		exp.eventLogger.LogItemDelivery(itemName, toStore, numDelivery, false, true);

		yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("You delivered " + itemDisplayText + " to the " + toStore.name, true, false, false, Config.deliveryCompleteInstructionsTime));

		exp.eventLogger.LogItemDelivery(itemName, toStore, numDelivery, false, false);

		Destroy(itemDelivered);

		orderedStores.Add(toStore);
		orderedItemsDelivered.Add(itemName);
	}

	IEnumerator DeliverItemAudio(int numDelivery){
		GameObject playerCollisionObject = exp.player.GetCollisionObject ();

		if (playerCollisionObject != null) {
		
			//play store audio! as long as it's not the last store.
			if(playerCollisionObject.tag == "Store"){
				Store collisionStore = playerCollisionObject.GetComponent<Store>();

				//TRIAL LOGGER LOGS THIS IN PLAYDELIVERYAUDIO() COROUTINE
				yield return StartCoroutine(collisionStore.PlayDeliveryAudio(numDelivery));
				string item = collisionStore.GetComponent<AudioSource>().clip.name;

				orderedStores.Add(collisionStore);
				orderedItemsDelivered.Add(item);
			}
		
		}

		yield return 0;
	}

	IEnumerator DoRecallPhase(Config.RecallType recallType, int numRecallPhase){

		currentState = TrialState.recall;

		exp.eventLogger.LogRecallPhaseStarted (recallType, true);

		RecallUI.alpha = 1.0f;

		exp.player.controls.ShouldLockControls = true;

		//record audio to a file in the session directory for the duration of the recall period
		string fileName = ExperimentSettings.currentSubject.name + "_" + numRecallPhase;

		int recallTime = 0;

		switch(recallType){
			case Config.RecallType.FreeItemRecall:
				recallTime = Config.freeRecallTime;
				exp.recallInstructionsController.DisplayText ("Free recall DELIVERED ITEMS");
				break;
			case Config.RecallType.FreeStoreRecall:
				recallTime = Config.freeRecallTime;
				exp.recallInstructionsController.DisplayText ("Free recall STORES DELIVERED TO");
				break;
			case Config.RecallType.CuedRecall:
				yield return StartCoroutine( DoCuedRecall (fileName));
			break;
			case Config.RecallType.FinalItemRecall:
				recallTime = Config.finalFreeRecallTime;
				exp.recallInstructionsController.DisplayText ("Free recall ALL DELIVERED ITEMS");
				break;
			case Config.RecallType.FinalStoreRecall:
				recallTime = Config.finalFreeRecallTime;
				exp.recallInstructionsController.DisplayText ("Free recall ALL STORES");
				break;
		}

		//only record here if free recall! cued recall recording handled within DoCuedRecall()
		if(recallType != Config.RecallType.CuedRecall){

			recallBeep.Play ();
			while (recallBeep.isPlaying) {
				yield return 0;
			}

			if (ExperimentSettings.isLogging) {
				yield return StartCoroutine (exp.audioRecorder.Record (exp.SessionDirectory + "audio", fileName, recallTime));
			}
			else{
				yield return new WaitForSeconds(recallTime);
			}
		}

		exp.player.controls.ShouldLockControls = false;

		RecallUI.alpha = 0.0f;

		exp.eventLogger.LogRecallPhaseStarted (recallType, false);
	}
	
	IEnumerator DoCuedRecall(string recordFileName){
		//go through all item-store pairs, and cue half with the store and half with the item

		//randomize order of indices
		// want to use number of items delivered, because we don't deliver to the last store
		List<int> randomIndexOrder = UsefulFunctions.GetRandomIndexOrder(orderedItemsDelivered.Count);

		string cueName = "";

		for(int i = 0; i < randomIndexOrder.Count; i++){
			int index = randomIndexOrder[i];

			GameObject storeImage = null;

			//if divisible by 2, make it store cued
			if(index % 2 == 0){
				cueName = orderedStores[index].name;

				exp.eventLogger.LogRecallStorePresentation(cueName, true, true);

				exp.recallInstructionsController.DisplayText ("What did you deliver to the " + cueName + "?");

				//show image
				//storeImage = exp.objectController.SpawnStoreImage(Vector3.zero, cueName);
				storeImage = exp.objectController.GetStoreImage(cueName);
				if(storeImage != null){
					storeImage.SetActive(true);
				}

				exp.eventLogger.LogRecallStorePresentation(cueName, true, false);
			}
			else{	//item cued
				cueName = orderedItemsDelivered[index];

				cueName.Replace("-", " ");

				exp.eventLogger.LogRecallItemPresentation(cueName, true, true);

				exp.recallInstructionsController.DisplayText ("Where did you deliver the " + cueName + "?");

				//play audio
				orderedStores[index].PlayCurrentAudio();

				exp.eventLogger.LogRecallItemPresentation(cueName, true, false);
			}

			recallBeep.Play ();
			while (recallBeep.isPlaying) {
				yield return 0;
			}

			if (ExperimentSettings.isLogging) {
				yield return StartCoroutine (exp.audioRecorder.Record (exp.SessionDirectory + "audio", recordFileName + "_" + cueName, Config.cuedRecallTime));
			}
			else{
				yield return new WaitForSeconds(Config.cuedRecallTime);
			}

			if(storeImage != null){
				storeImage.SetActive(false);
				//Destroy(storeImage);
			}
		}

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
