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
	
	public static bool isSyncbox = true;
	public static bool isSystem2 = false;


	//STORE AND WORD POOL FILE NAMES
	public static string StoreFileName = "dbstores.txt";
	public static string PoolFileName = "dbpool.txt";

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


	//mic test
	public static float micLoudnessThreshold=0.1f;

	//SPECIFIC TASK VARIABLES:
	public static float randomJitterMin = 0.0f;
	public static float randomJitterMax = 0.2f;

	//DELIVERY VARIABLES
	public static int numDeliveryStores = 3; //out of the total number of stores -- LAST STORE DOES NOT ACTUALLY GET DELIVERY
//	public static int numDeliveryStores = 4;
	public static bool isAudioDelivery = true;
	public static bool isStoreCorrelatedDelivery = true;


#if HOSPITAL
    //a learning session, for the first session unless skipped
    public static int numLearningIterationsSession = 4;
	public static int numLearningIterationsPhase = 1;
    public static float numDelivTime=45f;
#else
    //for scalp lab
    //first session
    public static int numLearningIterationsSession = 4;
    public static int numFirstSessionDelivDays = 2;
    //second session onwards
    public static int numLearningIterationsPhase = 1;
#endif

    public static int numDelivDays = 6;
   
    //learning phase, at the start of a delivery session
    public static bool doLearningPhase = true;
	public static int maxLearningTimeMinutes = 45;
	
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

    public static float eyeDetectionToleranceTime = 10f;

	public static int freeRecallTime = 90;
	public static int cuedRecallTime = 6;
	public static int finalFreeItemRecallTime = 300;
	public static int finalStoreRecallTime = 90;
    public static float timeBetweenCuedRecalls = 1f;
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


	public static bool shouldUseWaypoints = true;
	public static int maxNumWrongTurns = 3;

	//tilt variables
	public static bool isAvatarTilting = true;
	public static float turnAngleMult = 0.07f;

	//drive variables
	public static float driveSpeed = 10f;
    public static float reverseSpeed = 2.5f;

	//object buffer variables

	void Awake(){
		DontDestroyOnLoad(transform.gameObject);
	}

	void Start(){

	}

}
