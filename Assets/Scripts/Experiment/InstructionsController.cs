using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.UI;

public class InstructionsController : MonoBehaviour {

	Experiment exp { get { return Experiment.Instance; } }

	public float timePerInstruction;

	public bool isFinished = false;

	//TextMesh _textMesh;
	public Text text; //TODO: rename this!!!
	public Text oculusText;
	Color textColorDefault;
	public Color textColorOverlay;
	public Image background;
	public Image oculusBackground;
	Color backgroundColorDefault;

	public GameObject ScoreInstructions; //turn these on and off as necessary during the trial.......


	//INITIAL INSTRUCTIONS
	public static string initialInstructions1 = "INTRODUCTION" +
		"\n\nIn this game you will play a delivery person in a small city. " +
		"Your task is to drive through the city delivering packages to the correct stores, as quickly as possible. " +
		"\n\nYour current delivery goal will be shown in the top left of the screen.  Simply drive right up to the store, and the item you delivered will spoken out loud. " +
		"On each trial you will make a series of deliveries to stores all over the town. " +
		"\n\nYour current delivery goal will be shown in the top left of the screen. " + 
		"Simply drive right up to the store, and the item you delivered will be spoken out loud. " +
		"On each trial you will make a series of deliveries to stores all over the town. ";
	
	public static string initialInstructions2 = "\n\nOn the final delivery of a given trial, no item will be said. " +
		"Rather, the screen will go blank, and you'll see a row of asterisks (*******), and hear a tone. " +
		"At this point your job is to verbally recall all of the delivered items that you can remember, in any order." +
		"\n\nAfter the recall period you'll have a chance for a short break, and then the next set of deliveries will start. ";

	public static string initialInstructions3Learning = "Before you start the full task, you'll have a chance to explore the town, to get your bearings." +
		"\n\nWe will describe this practice period next." +
			"\n\nPlease tell the investigator when you have finished reading these instructions.";
	

	//LEARNING PHASE INSTRUCTIONS
	public static string learningInstructions1 = "PRACTICE SESSION" +
		"\n\nBefore you start making deliveries, we want to make sure you know your way around the city. " +
		" You'll be asked to go from store to store without delivering items. " +
		"The city will be exactly the same during the practice as during the later delivery trials, so you can use this time to figure out the fastest way to get from place to place. " +
			"To help you out, we will often send you from one store to another store that is nearby in the town.";
	
	public static string learningInstructions2 = "\n\nDon't worry if it takes a little while to learn the city!" +
		/*" Your bonus is not affected by how long the practice period takes." +*/
			"\n\nFinding the correct stores will be difficult at first, but it will get easier when you become more familiar with the city." +
			"\n\nPlease tell the investigator when you have finished reading.";


	public static string finalItemRecallInstructions = "In this next period, please recall as many ITEMS as you can remember from the entire session, in any order. " +
		"You will be given several minutes to do this; please keep trying for the whole period, as you may find that words keep springing up in your memory." +
		"\n\n* Press (X) to begin the recall period *";

	public static string finalStoreRecallInstructions = "In this next period, please recall as many STORE NAMES as you can remember from the entire session, in any order. " +
		"You will be given several minutes to do this; please keep trying for the whole period, as you may find that words keep springing up in your memory." +
		"\n\n* Press (X) to begin the recall period *";


	//ROTATION PHASE INSTRUCTIONS
	public static string rotationInstructions1 = "Now you will see images of the stores in the city so that you can better recognize them while doing the task. " +
		"Please pay attention to them as they appear on the screen.";
	// Use this for initialization
	void Start () {
		if (background != null) {
			backgroundColorDefault = background.color;
		}
		textColorDefault = text.color;
	}

	public void TurnOffInstructions(){
		SetInstructionsTransparentOverlay();
		SetInstructionsBlank();
	}

	public IEnumerator PlayStartInstructions(){
		yield return StartCoroutine (ShowSingleInstruction (InstructionsController.initialInstructions1 + InstructionsController.initialInstructions2 + InstructionsController.initialInstructions3Learning, true, true, false, Config.minInitialInstructionsTime));
		/*yield return StartCoroutine (ShowSingleInstruction (InstructionsController.initialInstructions2, true, true, false, Config.minInitialInstructionsTime));
		if (Config.doLearningPhase) {
			yield return StartCoroutine (ShowSingleInstruction (InstructionsController.initialInstructions3Learning, true, true, false, Config.minInitialInstructionsTime));
		} */
	}

	public IEnumerator PlayRotationInstructions(){
		yield return StartCoroutine (ShowSingleInstruction (InstructionsController.rotationInstructions1, true, true, false, Config.minInitialInstructionsTime));;
	}

	public IEnumerator PlayLearningInstructions(){
		yield return StartCoroutine (ShowSingleInstruction (InstructionsController.learningInstructions1 + InstructionsController.learningInstructions2, true, true, false, Config.minInitialInstructionsTime));
		//yield return StartCoroutine (ShowSingleInstruction (InstructionsController.learningInstructions2, true, true, false, Config.minInitialInstructionsTime));
	}

	public IEnumerator ShowSingleInstruction(string line, bool isDark, bool waitForButton, bool addRandomPostJitter, float minDisplayTimeSeconds){
		Experiment.Instance.trialController.GetComponent<TrialLogTrack> ().LogInstructionEvent ();

		if(isDark){
			SetInstructionsColorful();
		}
		else{
			SetInstructionsTransparentOverlay();
		}
		DisplayText(line);
		
		yield return new WaitForSeconds (minDisplayTimeSeconds);
		
		if (waitForButton) {
			yield return StartCoroutine (UsefulFunctions.WaitForActionButton ());
		}
		
		if (addRandomPostJitter) {
			yield return StartCoroutine(UsefulFunctions.WaitForJitter ( Config.randomJitterMin, Config.randomJitterMax ) );
		}
		
		TurnOffInstructions ();
		exp.cameraController.SetInGame();
	}

	public void SetSingleInstruction(string text, bool isDark){
		Experiment.Instance.trialController.GetComponent<TrialLogTrack> ().LogInstructionEvent ();
		if (isDark) {
			SetInstructionsColorful ();
		} else {
			SetInstructionsTransparentOverlay();
		}

		SetText (text);
	}

	void SetText(string newText){
		if(ExperimentSettings.isOculus){
			oculusText.text = newText;
		}
		else{
			text.text = newText;
		}
	}

	public void DisplayText(string line){
		SetText(line);
	}

	public void SetInstructionsBlank(){
		SetText ("");
	}

	public void SetInstructionsColorful(){
		//Debug.Log("set instructions dark");
		if(ExperimentSettings.isOculus){
			oculusBackground.color = backgroundColorDefault;
			oculusText.color = textColorDefault;
		}
		else{
			background.color = backgroundColorDefault;
			text.color = textColorDefault;
		}
	}
	
	public void SetInstructionsTransparentOverlay(){
		//Debug.Log("set instructions transparent overlay");
		if(ExperimentSettings.isOculus){
			oculusBackground.color = new Color(0,0,0,0);
			oculusText.color = textColorOverlay;
		}
		else{
			background.color = new Color(0,0,0,0);
			text.color = textColorOverlay;
		}
	}
	

}
