using UnityEngine;
using System.Collections;

public class Subject {
	
	public string name;
	public int score;
	public int trials;

	public Subject(){

	}

	public Subject(string newName, int newScore, int newBlock){
		name = newName;
		score = newScore;
		trials = newBlock;
	}

	public void IncrementTrial () {
		Debug.Log("incrementing session");
		trials++;
	}
	
	/*public void AddScore ( int scoreToAdd ) {
		Debug.Log("adding to score");
		score += scoreToAdd;
	}*/

}
