using UnityEngine;
using System.Collections;

//TODO: add some sort of a check if a controller is attached??? or a menu toggle for keyboard vs. joystick?
//LOG CUSTOM MESSAGES. should be accessed through some global class.
public class LogitechControllerLogTrack : LogTrack {

	int numButtons = 20;
	int numJoystickAxesUsed = 2; //DPAD, LEFT JOYSTICK

	// Use this for initialization
	void Start () {
		
	}
	
	void Update(){ //can be called in update because we are checking for input. other logtracks use LateUpdate because things like positions must be finished updating before they are logged.
		string[] joystickNames = Input.GetJoystickNames ();
		int numControllers = joystickNames.Length;
		if (ExperimentSettings.isLogging && numControllers > 0) {
			if (joystickNames [0] != "") {
				LogController ();
			}
		}
	}

	void LogController(){

		float[] joystickInput = GetLogitechControllerJoystickInput ();
		bool[] buttonInput = GetLogitechControllerButtonInput ();

		for (int i = 0; i < numJoystickAxesUsed; i++) {
			if(joystickInput[i] != 0){
				subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Joystick" + i + separator + joystickInput [i]);
			}
		}

		for (int i = 0; i < numButtons; i++) {
			if(buttonInput[i] == true){
				subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), "Button" + i + separator + buttonInput [i]);
			}
		}
	}

	bool[] GetLogitechControllerButtonInput(){
		bool[] buttonInput = new bool[numButtons];
		int button = 0;

		buttonInput[button++] = Input.GetKey(KeyCode.JoystickButton0);
		buttonInput[button++] = Input.GetKey(KeyCode.JoystickButton1);
		buttonInput[button++] = Input.GetKey(KeyCode.JoystickButton2);
		buttonInput[button++] = Input.GetKey(KeyCode.JoystickButton3);
		buttonInput[button++] = Input.GetKey(KeyCode.JoystickButton4);
		buttonInput[button++] = Input.GetKey(KeyCode.JoystickButton5);
		buttonInput[button++] = Input.GetKey(KeyCode.JoystickButton6);
		buttonInput[button++] = Input.GetKey(KeyCode.JoystickButton7);
		buttonInput[button++] = Input.GetKey(KeyCode.JoystickButton8);
		buttonInput[button++] = Input.GetKey(KeyCode.JoystickButton9);
		buttonInput[button++] = Input.GetKey(KeyCode.JoystickButton10);
		buttonInput[button++] = Input.GetKey(KeyCode.JoystickButton11);
		buttonInput[button++] = Input.GetKey(KeyCode.JoystickButton12);
		buttonInput[button++] = Input.GetKey(KeyCode.JoystickButton13);
		buttonInput[button++] = Input.GetKey(KeyCode.JoystickButton14);
		buttonInput[button++] = Input.GetKey(KeyCode.JoystickButton15);
		buttonInput[button++] = Input.GetKey(KeyCode.JoystickButton16);
		buttonInput[button++] = Input.GetKey(KeyCode.JoystickButton17);
		buttonInput[button++] = Input.GetKey(KeyCode.JoystickButton18);
		buttonInput[button++] = Input.GetKey(KeyCode.JoystickButton19);

		return buttonInput;
	}

	float[] GetLogitechControllerJoystickInput(){
		float[] axesInput = new float[numJoystickAxesUsed]; //will alternate: 
		int axis = 0;

		axesInput[axis++] = Input.GetAxis("Horizontal"); //left horizontal X AXIS
		axesInput[axis++] = Input.GetAxis("Vertical"); //left vertical Y AXIS
		//axesInput[axis++] = Input.GetAxis("HorizontalLeftJoystick"); //3rd AXIS
		//axesInput[axis++] = Input.GetAxis("VerticalLeftJoystick"); //4th AXIS

		return axesInput;
	}

}
