using UnityEngine;
using System.Collections;

public class Config : MonoBehaviour {

	public enum Version
	{
		DBoy3_1,
		DBoy3_2,
		DBoy3_3
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
	public static int numDeliveryStores = 2;//13; //out of the total number of stores -- LAST STORE DOES NOT ACTUALLY GET DELIVERY
	
	public static bool isAudioDelivery = true;
	public static bool isStoreCorrelatedDelivery = true;

	public static int numLearningIterations = 1;
	public static bool doLearningPhase = false;
	public static bool doRotationPhase = false;

	public static bool doFinalItemRecall = true;
	public static bool doFinalStoreRecall = true;

	public static float storeRotateTime = 12.0f;
	public static float numStoreRotations = 1.3f;
	
	public static float minInitialInstructionsTime = 0.0f;
	public static float deliveryCompleteInstructionsTime = 2.0f;
	public static float minDefaultInstructionTime = 0.0f; //time each learning trial instruction should be displayed for

	public static int freeRecallTime = 5;//90;
	public static int cuedRecallTime = 5;
	public static int finalFreeItemRecallTime = 5;//360;
	public static int finalStoreRecallTime = 5;//120;

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


	public static bool shouldUseWaypoints = true;
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
