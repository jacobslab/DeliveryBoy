using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIImageLogTrack: LogTrack {

	Image myImage;
	Sprite currentSprite;
	Color currentColor = Color.black;

	bool firstLog = false; //should make an initial log

	// Use this for initialization
	void Awake () {
		myImage = GetComponent<Image> ();
		currentSprite = myImage.sprite;
		currentColor = myImage.color;
	}

	//log on late update so that everything for that frame gets set first
	void LateUpdate () {
		if (myImage == null) {
			Debug.Log("UI Image not found!");
		}
		if (!firstLog) {
			firstLog = true;
			LogImage ();
			LogColor();
		}
		if(ExperimentSettings.isLogging && ( currentSprite != myImage.sprite ) ){ //if the text has changed, log it!
			LogImage ();
		}
		if(ExperimentSettings.isLogging && ( currentColor != myImage.color ) ){ //if the text has changed, log it!
			LogColor ();
		}
	}

	void LogImage()
	{
		if (myImage.sprite != null) {
			currentSprite = myImage.sprite;
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), gameObject.name + separator + "IMAGE" + separator + currentSprite.name.ToString ());
		} else {
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), gameObject.name + separator + "IMAGE" + separator + "None");
		}
	}



	void LogColor(){

		currentColor = myImage.color;
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), gameObject.name + separator + "IMAGE_COLOR" + separator + currentColor.r + separator + currentColor.g + separator + currentColor.b + separator + currentColor.a);
	}
}