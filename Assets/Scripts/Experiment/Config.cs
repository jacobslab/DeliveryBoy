using UnityEngine;
using System.Collections;

public class Config : MonoBehaviour {

	public enum Version
	{
		//syncbox can be 1, demo can be 1, system 2.0 can be 1,2,3
		DBoy3_1, //no stim
		DBoy3_2, //open stim
		DBoy3_3 //closed stim
	}
	
	public static Version BuildVersion = Version.DBoy3_1;
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

	//DELIVERY VARIABLES
	public static int numDeliveryStores = 13; //out of the total number of stores -- LAST STORE DOES NOT ACTUALLY GET DELIVERY
	
	public static bool isAudioDelivery = true;
	public static bool isStoreCorrelatedDelivery = true;
	
	//a learning session, for the first session unless skipped
	public static bool skipLearningSession = false;
	public static int numLearningIterationsSession = 4;
	
	//learning phase, at the start of a session
	public static bool doLearningPhase = false;
	public static int numLearningIterationsPhase = 1;
	
	public static bool doPresentationPhase = true;
	public static float storePresentationTime = 1.5f;
	public static float betweenStoreBlankScreenTimeMax = 0.6f;
	public static float betweenStoreBlankScreenTimeMin = 0.4f;

	public static bool doFinalItemRecall = true;
	public static bool doFinalStoreRecall = true;

	public static float storeRotateTime = 12.0f;
	public static float numStoreRotations = 1.3f;
	
	public static float minInitialInstructionsTime = 0.0f;
	public static float deliveryCompleteInstructionsTime = 2.0f;
	public static float minDefaultInstructionTime = 0.0f; //time each learning trial instruction should be displayed for

	public static int freeRecallTime = 60;
	public static int cuedRecallTime = 6;
	public static int finalFreeItemRecallTime = 300;
	public static int finalStoreRecallTime = 90;
	public static float cuedEndBeepTimeBeforeEnd = 0.5f; //if the recall time is 6s, end beep should play this much time before the end of 6s. ie; 6 - endBeepTime

	public enum RecallType
	{
		FreeItemRecall,
		//FreeStoreRecall,
		CuedRecall,
		FreeThenCued,
		FinalItemRecall,	
		FinalStoreRecall,
	}

	//NOTE: THE LENGTH OF THIS ARRAY SHOULD MATCH THE NUMBER OF DELIVERY DAYS / TEST TRIALS.
	// 0 - free recall
	// 1 - cued recall
	// 2 - both free and cued
	//NOTE THESE MAY BE CONFIGURED DIFFERENTLY IN MAIN MENU.
	public static RecallType[] RecallTypesAcrossTrials = { RecallType.FreeThenCued, RecallType.FreeThenCued, RecallType.FreeThenCued, RecallType.FreeThenCued, 
								RecallType.FreeThenCued, RecallType.FreeThenCued, RecallType.FreeThenCued, RecallType.FreeThenCued };


	public static bool shouldUseWaypoints = false;
	public static float timeUntilWaypoints = 360.0f; //6 minutes

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

}
