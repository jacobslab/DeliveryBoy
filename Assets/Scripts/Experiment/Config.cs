using UnityEngine;
using System.Collections;

public class Config : MonoBehaviour {

	//JUICE
	public static bool isJuice = true;
	public static bool isSoundtrack = false; //WON'T PLAY IF ISJUICE IS FALSE.

	//stimulation variables
	public static bool shouldDoStim;	//TODO
	public static int stimFrequency;	//TODO
	public static float stimDuration;	//TODO
	public static bool shouldDoBreak;	//TODO
	
	//test session variables
	//doTestSession (not implemented in the panda3d version )

	public static int numLearningIterations = 1;


	public static int numTestTrials = 2;
	
	//practice settings
	public static int numTrialsPract = 1;
	public static bool doPracticeTrial = false;
	public static int numSpecialObjectsPract = 2;


	//SPECIFIC TASK VARIABLES:
	public static float randomJitterMin = 0.0f;
	public static float randomJitterMax = 0.2f;

	//STORES
	public static int numDeliveryStores = 2; //out of the total number of stores
	


	public static string initialInstructions1 = "Welcome to Treasure Island!" + 
		"\n\nYou are going on a treasure hunt." + 
			"\n\nUse the joystick to control your movement." + 
			"\n\nDrive into treasure chests to open them. Remember where each object is located!";

	public static string initialInstructions2 = "TIPS FOR MAXIMIZING YOUR SCORE" + 
		"\n\nGet a time bonus by driving to the chests quickly." + 
			"\n\nIf you are more than 50% sure of an object's location, you should say you remember." + 
			"\n\nIf you say you are very sure, you should be at least 75% accurate." + 
			"\n\nPress (X) to begin!";

	
	public static float minInitialInstructionsTime = 0.0f; //TODO: change back to 5.0f
	public static float deliveryCompleteInstructionsTime = 2.0f;
	public static float minDefaultInstructionTime = 0.0f; //time each learning trial instruction should be displayed for

	public static float recallTime = 2.0f;

	//tilt variables
	public static bool isAvatarTilting = true;
	public static float turnAngleMult = 0.07f;

	//drive variables
	public static float driveSpeed = 22;

	//object buffer variables

	void Awake(){
		DontDestroyOnLoad(transform.gameObject);
	}

	void Start(){

	}

	public static int GetTotalNumTrials(){
		if (!doPracticeTrial) {
			return numTestTrials;
		} 
		else {
			return numTestTrials + numTrialsPract;
		}
	}

}
