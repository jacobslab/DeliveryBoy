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
	/*static int timeBonusSmall = 10;
	public static int TimeBonusSmall { get { return timeBonusSmall; } }

	static int timeBonusMed = 20;
	public static int TimeBonusMed { get { return timeBonusMed; } }
*/
	static int timeBonusBig = 50;
	public static int TimeBonusBig { get { return timeBonusBig; } }
	
	/*
	static int memoryScoreRight = 100;
	public static int MemoryScoreRight { get { return memoryScoreRight; } }

	static int memoryScoreWrong = -50;
	public static int MemoryScoreWrong { get { return memoryScoreWrong; } }
*/

	//Time bonus time variables!
	static int timeBonusTimeMin = 25;
	public static int TimeBonusTimeMin { get { return timeBonusTimeMin; } }
	/*
	static int timeBonusTimeMed = 37;
	public static int TimeBonusTimeMed { get { return timeBonusTimeMed; } }
	
	static int timeBonusTimeMax = 52;
	public static int TimeBonusTimeBig { get { return timeBonusTimeMax; } }
*/


	// Use this for initialization
	void Start () {
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

	bool isCorrect(){
		//TODO: fill in however you'd like.
		return true;
	}

	/*public int CalculateMemoryPoints (){
		int memoryPoints = 0;
		if (isCorrect()) {
			memoryPoints = memoryScoreRight;
		}
		else{ //wrong
			memoryPoints = memoryScoreWrong;
		}

		AddToScore(memoryPoints);
		scoreLogger.LogMemoryScoreAdded (memoryPoints);

		return memoryPoints;
	}*/

	public int CalculateTimeBonus(float secondsToCompleteTrial){
		int timeBonusScore = 0;
		if (secondsToCompleteTrial < timeBonusTimeMin) {
			timeBonusScore = timeBonusBig;
		} 
		/*else if (secondsToCompleteTrial < timeBonusTimeMed) {
			timeBonusScore = timeBonusMed;
		} 
		else if (secondsToCompleteTrial < timeBonusTimeMax) {
			timeBonusScore = timeBonusSmall;
		} */

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
