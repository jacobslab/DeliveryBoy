using UnityEngine;
using System.Collections;

public class ControllerInputTester : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	//COMMENTS ARE FOR LOGITECH CONTROLLER
	void Update () {

		//DPAD -- X & Y Axes
		if(Input.GetAxis("Horizontal") > 0){
			Debug.Log("HEYO HORIZONTAL POS: " + Input.GetAxis("Horizontal"));
		}
		else if(Input.GetAxis("Horizontal") < 0){
			Debug.Log("HEYO HORIZONTAL NEG: " + Input.GetAxis("Horizontal"));
		}

		if(Input.GetAxis("Vertical") > 0){
			Debug.Log("HEYO VERTICAL POS: " + Input.GetAxis("Vertical"));
		}
		else if(Input.GetAxis("Vertical") < 0){
			Debug.Log("HEYO VERTICAL NEG: " + Input.GetAxis("Vertical"));
		}

		//3rd axis
/*		if(Input.GetAxis("HorizontalLeftJoystick") > 0){
			Debug.Log("HEYO HORIZONTAL POS");
		}
		else if(Input.GetAxis("HorizontalLeftJoystick") < 0){
			Debug.Log("HEYO HORIZONTAL NEG");
		}

		//4th axis
		if(Input.GetAxis("VerticalLeftJoystick") > 0){
			Debug.Log("HEYO VERTICAL POS");
		}
		else if(Input.GetAxis("VerticalLeftJoystick") < 0){
			Debug.Log("HEYO VERTICAL NEG");
		}*/

		/*//5th axis
		if(Input.GetAxis("HorizontalRightJoystick") > 0){
			Debug.Log("HEYO HORIZONTAL POS");
		}
		else if(Input.GetAxis("HorizontalRightJoystick") < 0){
			Debug.Log("HEYO HORIZONTAL NEG");
		}
			
		//6th axis
		if(Input.GetAxis("VerticalRightJoystick") > 0){
			Debug.Log("HEYO VERTICAL POS");
		}
		else if(Input.GetAxis("VerticalRightJoystick") < 0){
			Debug.Log("HEYO VERTICAL NEG");
		}
*/

		if(Input.GetKey(KeyCode.JoystickButton0)){ //X
			Debug.Log("BUTTON: 0");
		}
		if(Input.GetKey(KeyCode.JoystickButton1)){ //A
			Debug.Log("BUTTON: 1");
		}
		if(Input.GetKey(KeyCode.JoystickButton2)){ //B
			Debug.Log("BUTTON: 2");
		}
		if(Input.GetKey(KeyCode.JoystickButton3)){ //Y
			Debug.Log("BUTTON: 3");
		}
		if(Input.GetKey(KeyCode.JoystickButton4)){ //Left Bumper
			Debug.Log("BUTTON: 4");
		}
		if(Input.GetKey(KeyCode.JoystickButton5)){ //Right Bumper
			Debug.Log("BUTTON: 5");
		}
		if(Input.GetKey(KeyCode.JoystickButton6)){ //Left Trigger
			Debug.Log("BUTTON: 6");
		}
		if(Input.GetKey(KeyCode.JoystickButton7)){ //Right Trigger
			Debug.Log("BUTTON: 7");
		}
		if (Input.GetKey (KeyCode.Joystick8Button8)) {
			Debug.Log("BUTTON: 8");
		}
		if (Input.GetKey (KeyCode.Joystick8Button9)) {
			Debug.Log("BUTTON: 9");
		}
		if (Input.GetKey (KeyCode.Joystick8Button10)) {
			Debug.Log("BUTTON: 10");
		}
		if (Input.GetKey (KeyCode.Joystick8Button11)) {
			Debug.Log("BUTTON: 11");
		}
		if (Input.GetKey (KeyCode.Joystick8Button12)) {
			Debug.Log("BUTTON: 12");
		}
		if (Input.GetKey (KeyCode.Joystick8Button13)) {
			Debug.Log("BUTTON: 13");
		}
		if (Input.GetKey (KeyCode.Joystick8Button14)) {
			Debug.Log("BUTTON: 14");
		}
		if (Input.GetKey (KeyCode.Joystick8Button15)) {
			Debug.Log("BUTTON: 15");
		}
		if (Input.GetKey (KeyCode.Joystick8Button16)) {
			Debug.Log("BUTTON: 16");
		}
		if (Input.GetKey (KeyCode.Joystick8Button17)) {
			Debug.Log("BUTTON: 17");
		}
		if (Input.GetKey (KeyCode.Joystick8Button18)) {
			Debug.Log("BUTTON: 18");
		}
		if (Input.GetKey (KeyCode.Joystick8Button19)) {
			Debug.Log("BUTTON: 19");
		}


	}
}
