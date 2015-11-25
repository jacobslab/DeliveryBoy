using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnvironmentPositionSelector : MonoBehaviour {

	Experiment exp { get { return Experiment.Instance; } }

	public GameObject PositionSelector;
	public GameObject PositionSelectorVisuals;
	public GameObject CorrectPositionIndicator;

	public PositionSelectorLogTrack logTrack;


	public Color VisualsDefaultColor;
	public Color VisualsSelectColor;
	
	bool shouldSelect;
	float selectionMovementSpeed = 80.0f;

	void Awake(){
		PositionSelectorVisuals.transform.localScale = new Vector3 (Config_CoinTask.selectionDiameter, PositionSelectorVisuals.transform.localScale.y, Config_CoinTask.selectionDiameter);
	}

	// Use this for initialization
	void Start () {
		EnableSelection(false);
	}
	
	// Update is called once per frame
	void Update () {
		if (shouldSelect) {
			GetMovementInput();
		}
	}

	public IEnumerator ChoosePosition(){
		//change position selector visual colors!
		yield return StartCoroutine (PositionSelectorVisuals.GetComponent<ColorChanger> ().LerpChangeColor(VisualsSelectColor, 0.2f));
		PositionSelectorVisuals.GetComponent<ColorChanger> ().ChangeColor (VisualsDefaultColor);
		
	}

	public void Reset(){
		PositionSelector.transform.position = GetStartPosition();
	}

	Vector3 GetStartPosition(){
		Vector3 envCenter = exp.environmentController.GetEnvironmentCenter ();
		Vector3 newStartPos = new Vector3 (envCenter.x, PositionSelector.transform.position.y, envCenter.z);

		return newStartPos;
	}

	void GetMovementInput(){
		float verticalAxisInput = Input.GetAxis ("Vertical");
		float horizontalAxisInput = Input.GetAxis ("Horizontal");

		float epsilon = 0.1f;
		bool positionCloseToTower1 = CheckPositionsClose (epsilon, exp.player.transform.position, exp.player.controls.towerPositionTransform1.position);
		bool positionCloseToTower2 = CheckPositionsClose (epsilon, exp.player.transform.position, exp.player.controls.towerPositionTransform2.position);

		if (positionCloseToTower1) {
			Move (verticalAxisInput * selectionMovementSpeed * Time.deltaTime, horizontalAxisInput * selectionMovementSpeed * Time.deltaTime);
		} 
		else if (positionCloseToTower2) {
			Move (-verticalAxisInput * selectionMovementSpeed * Time.deltaTime, -horizontalAxisInput * selectionMovementSpeed * Time.deltaTime);
		}
	}

	bool CheckPositionsClose(float epsilon, Vector3 pos1, Vector3 pos2){
		float distance = (pos1 - pos2).magnitude;
		if (distance < epsilon) {
			return true;
		}
		return false;
	}

	void Move(float amountVertical, float amountHorizontal){
		float epsilon = 0.01f;

		Vector3 vertAmountVec = PositionSelector.transform.forward * amountVertical;
		Vector3 horizAmountVec = PositionSelector.transform.right * amountHorizontal;

		bool wouldBeInWallsVert = exp.environmentController.CheckWithinWallsVert (PositionSelector.transform.position + (vertAmountVec), Config_CoinTask.objectToWallBuffer);
		bool wouldBeInWallsHoriz = exp.environmentController.CheckWithinWallsHoriz (PositionSelector.transform.position + (horizAmountVec), Config_CoinTask.objectToWallBuffer); 


		if (wouldBeInWallsVert) {
			PositionSelector.transform.position += vertAmountVec;
		} else {
			//move to edge
			if( amountVertical < -epsilon ){

				float vertDist = exp.environmentController.GetDistanceFromEdge( PositionSelector, Config_CoinTask.objectToWallBuffer, -PositionSelector.transform.forward);
				PositionSelector.transform.position -= PositionSelector.transform.forward*vertDist;
			}
			else if(amountVertical > epsilon ){
				float vertDist = exp.environmentController.GetDistanceFromEdge( PositionSelector, Config_CoinTask.objectToWallBuffer, PositionSelector.transform.forward);
				PositionSelector.transform.position -= PositionSelector.transform.forward*vertDist;
			}
		}
		if (wouldBeInWallsHoriz) {
			PositionSelector.transform.position += horizAmountVec;
		} else {
			//move to edge
			if( amountHorizontal < -epsilon ){
				float horizDist = exp.environmentController.GetDistanceFromEdge( PositionSelector, Config_CoinTask.objectToWallBuffer, -PositionSelector.transform.right);
				PositionSelector.transform.position -= PositionSelector.transform.right*horizDist;
			}
			else if( amountHorizontal > epsilon ){
				float horizDist = exp.environmentController.GetDistanceFromEdge( PositionSelector, Config_CoinTask.objectToWallBuffer, PositionSelector.transform.right);
				PositionSelector.transform.position -= PositionSelector.transform.right*horizDist;
			}
		}
	}



	public bool GetRadiusOverlap(Vector3 correctPosition){
		float distance = (correctPosition - PositionSelector.transform.position).magnitude;
		float positionSelectorRadius = PositionSelectorVisuals.transform.localScale.x / 2.0f;
		if (distance < positionSelectorRadius) {
			return true;
		}

		return false;
	}

	public Vector3 GetSelectorPosition(){
		return PositionSelector.transform.position;
	}
	
	public void EnableSelection(bool shouldEnable){
		shouldSelect = shouldEnable;
		EnableSelectionIndicator (shouldEnable);
	}

	void EnableSelectionIndicator(bool shouldEnable){
		PositionSelectorVisuals.GetComponent<VisibilityToggler> ().TurnVisible (shouldEnable);
	}
	
}
