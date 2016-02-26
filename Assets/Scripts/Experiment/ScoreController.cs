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

	static int timeBonusBig = 50;
	public static int TimeBonusBig { get { return timeBonusBig; } }

	//Time bonus time variables!
	static int timeBonusTimeMin = 25;
	public static int TimeBonusTimeMin { get { return timeBonusTimeMin; } }



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

	public int CalculateTimeBonus(float secondsToCompleteTrial){
		int timeBonusScore = 0;
		if (secondsToCompleteTrial < timeBonusTimeMin) {
			timeBonusScore = timeBonusBig;
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
