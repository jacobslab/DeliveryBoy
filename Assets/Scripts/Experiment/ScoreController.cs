using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour {
	
	Experiment exp { get { return Experiment.Instance; } }

	public ScoreLogTrack scoreLogger;

	public int score = 0;
	public Text scoreText;
	int scoreTextScore = 0;
	int amountLeftToAdd = 0; //amount left to add to the score text

	//SCORE VARIABLES -- don't want anyone to change them, so make public getters, no setters.
	static int timeBonusSmall = 10;
	public static int TimeBonusSmall { get { return timeBonusSmall; } }

	static int timeBonusMed = 20;
	public static int TimeBonusMed { get { return timeBonusMed; } }

	static int timeBonusBig = 30;
	public static int TimeBonusBig { get { return timeBonusBig; } }
	

	static int memoryScoreRight = 100;
	public static int MemoryScoreRight { get { return memoryScoreRight; } }

	static int memoryScoreWrong = -50;
	public static int MemoryScoreWrong { get { return memoryScoreWrong; } }

	static int memoryScoreRightNotRemembered = 50;
	public static int MemoryScoreRightNotRemembered { get { return memoryScoreRightNotRemembered; } }
	
	static int memoryScoreWrongNotRemembered = 0;
	public static int MemoryScoreWrongNotRemembered { get { return memoryScoreWrongNotRemembered; } }

	static int memoryScoreRightVerySure = 200;
	public static int MemoryScoreRightVerySure { get { return memoryScoreRightVerySure; } }

	static int memoryScoreWrongVerySure = -350;
	public static int MemoryScoreWrongVerySure { get { return memoryScoreWrongVerySure; } }

	static int specialObjectPoints = 0;
	public static int SpecialObjectPoints { get { return specialObjectPoints; } }

	static int boxSwapperPoints = 50;
	public static int BoxSwapperPoints { get { return boxSwapperPoints; } }



	public TextMesh verySureScoreExplanation;
	public TextMesh notVerySureScoreExplanation;



	//Time bonus time variables!
	static int timeBonusTimeMin = 22;
	public static int TimeBonusTimeMin { get { return timeBonusTimeMin; } }
	
	static int timeBonusTimeMed = 37;
	public static int TimeBonusTimeMed { get { return timeBonusTimeMed; } }
	
	static int timeBonusTimeMax = 52;
	public static int TimeBonusTimeBig { get { return timeBonusTimeMax; } }



	// Use this for initialization
	void Start () {
		verySureScoreExplanation.text = "win " + memoryScoreRightVerySure + "/" + "lose " + memoryScoreWrongVerySure;
		notVerySureScoreExplanation.text = "win " + memoryScoreRight + "/" + "lose " + memoryScoreWrong;
		StartCoroutine (UpdateScoreText());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void AddToScore(int amountToAdd){
		amountLeftToAdd += amountToAdd;
		score += amountToAdd;
		ExperimentSettings.currentSubject.score = score;
	}

	IEnumerator UpdateScoreText(){
		float timeBetweenUpdates = 0.01f;

		int increment = 3;
		int incrementMult = 1;

		while (true) {

			int absAmountLeftToAdd = Mathf.Abs (amountLeftToAdd);
			while (absAmountLeftToAdd > 0) {

				if (amountLeftToAdd < 0) {
					incrementMult = -1;
				}
				else{
					incrementMult = 1;
				}

				if(absAmountLeftToAdd > increment){
					amountLeftToAdd -= (increment*incrementMult);
					scoreTextScore += (increment*incrementMult);
				} else {
					scoreTextScore += absAmountLeftToAdd;
					amountLeftToAdd = 0;
				}

				scoreText.text = "$ " + (scoreTextScore);

				absAmountLeftToAdd = Mathf.Abs (amountLeftToAdd);

				yield return new WaitForSeconds (timeBetweenUpdates);
			}

			yield return 0;

		}
	}

	public void AddBoxSwapperPoints(){
		AddToScore(boxSwapperPoints);
		scoreLogger.LogBoxSwapperPoints(boxSwapperPoints);
	}

	public void AddSpecialPoints(){
		AddToScore(specialObjectPoints);
		scoreLogger.LogTreasureOpenScoreAdded (specialObjectPoints);
	}

	public int CalculateMemoryPoints (Vector3 correctPosition, bool didRemember, bool doubledDown){
		int memoryPoints = 0;
		if (exp.environmentController.myPositionSelector.GetRadiusOverlap (correctPosition)) {
			if(!didRemember){
				memoryPoints = memoryScoreRightNotRemembered;
			}
			else{
				if(!doubledDown){
					memoryPoints = memoryScoreRight;
				}
				else{
					memoryPoints = memoryScoreRightVerySure;
				}
			}
		}
		else{ //wrong
			if(!didRemember){
				memoryPoints = memoryScoreWrongNotRemembered;
			}
			else{
				if(!doubledDown){
					memoryPoints = memoryScoreWrong;
				}
				else{
					memoryPoints = memoryScoreWrongVerySure;
				}
			}
		}

		AddToScore(memoryPoints);
		scoreLogger.LogMemoryScoreAdded (memoryPoints);

		return memoryPoints;
	}

	public int CalculateTimeBonus(float secondsToCompleteTrial){
		int timeBonusScore = 0;
		if (secondsToCompleteTrial < timeBonusTimeMin) {
			timeBonusScore = timeBonusBig;
		} 
		else if (secondsToCompleteTrial < timeBonusTimeMed) {
			timeBonusScore = timeBonusMed;
		} 
		else if (secondsToCompleteTrial < timeBonusTimeMax) {
			timeBonusScore = timeBonusSmall;
		} 

		AddToScore (timeBonusScore);
		scoreLogger.LogTimeBonusAdded (timeBonusScore);

		return timeBonusScore;

	}

	public void Reset(){
		score = 0;
		scoreTextScore = 0;
		amountLeftToAdd = 0;
		scoreText.text = "$ " + 0;

		scoreLogger.LogScoreReset ();
	}

}
