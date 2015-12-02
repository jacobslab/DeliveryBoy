using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.UI;

public class InstructionsController : MonoBehaviour {
	
	public float timePerInstruction;

	public bool isFinished = false;

	//TextMesh _textMesh;
	public Text text; //TODO: rename this!!!
	public Text oculusText;
	public Color textColorDefault;
	public Color textColorOverlay;
	public Image background;
	public Image oculusBackground;
	public Color backgroundColorDefault;

	public GameObject ScoreInstructions; //turn these on and off as necessary during the trial.......

	List<string> _instructions;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void TurnOffInstructions(){
		SetInstructionsTransparentOverlay();
		SetInstructionsBlank();
	}

	public void SetSingleInstruction(string text, bool isDark){
		if (isDark) {
			SetInstructionsColorful ();
		} else {
			SetInstructionsTransparentOverlay();
		}

		SetText (text);
	}

	void SetText(string newText){
		if(ExperimentSettings.isOculus){
			oculusText.text = newText;
		}
		else{
			text.text = newText;
		}
	}

	public void SetInstructionsBlank(){
		SetText ("");
	}

	public void SetInstructionsColorful(){
		//Debug.Log("set instructions dark");
		if(ExperimentSettings.isOculus){
			oculusBackground.color = backgroundColorDefault;
			oculusText.color = textColorDefault;
		}
		else{
			background.color = backgroundColorDefault;
			text.color = textColorDefault;
		}
	}
	
	public void SetInstructionsTransparentOverlay(){
		//Debug.Log("set instructions transparent overlay");
		if(ExperimentSettings.isOculus){
			oculusBackground.color = new Color(0,0,0,0);
			oculusText.color = textColorOverlay;
		}
		else{
			background.color = new Color(0,0,0,0);
			text.color = textColorOverlay;
		}
	}

	public void RunInstructions(){
		SetInstructionsColorful();
		isFinished = false;
		Parse ();
		StartCoroutine (DisplayReadInText ());
	}

	public void Parse(){
		_instructions = new List<string> ();

		StreamReader reader = new StreamReader ("TextFiles/Instructions.txt");
		string line = reader.ReadLine ();

		string newInstruction = "";

		while (line != null) {
			char[] characters = line.ToCharArray ();
			if(characters.Length > 0){
				if (characters [0] == '%') { //new instruction
					AddInstruction(newInstruction);
					newInstruction = line;
					newInstruction += '\n';
				}
				else {
					newInstruction += line;
					newInstruction += '\n';
				}
			}
			else{
				newInstruction += '\n';
			}

			line = reader.ReadLine ();
		}
		AddInstruction(newInstruction); //add the last instruction

	}

	void AddInstruction(string newInstruction){
		if (newInstruction != "" && newInstruction != null) {
			newInstruction = newInstruction.Replace("%" , ""); //if the 'new instructions symbol' is in the line, take it out.
			_instructions.Add (newInstruction);
		}
	}

	IEnumerator DisplayReadInText(){
		for (int i = 0; i < _instructions.Count; i++) {
			SetText(_instructions[i]);
			yield return StartCoroutine(WaitForInstruction());
		}
		isFinished = true;
	}

	public void DisplayText(string line){
		SetText(line);
	}

	IEnumerator WaitForInstruction(){
		float timePassed = 0;

		bool actionButtonUp = false;

		while (timePassed < timePerInstruction) {
			//want to make sure its a new button press
			if(Input.GetAxis("Action Button") == 0.0f){
				actionButtonUp = true;
			}

			//if button pressed after action button was up -- skip the instruction
			if(Input.GetAxis("Action Button") == 1.0f && actionButtonUp){
				timePassed += timePerInstruction; // will skip instruction!
			}

			//otherwise, increment the timePassed
			timePassed += Time.deltaTime;
			yield return 0;
		}
	}


}
