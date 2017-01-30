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

	//instruction screen images
	public Image presentationInstructions;
	public Image practiceInstructions;
	public Image recapDeliveryInstructions;
	public Image finishedDeliveryInstructions;

	//TextMesh _textMesh;
	public Text text; //TODO: rename this!!!
	public Text oculusText;
	Color textColorDefault;
	public Color textColorOverlay;
	public Image background;
	public Image oculusBackground;
	Color backgroundColorDefault;

	public GameObject ScoreInstructions; //turn these on and off as necessary during the trial.......
	public GameObject VideoInstructions;
	public VideoPlayer videoPlayer;

	//INITIAL INSTRUCTIONS
#if (!GERMAN)
	public static string videoInstruction=" Play Instruction Video? (Y/N)";
	public static string initialInstructions1 = "INTRODUCTION" +
		"\n\nIn this game you will play a delivery person in a small city. " +
		"Your task is to drive through the city delivering packages to the correct stores, as quickly as possible. " +
		"\n\nYour current delivery goal will be shown in the top left of the screen.  Simply drive right up to the store, and the object you delivered will spoken out loud. " +
		"On each trial you will make a series of deliveries to stores all over the town. " +

		"\n\nOn the final delivery of a given trial, no object will be said. " +
		"Rather, the screen will go blank, and you'll see a row of asterisks (*******), and hear a tone. " +
		"When the tone has finished and the asterisks turn red, your job is to verbally recall all of the delivered objects that you can remember, in any order. " +
		"This free recall period will last for " + Config.freeRecallTime + " seconds. " +
		"Once this free recall period has ended, you will either be shown a building and asked to recall the object you delivered, " +
		"or you will hear an object that you delivered, and will be asked to recall the store to which you delivered it. " +
		"You will have 6 seconds to recall the matching object or store." +

			"\n\nAfter this cued recall period, you'll have a chance for a short break, and then the next set of deliveries will start. ";

	public static string initialInstructions2Learning = "\n\nBefore you start the full task, you'll have a chance to explore the town, to get your bearings. " +
		"We will describe this practice period next." +
			"\n\nPlease tell the investigator when you have finished reading these instructions.";

	public static string pressToContinueText = "\n\nPress (X) to continue.";

	//LEARNING PHASE INSTRUCTIONS
	public static string learningInstructions = "PRACTICE SESSION" +
		"\n\nBefore you start making deliveries, we want to make sure you know your way around the city. " +
		" You'll be asked to go from store to store without delivering objects. " +
		"The city will be exactly the same during the practice as during the later delivery trials, so you can use this time to figure out the fastest way to get from place to place. " +
			"To help you out, we will often send you from one store to another store that is nearby in the town." +

		"\n\nDon't worry if it takes a little while to learn the city!" +
		/*" Your bonus is not affected by how long the practice period takes." +*/
		"\n\nFinding the correct stores will be difficult at first, but it will get easier when you become more familiar with the city." +
			"\n\nPlease tell the investigator when you have finished reading.";


	public static string finalItemRecallInstructions = "In this next period, please recall as many OBJECTS as you can remember from the entire session, in any order. " +
		"You will be given several minutes to do this; please keep trying for the whole period, as you may find that words keep springing up in your memory." +
		"\n\n* Press (X) to begin the recall period *";

	public static string finalStoreRecallInstructions = "In this next period, please recall as many STORE NAMES as you can remember from the entire session, in any order. " +
		"You will be given several minutes to do this; please keep trying for the whole period, as you may find that words keep springing up in your memory." +
		"\n\n* Press (X) to begin the recall period *";
