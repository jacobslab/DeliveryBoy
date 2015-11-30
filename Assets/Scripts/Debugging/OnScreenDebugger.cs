using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OnScreenDebugger : MonoBehaviour {

	public Text DebugText;

	bool isEnabled = false;


	// Use this for initialization
	void Start () {
		if (isEnabled) {
			TurnOnDebugText ();
		} 
		else {
			TurnOffDebugText ();
		}
	}

	void UpdateDebugText(){
		DebugText.text = "FPS: " + (1.0f / Time.smoothDeltaTime);
	}

	void TurnOnDebugText(){
		DebugText.enabled = true;
	}

	void TurnOffDebugText(){
		DebugText.enabled = false;
	}

	// Update is called once per frame
	void Update () {
		UpdateDebugText (); 
		CheckForEnableKey ();
	}

	void CheckForEnableKey(){
		if (Input.GetKeyDown (KeyCode.Z)) { //D for debugging was taken for WASD movement.
			Toggle();
		}
	}

	void Toggle(){
		isEnabled = !isEnabled; //toggle the bool

		//turn debug text on or off depending on the new bool
		if (isEnabled) {
			TurnOnDebugText();
		}
		else {
			TurnOffDebugText();
		}
	}
}
