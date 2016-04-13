using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine.UI;

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
	public CanvasGroup InitialDeliveryInstructionGroup;
	public Text InitialDeliveryInstructionText;
	public Text DeliveryInstructionText;

	//audio
	public AudioSource recallStartBeep;
	public AudioSource recallEndBeep;

	//delivery timer
	public SimpleTimer deliveryTimer;

	//store rotation phase
	public Transform storeRotationTransform;
	public VisibilityToggler rotationBackgroundCube;

	int numRealTrials = 0; //used for logging trial ID's


	[HideInInspector] public GameObject currentDefaultObject; //current treasure chest we're looking for. assuming a one-by-one reveal.
	

	void Start(){
		rotationBackgroundCube.TurnVisible (false);
		orderedStores = new List<Store>();
		orderedItemsDelivered = new List<string>();
	}


	
	void Update(){
		if(!isConnectingToHardware && currentState != TrialState.recall){
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
			TCPServer.Instance.SetState (TCP_Config.DefineStates.PAUSED, true);
		} 
		else {
			Time.timeScale = 1.0f;
			PauseUI.alpha = 0.0f;
			TCPServer.Instance.SetState (TCP_Config.DefineStates.PAUSED, false);
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
			yield return StartCoroutine(exp.instructionsController.PlayLearningInstructions());

			//ROTATION PHASE
			if(Config.doRotationPhase){
				yield return StartCoroutine(DoStoreRotationPhase());
			}

			//LEARNING PHASE
			if(Config.doLearningPhase){
				yield return StartCoroutine(DoLearningPhase());
			}


			exp.eventLogger.LogSessionStarted(Experiment.sessionID);

			for(int i = 0; i < ExperimentSettings.numDelivDays; i++){
				exp.player.controls.ShouldLockControls = true;

				yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("Press (X) to begin delivery day number " + (i+1) + "/" + ExperimentSettings.numDelivDays + ".", true, true, false, Config.minDefaultInstructionTime));
			

				exp.player.controls.ShouldLockControls = false;

				//DELIVERY DAY
				orderedStores.Clear();
				orderedItemsDelivered.Clear();
				yield return StartCoroutine(DoStoreDeliveryPhase(i));


				//RECALL
				//TODO: implement different kinds of recall phases
				Config.RecallType recallType = Config.RecallTypesAcrossTrials[i];
				if(recallType == Config.RecallType.FreeThenCued){
					yield return StartCoroutine(DoRecallPhase( Config.RecallType.FreeItemRecall, i));
					yield return StartCoroutine(DoRecallPhase( Config.RecallType.CuedRecall, i));
				}
				else{
					yield return StartCoroutine(DoRecallPhase( recallType, i));
				}

			}

			exp.player.controls.ShouldLockControls = true;

			//FINAL RECALL
			if(Config.doFinalStoreRecall){
				yield return StartCoroutine(DoRecallPhase(Config.RecallType.FinalStoreRecall, ExperimentSettings.numDelivDays + 1)); //it's an extra recall phase! +1
			}

			if(Config.doFinalItemRecall){
				yield return StartCoroutine(DoRecallPhase(Config.RecallType.FinalItemRecall, ExperimentSettings.numDelivDays));
			}

			exp.player.controls.ShouldLockControls = true;
			yield return StartCoroutine(exp.instructionsController.ShowSingleInstruction("You have completed the session! \nPress (X) to proceed.", true, true, false, 0.0f));
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
		TCPServer.Instance.SetState (TCP_Config.DefineStates.LEARNING_NAVIGATION_PHASE, true);

		yield return StartCoroutine(exp.instructionsController.ShowSingleInstruction("Press (X) to begin the practice session.", true, true, false, 0.0f));

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
		TCPServer.Instance.SetState (TCP_Config.DefineStates.LEARNING_NAVIGATION_PHASE, false);
	}

	IEnumerator DoStoreRotationPhase(){
		currentState = TrialState.rotationLearning;
		TCPServer.Instance.SetState (TCP_Config.DefineStates.LEARNING_ROTATION_PHASE, true);

		exp.eventLogger.LogRotationPhase (true);

		yield return StartCoroutine(exp.instructionsController.PlayRotationInstructions());

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
		TCPServer.Instance.SetState (TCP_Config.DefineStates.LEARNING_ROTATION_PHASE, false);
	}

	Store lastStore;	
	IEnumerator DoVisitStoreCommand(Store storeToVisit, bool isLearning, int numDeliveryToday){ //if it's not learning, it's a delivery!
		lastStore = storeToVisit;

		exp.eventLogger.LogStoreStarted (storeToVisit, isLearning, true, numDeliveryToday);
		SetServerStoreTarget(numDeliveryToday, true);

		exp.player.controls.ShouldLockControls = true;

		//show initial delivery command instruction
		InitialDeliveryInstructionGroup.alpha = 1.0f;
		InitialDeliveryInstructionText.text = "Please find the " + storeToVisit.name + ".\nPress (X) to continue.";
		yield return StartCoroutine (UsefulFunctions.WaitForActionButton ());
		InitialDeliveryInstructionGroup.alpha = 0.0f;

		exp.player.controls.ShouldLockControls = false;

		//show instruction at top of screen, don't wait for button, wait for collision
		DeliveryInstructionText.text = "Go to the " + storeToVisit.name;
		yield return StartCoroutine (exp.player.WaitForStoreCollision (storeToVisit.gameObject));
		DeliveryInstructionText.text = " ";

		exp.eventLogger.LogStoreStarted (storeToVisit, isLearning, false, numDeliveryToday);
		SetServerStoreTarget(numDeliveryToday, false);
	}
	
	IEnumerator DoStoreDeliveryPhase(int deliveryDay){

		currentState = TrialState.delivery;

		exp.player.controls.GoToStartPosition ();

		exp.eventLogger.LogDeliveryDay (deliveryDay, true);
		TCPServer.Instance.SetState (TCP_Config.DefineStates.DELIVERY_NAVIGATION, true);

		List<Store> deliveryStores = exp.storeController.GetRandomDeliveryStores();

		//if the first delivery store in this list is the same as the last one in the last list...
			//swap it with another one so that they're not consecutive!
		/*if (deliveryStores [0] == lastStore) {
			int randomSwapIndex = Random.Range(1, deliveryStores.Count);
			Store temp = deliveryStores[0];
			deliveryStores[0] = deliveryStores[randomSwapIndex];
			deliveryStores[randomSwapIndex] = temp;
		}*/


		for (int numDelivery = 0; numDelivery < deliveryStores.Count; numDelivery++) {
			//start delivery timer
			deliveryTimer.StartTimer();
			//visit store
			yield return StartCoroutine(DoVisitStoreCommand(deliveryStores[numDelivery], false, numDelivery));

			//calculate score
			//exp.scoreController.CalculateTimeBonus(deliveryTimer.GetSecondsInt());

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
		TCPServer.Instance.SetState (TCP_Config.DefineStates.DELIVERY_NAVIGATION, false);
	}

	IEnumerator DeliverItemVisible(Store toStore, int numDelivery){
		//show delivered item
		GameObject itemDelivered = exp.objectController.SpawnDeliverable(Vector3.zero);
		string itemDisplayText = exp.objectController.GetDeliverableText(itemDelivered);

		string itemName = itemDelivered.GetComponent<SpawnableObject> ().GetName ();
		exp.eventLogger.LogItemDelivery(itemName, toStore, numDelivery, false, true);
		SetServerItemDelivered(numDelivery, true);

		yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("You delivered " + itemDisplayText + " to the " + toStore.name, true, false, false, Config.deliveryCompleteInstructionsTime));

		exp.eventLogger.LogItemDelivery(itemName, toStore, numDelivery, false, false);
		SetServerItemDelivered(numDelivery, false);

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
				SetServerItemDelivered(numDelivery, true);
				yield return StartCoroutine(collisionStore.PlayDeliveryAudio(numDelivery));
				SetServerItemDelivered(numDelivery, false);

				string item = collisionStore.GetComponent<AudioSource>().clip.name;

				orderedStores.Add(collisionStore);
				orderedItemsDelivered.Add(item);
			}
		
		}

		yield return 0;
	}

	IEnumerator StartRecall(){
		Debug.Log ("START BEEP");
		recallStartBeep.Play ();
		while (recallStartBeep.isPlaying) {
			yield return 0;
		}
	}
	
	IEnumerator EndRecall(){
		Debug.Log ("END BEEP");
		recallEndBeep.Play ();
		while(recallEndBeep.isPlaying){
			yield return 0;
		}
	}

	IEnumerator DoRecallPhase(Config.RecallType recallType, int numRecallPhase){

		currentState = TrialState.recall;

		RecallUI.alpha = 1.0f;

		exp.player.controls.ShouldLockControls = true;

		//record audio to a file in the session directory for the duration of the recall period
		string fileName = ExperimentSettings.currentSubject.name + "_" + numRecallPhase;

		int recallTime = 0;

		TCP_Config.DefineStates recallState = TCP_Config.DefineStates.RECALL_CUED;

		switch(recallType){
			case Config.RecallType.FreeItemRecall:
				recallState = TCP_Config.DefineStates.RECALL_FREE_ITEM;
				recallTime = Config.freeRecallTime;
				exp.recallInstructionsController.DisplayText ("Speak aloud all items that you remember from this delivery day.");
				break;
			case Config.RecallType.CuedRecall:
				exp.eventLogger.LogRecallPhaseStarted (recallType, true);
				TCPServer.Instance.SetState(recallState, true);
				yield return StartCoroutine( DoCuedRecall (fileName));
			break;
			case Config.RecallType.FreeThenCued:
			/*recallState = TCP_Config.DefineStates.RECALL_FREE_AND_CUED;
				recallTime = Config.freeRecallTime;
				exp.recallInstructionsController.DisplayText ("Free recall STORES DELIVERED TO");*/
				Debug.Log("FREE THEN CUED SHOULD JUST RUN FREE, THEN CUED. NOT BOTH AT ONCE.");
				break;
			case Config.RecallType.FinalItemRecall:
				recallState = TCP_Config.DefineStates.FINALRECALL_ITEM;
				recallTime = Config.finalFreeItemRecallTime;
				RecallUI.alpha = 0.0f;
				exp.recallInstructionsController.DisplayText("");
				yield return StartCoroutine(exp.instructionsController.ShowSingleInstruction(InstructionsController.finalItemRecallInstructions, true, true, false, 0.0f));
				RecallUI.alpha = 1.0f;
				//exp.recallInstructionsController.DisplayText ("Speak aloud all items that you remember.");
				break;
			case Config.RecallType.FinalStoreRecall:
				recallState = TCP_Config.DefineStates.FINALRECALL_STORE;
				recallTime = Config.finalStoreRecallTime;
				RecallUI.alpha = 0.0f;
				exp.recallInstructionsController.DisplayText("");
				yield return StartCoroutine(exp.instructionsController.ShowSingleInstruction(InstructionsController.finalStoreRecallInstructions, true, true, false, 0.0f));
				RecallUI.alpha = 1.0f;
				//exp.recallInstructionsController.DisplayText ("Speak aloud all stores that you remember.");
				break;
		}

		//only record here if free recall! cued recall recording handled within DoCuedRecall()
		if(recallType != Config.RecallType.CuedRecall){

			exp.eventLogger.LogRecallPhaseStarted (recallType, true);
			TCPServer.Instance.SetState(recallState, true);

			yield return StartCoroutine(StartRecall());

			if (ExperimentSettings.isLogging) {
				yield return StartCoroutine (exp.audioRecorder.Record (exp.SessionDirectory + "audio", fileName, recallTime));
			}
			else{
				yield return new WaitForSeconds(recallTime);
			}

			yield return StartCoroutine(EndRecall());
		}

		exp.player.controls.ShouldLockControls = false;

		RecallUI.alpha = 0.0f;

		exp.eventLogger.LogRecallPhaseStarted (recallType, false);
		TCPServer.Instance.SetState(recallState, false);
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

				exp.eventLogger.LogCuedRecallPresentation(cueName, false, false, true);

				exp.recallInstructionsController.DisplayText ("What did you deliver here?"); //to the " + cueName + "?");

				//show image
				//storeImage = exp.objectController.SpawnStoreImage(Vector3.zero, cueName);
				storeImage = exp.objectController.GetStoreImage(cueName);
				if(storeImage != null){
					storeImage.GetComponent<VisibilityToggler>().TurnVisible(true);
				}

				exp.eventLogger.LogCuedRecallPresentation(cueName, false, false, false);
				SetServerStoreCueState(index, true);
			}
			else{	//item cued

				cueName = orderedItemsDelivered[index];

				cueName.Replace("-", " ");

				exp.eventLogger.LogCuedRecallPresentation(cueName, true, true, true);

				exp.recallInstructionsController.DisplayText ("Where did you deliver the spoken item?");// the " + cueName + "?");

				//play audio
				orderedStores[index].PlayCurrentAudio();
				while(orderedStores[index].GetIsAudioPlaying()){ //wait for audio to finish playing before proceeding
					yield return 0;
				}

				exp.eventLogger.LogCuedRecallPresentation(cueName, true, true, false);
				SetServerItemCueState(index, true);
			}

			yield return StartCoroutine (StartRecall());

			if (ExperimentSettings.isLogging) {
				yield return StartCoroutine (exp.audioRecorder.Record (exp.SessionDirectory + "audio", recordFileName + "_" + cueName, Config.cuedRecallTime));
			}
			else{
				yield return new WaitForSeconds(Config.cuedRecallTime);
			}

			yield return StartCoroutine(EndRecall());

			if(storeImage != null){
				storeImage.GetComponent<VisibilityToggler>().TurnVisible(false);
				SetServerStoreCueState(index, false);
				//Destroy(storeImage);
			}
			else{
				SetServerItemCueState(index, false);
			}
		}

		yield return 0;
	}

	void SetServerStoreCueState(int numStore, bool isTrue){
		switch (numStore){
		case 0:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_0, isTrue);
			break;
		case 1:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_1, isTrue);
			break;
		case 2:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_2, isTrue);
			break;
		case 3:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_3, isTrue);
			break;
		case 4:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_4, isTrue);
			break;
		case 5:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_5, isTrue);
			break;
		case 6:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_6, isTrue);
			break;
		case 7:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_7, isTrue);
			break;
		case 8:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_8, isTrue);
			break;
		case 9:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_9, isTrue);
			break;
		case 10:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_10, isTrue);
			break;
		case 11:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_11, isTrue);
			break;
		}

	}

	void SetServerItemCueState(int numItem, bool isTrue){
		switch (numItem){
		case 0:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_0, isTrue);
			break;
		case 1:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_1, isTrue);
			break;
		case 2:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_2, isTrue);
			break;
		case 3:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_3, isTrue);
			break;
		case 4:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_4, isTrue);
			break;
		case 5:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_5, isTrue);
			break;
		case 6:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_6, isTrue);
			break;
		case 7:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_7, isTrue);
			break;
		case 8:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_8, isTrue);
			break;
		case 9:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_9, isTrue);
			break;
		case 10:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_10, isTrue);
			break;
		case 11:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_11, isTrue);
			break;
		}
	}

	void SetServerStoreTarget(int numStore, bool isTrue){
		switch (numStore){
		case 0:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_0, isTrue);
			break;
		case 1:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_1, isTrue);
			break;
		case 2:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_2, isTrue);
			break;
		case 3:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_3, isTrue);
			break;
		case 4:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_4, isTrue);
			break;
		case 5:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_5, isTrue);
			break;
		case 6:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_6, isTrue);
			break;
		case 7:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_7, isTrue);
			break;
		case 8:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_8, isTrue);
			break;
		case 9:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_9, isTrue);
			break;
		case 10:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_10, isTrue);
			break;
		case 11:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_11, isTrue);
			break;
		case 12:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_12, isTrue);
			break;
		}
	}

	void SetServerItemDelivered(int numItem, bool isTrue){
		switch (numItem){
		case 0:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_0, isTrue);
			break;
		case 1:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_1, isTrue);
			break;
		case 2:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_2, isTrue);
			break;
		case 3:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_3, isTrue);
			break;
		case 4:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_4, isTrue);
			break;
		case 5:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_5, isTrue);
			break;
		case 6:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_6, isTrue);
			break;
		case 7:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_7, isTrue);
			break;
		case 8:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_8, isTrue);
			break;
		case 9:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_9, isTrue);
			break;
		case 10:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_10, isTrue);
			break;
		case 11:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_11, isTrue);
			break;
		}
	}

	void DestroyGameObjectList(List<GameObject> listOfGameObjects){
		int numObjects = listOfGameObjects.Count;
		for (int i = 0; i < numObjects; i++) {
			Destroy (listOfGameObjects [i]);
		}
		listOfGameObjects.Clear ();
	}
	
}
