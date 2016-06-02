using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class TextLogTrack : LogTrack {

	Text myText;
	string currentText = "";
	Color currentColor;

	bool firstLog = false; //should make an initial log

	// Use this for initialization
	void Awake () {
		myText = GetComponent<Text> ();
	}
	
	//log on late update so that everything for that frame gets set first
	void LateUpdate () {
		if (myText == null) {
			Debug.Log("Text is null! Did you mean to add a TextMeshLogTrack instead?");
		}
		if(ExperimentSettings.isLogging && ( currentText != myText.text || currentColor != myText.color || !firstLog) ){ //if the text has changed, log it!
			firstLog = true;
			LogText ();
			LogColor();
		}
	}

	void LogText(){
		currentText = myText.text;

		string textToLog = myText.text;

		if (myText.text == "") {
			textToLog = " "; //log a space -- makes it easier to read it during replay!
		}
		else {
			textToLog = Regex.Replace( textToLog, "\r?\n", "_NEWLINE_"); //will replace line breaks on mac or windows with _NEWLINE_ for easier log file parsing.
		}

		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), gameObject.name + separator + "TEXT" + separator + textToLog );
	}

	void LogColor(){
		currentColor = myText.color;
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), gameObject.name + separator + "TEXT_COLOR" + separator + myText.color.r + separator + myText.color.g + separator + myText.color.b + separator + myText.color.a);
	}
}