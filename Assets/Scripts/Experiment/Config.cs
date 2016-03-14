using UnityEngine;
using System.Collections;

public class Config : MonoBehaviour {

	public enum Version
	{
		DBoy3	
	}
	
	public static Version BuildVersion = Version.DBoy3;
	public static string VersionNumber = "3.0";
	
	public static bool isSyncbox = false;
	public static bool isSystem2 = false;


	//REPLAY
	public static int replayPadding = 6;

	
	//JUICE
	public static bool isJuice = true;
	public static bool isSoundtrack = false; //WON'T PLAY IF ISJUICE IS FALSE.

	//stimulation variables
	public static bool shouldDoStim;	//TODO
	public static int stimFrequency;	//TODO
	public static float stimDuration;	//TODO
	public static bool shouldDoBreak;	//TODO


	//SPECIFIC TASK VARIABLES:
	public static float randomJitterMin = 0.0f;
	public static float randomJitterMax = 0.2f;
	
	public static int numTestTrials = 8;
	
	//practice settings
	public static int numTrialsPract = 1;
	public static bool doPracticeTrial = false;

	//DELIVERY VARIABLES
	public static int numDeliveryStores = 13; //out of the total number of stores -- LAST STORE DOES NOT ACTUALLY GET DELIVERY
	
	public static bool isAudioDelivery = true;
	public static bool isStoreCorrelatedDelivery = false;

	public static int numLearningIterations = 1;
	public static bool doLearningPhase = true;
	public static bool doRotationPhase = true;

	public static bool doFinalItemRecall = true;
	public static bool doFinalStoreRecall = true;

	public static float storeRotateTime = 8.0f;
	public static float numStoreRotations = 1.3f;
	
	public static float minInitialInstructionsTime = 0.0f;
	public static float deliveryCompleteInstructionsTime = 2.0f;
	public static float minDefaultInstructionTime = 0.0f; //time each learning trial instruction should be displayed for

	public static int freeRecallTime = 90;
	public static int cuedRecallTime = 5;
	public static int finalFreeRecallTime = 360;
	public static int finalStoreRecallTime = 120;

	public enum RecallType
	{
		FreeItemRecall,
		FreeStoreRecall,
		CuedRecall,
		FinalItemRecall,	
		FinalStoreRecall,
	}

	//NOTE: THE LENGTH OF THIS ARRAY SHOULD MATCH THE NUMBER OF DELIVERY DAYS / TEST TRIALS.
	// 0 - free recall
	// 1 - cued recall
	// 2 - both free and cued
	public static RecallType[] RecallTypesAcrossTrials = { RecallType.FreeItemRecall, RecallType.FreeStoreRecall, RecallType.CuedRecall, RecallType.FreeItemRecall, 
															RecallType.FreeStoreRecall, RecallType.CuedRecall, RecallType.FreeItemRecall, RecallType.FreeStoreRecall };


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
