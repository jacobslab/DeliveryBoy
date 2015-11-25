using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class UIPanelLogTrack : LogTrack {

	Image myPanelImage;
	Color currentPanelColor;

	bool firstLog = false; //should do an initial log
	

	// Use this for initialization
	void Start () {
		myPanelImage = GetComponent<Image> ();
	}
	
	//log on late update so that everything for that frame gets set first
	void LateUpdate () {
		if(ExperimentSettings.isLogging && (currentPanelColor != myPanelImage.color || !firstLog) ){ //if the color has changed, or it's the first log
			firstLog = true;
			LogPanel();
		}
	}

	void LogPanel(){
		currentPanelColor = myPanelImage.color;
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), gameObject.name 
		                   + separator + "PANEL" + separator + myPanelImage.color.r + separator + myPanelImage.color.g + separator + myPanelImage.color.b + separator + myPanelImage.color.a);
	}
}