#elif GERMAN
	public static string initialInstructions1 = "ANLEITUNG" +
		"\n\nIn diesem Computerspiel übernehmen Sie die Aufgabe eines Kurierboten in einer kleinen Stadt. " +
		"Ihre Aufgabe ist es durch die Stadt zu fahren und, so schnell wie möglich, Pakete an die richtigen Geschäfte zu liefern. " +

		"\n\nDas Ziel jeder Lieferung wird oben links auf dem Bildschirm angezeigt. Wenn Sie am Ziel angekommen sind, fahren Sie einfach direkt " +
		"an das Geschäft heran und der Gegenstand den Sie geliefert haben, wird angezeigt. Während des Spiels haben Sie die Gelegenheit, eine Reihe " +
		"solcher Lieferungen in der ganzen Stadt zu tätigen." +

		"\n\nAm Ende der letzten Lieferung einer solchen Serie werden Sie anstelle eines Gegenstandes einen leeren Bildschirm mit einer Reihe von " +
		"Sternchen (*******) sehen und einen Ton hören. Sobald der Ton abbricht und die weißen Sternchen (*******) rot werden, " +
		"haben Sie" + Config.freeRecallTime + "Sekunden Zeit, so viele der gelieferten Gegenstände wie möglich laut aufzuzählen (die Reihenfolge ist dabei unwichtig." +
		
		"\n\nDanach werden Sie entweder ein Gebäude sehen und nach dem Gegenstand gefragt, den Sie hierher geliefert haben oder einen Gegenstand" +
		"sehen und nach dem Geschäft gefragt, an das Sie den Gegenstand geliefert haben." +

			//" You will have 6 seconds to recall the matching item or store." +
			" Sie haben 6 Sekunden zu erinnern, der Gegenstand oder das Geschäft." +

			" Nach diesem Gedächtnis Test können Sie eine kurze Pause machen, bevor eine neue Runde von Lieferungen beginnt.";
	
	public static string initialInstructions2Learning = "\n\nBevor das eigentliche Spiel beginnt, lassen wir Sie erstmal die Stadt " +
		"erkunden, damit Sie sich orientieren können. Diese Übungsphase werden wir Ihnen als nächstes erklären." +
		"\n\nBitte sagen Sie dem Versuchsleiter Bescheid, wenn sie diese Anleitung zu Ende gelesen haben.";
	
	public static string pressToContinueText = "\n\nDrücken Sie (X) um fortzufahren.";


	//LEARNING PHASE INSTRUCTIONS
	public static string learningInstructions = "ÜBUNGSPHASE" +
		"\n\nBevor Sie mit den Lieferungen beginnen, möchten wir Sie mit der Stadt vertraut machen. Wir werden Sie bitten, zu verschiedenen Geschäften " +
		"zu fahren ohne Lieferungen vorzunehmen. Sie werden später in der gleichen Stadt Lieferungen vornehmen, daher ist es günstig, wenn Sie " + 
		"diese Übungsphase nutzen, um die kürzesten Wege zwischen Geschäften herauszufinden. Als kleine Hilfe werden wir Sie in dieser Übungsphase " +
		"oft bitten, zu naheliegenden Geschäften zu fahren." +
	
		"\n\nMachen Sie sich keine Sorgen, wenn Sie etwas länger brauchen um sich mit der Stadt vertraut zu machen. " +
		"\n\nZunächst wird es schwierig sein die richtigen Geschäfte zu finden, aber mit der Zeit sollten Sie ein gutes Gefühl für die Stadt bekommen." +
			"\n\nBitte sagen Sie dem Versuchsleiter Bescheid, wenn Sie diese Anleitung zu Ende gelesen haben.";
	
	//FINAL RECALL INSTRUCTIONS
	public static string finalItemRecallInstructions = "In dem folgenden Gedächtnistest, zählen Sie bitte so viele von allen " +
		"heute gelieferten GEGENSTÄNDEN (d.h. von allen Lieferphasen) laut auf wie möglich (die Reihenfolge ist dabei unwichtig). Sie haben dafür " +
		"mehrere Minuten Zeit. Bitte versuchen Sie sich bis zum Ende diese Zeitraumes an die Namen von Geschäften zu erinnern, da Ihnen u.U. auch " + 
		"noch Gegenstände einfallen, wenn Sie glauben sich an keine weiteren erinnern zu können." +
			"\n\n* Bitte drücken Sie (X), wenn Sie bereit sind  *";

	public static string finalStoreRecallInstructions = "Im folgenden Gedächtnistest zählen Sie bitte so viele NAMEN VON" +
		"GESCHÄFTEN laut auf wie möglich (die Reihenfolge ist dabei unwichtig). Sie haben dafür mehrere Minuten Zeit. Bitte versuchen Sie "+
			"sich bis zum Ende dieses Zeitraumes an die Namen von Gegenstand zu erinnern, da Ihnen u.U. auch noch Namen einfallen, wenn Sie glauben " +
		"sich an keine weiteren erinnern zu können." +
		"\n\n* Bitte drücken Sie (X), wenn Sie bereit sind *";
