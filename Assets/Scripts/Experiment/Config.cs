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

	public static int numTestTrials = 8;
	
	//practice settings
	public static int numTrialsPract = 1;
	public static bool doPracticeTrial = false;


	//SPECIFIC TASK VARIABLES:
	public static float randomJitterMin = 0.0f;
	public static float randomJitterMax = 0.2f;

	//DELIVERY VARIABLES
	public static int numDeliveryStores = 12; //out of the total number of stores
	
	public static bool isAudioDelivery = true;

	public static int numLearningIterations = 1;
	public static bool doLearningPhase = false;
	public static bool doRotationPhase = false;

	public static float storeRotateTime = 8.0f;
	public static float numStoreRotations = 1.3f;
	
	public static float minInitialInstructionsTime = 0.0f;
	public static float deliveryCompleteInstructionsTime = 2.0f;
	public static float minDefaultInstructionTime = 0.0f; //time each learning trial instruction should be displayed for

	public static int recallTime = 2;

	public enum RecallType
	{
		FreeItemRecall,
		FreeStoreRecall,
		CuedItemRecall,
		CuedStoreRecall,
		FinalItemRecall,
		FinalStoreRecall,
	}


	public static bool shouldUseWaypoints = true;
	public static float timeUntilWaypoints = 10.0f;

	//tilt variables
	public static bool isAvatarTilting = true;
	public static float turnAngleMult = 0.07f;

	//drive variables
	public static float driveSpeed = 10;

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
