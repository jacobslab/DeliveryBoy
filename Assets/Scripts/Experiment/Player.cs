using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	Experiment exp { get { return Experiment.Instance; } }

	public PlayerControls controls;
	public EnvironmentPositionSelector positionSelector;
	public GameObject visuals;


	float arrowAngleThreshold = 30.0f; //minimum (absolute) angle between player and chest before arrows should be turned on
	bool rightArrowsOn = true;
	bool leftArrowsOn = true;

	public GameObject rightArrows;
	EnableChildrenLogTrack rightArrowEnableLog;
	public GameObject leftArrows;
	EnableChildrenLogTrack leftArrowEnableLog;
	

	// Use this for initialization
	void Start () {
		rightArrowEnableLog = rightArrows.GetComponent<EnableChildrenLogTrack> ();
		leftArrowEnableLog = leftArrows.GetComponent<EnableChildrenLogTrack> ();

		TurnOnRightArrows (false);
		TurnOnLeftArrows (false);
	}
	
	// Update is called once per frame
	void Update () {
		SetArrows ();
	}

	public void TurnOnVisuals(bool isVisible){
		visuals.SetActive (isVisible);
	}

	void SetArrows(){
		if ( exp.trialController.currentDefaultObject ) {
			Vector2 currentDefaultPosXZ = new Vector2 ( exp.trialController.currentDefaultObject.transform.position.x, exp.trialController.currentDefaultObject.transform.position.z );

			float angleBetweenPlayerAndTreasure = controls.GetYAngleBetweenFacingDirAndObjectXZ( currentDefaultPosXZ );

			//if the angle is bigger than the threshold, turn on the appropriate arrows
			if( Mathf.Abs(angleBetweenPlayerAndTreasure) > arrowAngleThreshold){
				if(angleBetweenPlayerAndTreasure < 0){
					TurnOnLeftArrows(true);
					TurnOnRightArrows(false);
				}
				else{
					TurnOnRightArrows(true);
					TurnOnLeftArrows(false);
				}
			}
			//if smaller than the threshold, turn off all arrows
			else{
				TurnOnLeftArrows(false);
				TurnOnRightArrows(false);
			}
		}
		else if(rightArrowsOn){
			TurnOnRightArrows(false);
		}
		else if(leftArrowsOn){
			TurnOnLeftArrows(false);
		}
	}

	public void TurnOnRightArrows(bool shouldTurnOn){
		//only toggle them if they aren't in the shouldTurnOn state
		if (rightArrowsOn == !shouldTurnOn) {
			UsefulFunctions.EnableChildren (rightArrows.transform, shouldTurnOn);
			rightArrowEnableLog.LogChildrenEnabled (shouldTurnOn);
			rightArrowsOn = shouldTurnOn;
		}
	}

	public void TurnOnLeftArrows(bool shouldTurnOn){
		//only toggle them if they aren't in the shouldTurnOn state
		if (leftArrowsOn == !shouldTurnOn) {
			UsefulFunctions.EnableChildren (leftArrows.transform, shouldTurnOn);
			leftArrowEnableLog.LogChildrenEnabled (shouldTurnOn);
			leftArrowsOn = shouldTurnOn;
		}
	}


}