#endif


	//ROTATION PHASE INSTRUCTIONS
#if GERMAN
	public static string rotationInstructions1 = "Sie werden jetzt Bilder von den Geschäften in der Stadt sehen, damit Sie sie während des Spiels besser wieder erkennen können. " + 
		"Bitte schauen Sie sich die Bilder sorgfältig an.";
#else
	public static string rotationInstructions1 = "Now you will see images of the stores in the city so that you can better recognize them while doing the task. " +
		"Please pay attention to them as they appear on the screen.";
#endif

	// Use this for initialization
	void Start () {
		if (background != null) {
			backgroundColorDefault = background.color;
		}
		textColorDefault = text.color;
		TurnOffInstructions ();
	}

	public void TurnOffInstructions(){
		SetInstructionsTransparentOverlay();
		DisableInstructionScreens ();
		SetInstructionsBlank();
	}

	void DisableInstructionScreens()
	{
		presentationInstructions.enabled = false;
		practiceInstructions.enabled = false;
		recapDeliveryInstructions.enabled = false;
		finishedDeliveryInstructions.enabled = false;
	}
	public IEnumerator PlayVideoInstructions(){
		VideoInstructions.GetComponent<CanvasGroup> ().alpha = 1f;
		yield return StartCoroutine (videoPlayer.Play());

		VideoInstructions.GetComponent<CanvasGroup> ().alpha = 0f;
		yield return null;
	}

	public IEnumerator PlayStartInstructions(){
		string startInstruction = InstructionsController.initialInstructions1;

		if (Config.doLearningPhase || ExperimentSettings.Instance.mySessionType == ExperimentSettings.SessionType.learningSession) {
			startInstruction += InstructionsController.initialInstructions2Learning;
		}

		startInstruction += InstructionsController.pressToContinueText;
			
		yield return StartCoroutine (ShowSingleInstruction (startInstruction, true, true, false, Config.minInitialInstructionsTime));

	}

	public IEnumerator AskIfShouldPlay(){
		SetInstructionsColorful ();
		SetText ("Play instruction video? (y/n)");
		Debug.Log("show instructions");
		bool isValidInput = false;
		while (!isValidInput) {
			if (Input.GetKeyUp (KeyCode.Y)) {
				isValidInput = true;
				videoPlayer.shouldPlay = true;

			}
			else if (Input.GetKeyUp (KeyCode.N)) {
				isValidInput = true;
				videoPlayer.shouldPlay = false;
			}
			yield return 0;
		}

		SetInstructionsBlank ();
		SetInstructionsTransparentOverlay ();
	}


	public IEnumerator PlayPresentationInstructions(){
		//string rotInstructions = InstructionsController.rotationInstructions1 + InstructionsController.pressToContinueText;
		yield return StartCoroutine (ShowInstructionScreen (presentationInstructions, true, false, Config.minInitialInstructionsTime));
		//yield return StartCoroutine (ShowSingleInstruction (rotInstructions, true, true, false, Config.minInitialInstructionsTime));;
	}

	public IEnumerator PlayPracticeInstructions(){

		yield return StartCoroutine (ShowInstructionScreen (practiceInstructions, true, false, Config.minInitialInstructionsTime));

	//	string learningInstructions = InstructionsController.learningInstructions + InstructionsController.pressToContinueText;
	//	yield return StartCoroutine (ShowSingleInstruction (learningInstructions, true, true, false, Config.minInitialInstructionsTime));
	}

	public IEnumerator ShowInstructionScreen(Image instructionImage, bool waitForButton, bool addRandomPostJitter, float minDisplayTimeSeconds)
	{
		instructionImage.enabled = true;
		Experiment.Instance.trialController.GetComponent<TrialLogTrack> ().LogInstructionEvent ();
		yield return new WaitForSeconds (minDisplayTimeSeconds);

		if (waitForButton) {
			yield return StartCoroutine (UsefulFunctions.WaitForActionButton ());
		}

		if (addRandomPostJitter) {
			yield return StartCoroutine(UsefulFunctions.WaitForJitter ( Config.randomJitterMin, Config.randomJitterMax ) );
		}

		instructionImage.enabled = false;
		TurnOffInstructions ();
		exp.cameraController.SetInGame();
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
