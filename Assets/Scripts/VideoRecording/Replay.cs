using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text.RegularExpressions;


public class Replay : MonoBehaviour {

	Experiment exp { get { return Experiment.Instance; } }

	//image recording
	public ScreenRecorder MyScreenRecorder;
	public Toggle PNGToggle;
	static bool isRecording;

	//GUI
	public InputField LogFilePathInputField;
	public InputField FPSInputField; //Frames Per Second
	static int FPS = 30; //use 30 for default


	//I/O
	StreamReader fileReader;
	static string logFilePath;
	string currentLogFileLine;


	//keeping track of objects
	Dictionary<String, GameObject> objsInSceneDict;


	//a bool to determine if we should start the log file processing. replay should start once 
	static bool shouldStartProcessingLog = false;

	// Use this for initialization
	void Start () {
		objsInSceneDict = new Dictionary<String, GameObject> ();
	}
	
	// Update is called once per frame
	void Update () {

		if (ExperimentSettings.isReplay && Application.loadedLevel == 1) { //if we're in the main scene, which in the case of YC1 corresponds to 1
			if( shouldStartProcessingLog ){
				//process log file;
				Debug.Log("PROCESSING LOG FILE OH HEYYYY");
				StartCoroutine(ProcessLogFile());
				
				shouldStartProcessingLog = false;
			}
		}
	}

	public void SetIsRecording(){
		isRecording = PNGToggle.isOn;
	}

	public void ReplayScene(){ //gets called via replay button in the main menu scene
		objsInSceneDict.Clear ();

		if (LogFilePathInputField != null) {
			SetLogFile (LogFilePathInputField.text);
		}

		if (FPSInputField != null) {
			SetFPS (int.Parse(FPSInputField.text));
		}

		
		try 
		{
			// Create an instance of StreamReader to read from a file. 
			// The using statement also closes the StreamReader. 
			using (fileReader = new StreamReader (logFilePath)) 
			{
				//open scene
				ExperimentSettings.Instance.SetReplayTrue();
				SceneController.Instance.LoadExperiment(); //will load the experiment scene. Experiment.cs will not run the experiment because ExperimentSettings.isReplay = true!
				
				shouldStartProcessingLog = true;
			}
		}
		catch (Exception e) 
		{
			Debug.Log("Invalid log file path. Cannot replay." + e.StackTrace);
		}
		


	}

	void SetLogFile(string chosenLogFile){
		logFilePath = chosenLogFile;
		Debug.Log (logFilePath);
	}

	void SetFPS(int newFPS){
		FPS = newFPS;
	}


	//TODO: make log file just log the time elapsed????
	long GetMillisecondDifference(long baseMS, long newMS){
		return (newMS - baseMS); 
	}

	void RecordScreenShot(){
		if(MyScreenRecorder != null){
			//will check if it's supposed to record or not
			//also will wait until endofframe in order to take the shot
			MyScreenRecorder.TakeNextContinuousScreenShot();
		}
		else{
			Debug.Log("No screen recorder attached!");
		}
	}
	
	//THIS PARSING DEPENDS GREATLY ON THE FORMATTING OF THE LOG FILE.
	//IF THE FORMATTING OF THE LOG FILE IS CHANGED, THIS WILL VERY LIKELY HAVE TO CHANGE AS WELL.
	IEnumerator ProcessLogFile(){

		long currentFrame = 0;
		long currentTimeStamp = 0;
		long lastTimeRecorded = 0;
		long timeDifference = 0;


		//if (logFilePath != "") { 

		if (FPS == 0) {
			Debug.Log("ERROR: SET FRAMES PER SECOND TO SOMETHING NOT ZERO.");
		}
		
		float secondsPerFrame = 1.0f / (float)FPS;
		int millisecondsPerFrame = Mathf.RoundToInt( secondsPerFrame * 1000 );
		Debug.Log ("MS per frame!: " + millisecondsPerFrame + "FPS: " + FPS);


		fileReader = new StreamReader (logFilePath);
	
		currentLogFileLine = fileReader.ReadLine (); //the first line in the file should be the date.
		currentLogFileLine = fileReader.ReadLine (); //the second line should be the first real line with logged data

		string[] splitLine;

		bool hasFinishedSettingFrame = false;

		char splitCharacter = Logger_Threading.LogTextSeparator.ToCharArray () [0];

		//PARSE
		while (currentLogFileLine != null) {

			splitLine = currentLogFileLine.Split(splitCharacter);

			if(splitLine.Length > 0){
				for (int i = 0; i < splitLine.Length; i++){

					//0 -- timestamp
					if (i == 0){

						//Debug.Log(currentFrame + " " + splitLine[0] + " " + splitLine[1] + " " + splitLine[2]);

						currentTimeStamp = long.Parse(splitLine[i]);
						timeDifference = currentTimeStamp - lastTimeRecorded; //gets time between log file lines
					}

					//1 -- frame
					else if(i == 1){
						long readFrame = long.Parse(splitLine[i]);
						
						while(readFrame != currentFrame){
							currentFrame++;
							hasFinishedSettingFrame = true;
							
							//Debug.Log(currentFrame);
						}
						
						//first frame case -- need to set the last time recorded as the current time stamp
						if (currentFrame == 1 && hasFinishedSettingFrame){ //record frame 0 before reading and setting the rest of frame 1...
							lastTimeRecorded = currentTimeStamp;
							
							if(isRecording){
								RecordScreenShot();
							}
							yield return 0; //advance the game a frame before continuing
							
						}
						else if(timeDifference > millisecondsPerFrame){
							int numFramesToCapture = Mathf.FloorToInt( (float)timeDifference / millisecondsPerFrame ); //EXAMPLE: if time passed is 30 milliseconds and the required time per frame is 15 milliseconds, you should capture 2 frames
							
							//record and wait the appropriate number of frames
							for(int j = 0; j < numFramesToCapture; j++){
								if(isRecording){
									RecordScreenShot();
								}
								yield return 0; //advance the game a frame before continuing
							}
							
							long timeToAddToLastTimeStamp = numFramesToCapture*millisecondsPerFrame; //EXAMPLE: if you capture 2 frames, add 2 frames worth of time to the last time that we recorded a frame to a PNG
							lastTimeRecorded += timeToAddToLastTimeStamp;
							
							//DEBUG TO CHECK THAT THE TIME INCREMENT IS WORKING PROPERLY
							//Debug.Log("Time to record screenshot. Frame: " + currentFrame + " Time Stamp: " + currentTimeStamp + " Last Time Recorded: " + lastTimeRecorded + " Time Difference: " + timeDifference + " Num Frames Recorded: " + numFramesToCapture);
						}
					}

					//2 -- name of object
					else if (i == 2){
						string objName = splitLine[i];
						
						if(objName != "Mouse" && objName != "Keyboard" && objName != "Trial Info"){

							GameObject objInScene;
								
							if(objsInSceneDict.ContainsKey(objName)){
								
								objInScene = objsInSceneDict[objName];

								if(objInScene == null){
									//Object got destroyed before it could be removed from the list.
									//This can happen in cases like an auto-destructing particle emitter.
									//...So we should remove it from the objsInSceneDict.
									if(splitLine[i+1] == "DESTROYED"){
										objsInSceneDict.Remove(objName);
									}
								}

							}
							else{

								objInScene = GameObject.Find(objName);

								if(objInScene != null){
									objsInSceneDict.Add(objName, objInScene);
								}
								else{ //if the object is not in the scene, but is in the log file, we should instantiate it!
										//we could also check for the SPAWNED keyword

									//TODO: use new functions in spawnable object?
									//separate out the object name from a numeric ID
									Regex numAlpha = new Regex("(?<Alpha>[a-zA-Z ']*)(?<Numeric>[0-9]*)");
									Match match = numAlpha.Match(objName);
									string objShortName = match.Groups["Alpha"].Value;
									string objID = match.Groups["Numeric"].Value;


									objInScene = exp.objectController.ChooseSpawnableObject(objShortName);
									
									if(objInScene != null){ //if it did grab the prefab...

										objInScene = exp.objectController.SpawnObject(objInScene, Vector3.zero); //position and rotation should be set next...
										SpawnableObject objInSceneSpawnable = objInScene.GetComponent<SpawnableObject>();
										objInScene.name = objInSceneSpawnable.GetName();

										//ID's are in the format 000 - 999
										char[] splitNum = objID.ToCharArray();
										if(splitNum.Length > 0){
											int numHundreds = int.Parse(splitNum[0].ToString());
											int numTens = int.Parse(splitNum[1].ToString());
											int numOnes = int.Parse(splitNum[2].ToString());
											int objIDint = (numHundreds*100) + (numTens*10) + numOnes;

											objInSceneSpawnable.SetNameID(objIDint);
										}

										objsInSceneDict.Add(objName, objInScene);
									}

								}

							}
							if(objInScene != null){
								//NOW MOVE & ROTATE THE OBJECT.
								string loggedProperty = splitLine[i+1];
								
								if(loggedProperty == "POSITION"){
									
									float posX = float.Parse(splitLine[i+2]);
									float posY = float.Parse(splitLine[i+3]);
									float posZ = float.Parse(splitLine[i+4]);
									
									objInScene.transform.position = new Vector3(posX, posY, posZ);
									
								}
								else if(loggedProperty == "ROTATION"){
									
									float rotX = float.Parse(splitLine[i+2]);
									float rotY = float.Parse(splitLine[i+3]);
									float rotZ = float.Parse(splitLine[i+4]);

									objInScene.transform.rotation = Quaternion.Euler(rotX, rotY, rotZ);

								}
								else if(loggedProperty == "SCALE"){
									
									float scaleX = float.Parse(splitLine[i+2]);
									float scaleY = float.Parse(splitLine[i+3]);
									float scaleZ = float.Parse(splitLine[i+4]);
									
									objInScene.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
									
								}

								else if(loggedProperty == "ENABLED_CHILDREN"){
									string shouldEnableString = splitLine[i+2];
									bool shouldEnable = true;
									if(shouldEnableString == "False"){
										shouldEnable = false;
									}

									UsefulFunctions.EnableChildren( objInScene.transform, shouldEnable );
									
								}

								else if(loggedProperty == "VISIBILITY"){
									VisibilityToggler visibilityToggler = objInScene.GetComponent<VisibilityToggler>();
									if(visibilityToggler != null){
										bool visibleState = true;
										if(splitLine[i+2] == "false" || splitLine[i+2] == "False"){
											visibleState = false;
										}
										visibilityToggler.TurnVisible(visibleState);
									}
									else{
										Debug.Log("no visibility toggler!");
									}
								}

								else if(loggedProperty == "OBJECT_COLOR"){
									Renderer renderer = objInScene.GetComponent<Renderer>();
									if(renderer != null){
										float r = float.Parse(splitLine[i+2]);
										float g = float.Parse(splitLine[i+3]);
										float b = float.Parse(splitLine[i+4]);
										float a = float.Parse(splitLine[i+5]);

										Color color = new Color(r, g, b, a);

										renderer.material.color = color;
									}
									else{
										Debug.Log("no renderer!");
									}
								}

								else if(loggedProperty == "CANVAS_GROUP_ALPHA"){
									CanvasGroup canvasGroup = objInScene.GetComponent<CanvasGroup>();
									if(canvasGroup != null){
										float alpha = float.Parse(splitLine[i+2]);
										
										canvasGroup.alpha = alpha;
									}
									else{
										Debug.Log("no canvas group!");
									}
								}

								//SHADOW CAST CHANGE
								else if (loggedProperty == "SHADOW_SETTING"){
									string shadowSetting = splitLine[i+2];
									bool shouldSetShadows = true;
									if(shadowSetting == "Off"){
										shouldSetShadows = false;
									}
									//set the shadow setting
									objInScene.GetComponent<SpawnableObject>().SetShadowCasting(shouldSetShadows);
								}
								
								//LAYER CHANGE
								else if (loggedProperty == "LAYER_CHANGE"){
									int newLayer = int.Parse(splitLine[i+2]);
									
									//set the layer
									UsefulFunctions.SetLayerRecursively(objInScene, newLayer);
								}

								else if(loggedProperty == "CAMERA_ENABLED"){
									Camera objCamera = objInScene.GetComponent<Camera>();
									if(objCamera != null){
										if(splitLine[i+2] == "true" || splitLine[i+2] == "True"){
											objCamera.enabled = true;
										}
										else{
											objCamera.enabled = false;
										}
									}
								}
								else if(loggedProperty == "DESTROYED"){
									Debug.Log("Destroying object! " + objInScene.name);
									objsInSceneDict.Remove( objName );
									GameObject.Destroy(objInScene);
								}

								//TEXT_MESH TEXT
								else if(loggedProperty == "TEXT_MESH"){
									TextMesh text = objInScene.GetComponent<TextMesh>();
									text.text = "";
									for(int j = i+2; j < splitLine.Length; j++){ //the text may have been split unnecessarily if there is a splitCharacter in the text
										text.text += splitLine[j]; //add each piece of the split text
										if(j+1 < splitLine.Length){
											text.text += Logger_Threading.LogTextSeparator; //add back the split characters into the text!
										}
									}
									
									text.text = text.text.Replace("_NEWLINE_", "\n");
								}
								else if(loggedProperty == "TEXT_MESH_COLOR"){
									TextMesh text = objInScene.GetComponent<TextMesh>();
									float r = float.Parse(splitLine[i+2]);
									float g = float.Parse(splitLine[i+3]);
									float b = float.Parse(splitLine[i+4]);
									float a = float.Parse(splitLine[i+5]);
									text.color = new Color(r, g, b, a);
								}


								//UI - TEXT ...sadly I know this is repetitive :(
								else if(loggedProperty == "TEXT"){
									Text text = objInScene.GetComponent<Text>();
									text.text = "";
									for(int j = i+2; j < splitLine.Length; j++){ //the text may have been split unnecessarily if there is a splitCharacter in the text
										text.text += splitLine[j]; //add each piece of the split text
										if(j+1 < splitLine.Length){
											text.text += Logger_Threading.LogTextSeparator; //add back the split characters into the text!
										}
									}

									text.text = text.text.Replace("_NEWLINE_", "\n");
								}
								else if(loggedProperty == "TEXT_COLOR"){
									Text text = objInScene.GetComponent<Text>();
									float r = float.Parse(splitLine[i+2]);
									float g = float.Parse(splitLine[i+3]);
									float b = float.Parse(splitLine[i+4]);
									float a = float.Parse(splitLine[i+5]);
									text.color = new Color(r, g, b, a);
								}

								//UI - PANEL
								else if (loggedProperty == "PANEL"){
									Image image = objInScene.GetComponent<Image>();
									float r = float.Parse(splitLine[i+2]);
									float g = float.Parse(splitLine[i+3]);
									float b = float.Parse(splitLine[i+4]);
									float a = float.Parse(splitLine[i+5]);
									image.color = new Color(r, g, b, a);
								}

								//PARTICLE SYSTEMS
								else if (loggedProperty == "PARTICLE_SYSTEM_PLAYING"){
									string particleSystemName = splitLine [i+2];
									ParticleSystem particles = objInScene.GetComponent<ParticleSystem>();
									if(particles == null){
										particles = objInScene.transform.FindChild( particleSystemName ).GetComponent<ParticleSystem>();
									}

									particles.Play();

								}
								else if (loggedProperty == "PARTICLE_SYSTEM_STOPPED"){
									string particleSystemName = splitLine [i+2];
									ParticleSystem particles = objInScene.GetComponent<ParticleSystem>();
									if(particles == null){
										particles = objInScene.transform.FindChild( particleSystemName ).GetComponent<ParticleSystem>();
									}
									
									particles.Stop();
									
								}

								//PARTICLE EMITTERS
								else if (loggedProperty == "PARTICLE_EMITTER_PLAYING"){
									string particleSystemName = splitLine [i+2];
									ParticleEmitter particles = objInScene.GetComponent<ParticleEmitter>();
									if(particles == null){
										particles = objInScene.transform.FindChild( particleSystemName ).GetComponent<ParticleEmitter>();
									}


									particles.emit = true;
									
								}
								else if (loggedProperty == "PARTICLE_EMITTER_STOPPED"){
									string particleSystemName = splitLine [i+2];
									ParticleEmitter particles = objInScene.GetComponent<ParticleEmitter>();
									if(particles == null){
										particles = objInScene.transform.FindChild( particleSystemName ).GetComponent<ParticleEmitter>();
									}
									
									particles.emit = false;
									
								}

								//AUDIO
								else if (loggedProperty == "AUDIO_PLAYING"){
									string audioSourceName = splitLine [i+2];
									AudioSource audio = objInScene.GetComponent<AudioSource>();
									if(audio == null){
										audio = objInScene.transform.FindChild( audioSourceName ).GetComponent<AudioSource>();
									}
									
									
									audio.Play();
									
								}
								else if (loggedProperty == "AUDIO_STOPPED"){
									string audioSourceName = splitLine [i+2];
									AudioSource audio = objInScene.GetComponent<AudioSource>();
									if(audio == null){
										audio = objInScene.transform.FindChild( audioSourceName ).GetComponent<AudioSource>();
									}
									
									audio.Stop();
									
								}

								//LINE RENDERERS
								else if (loggedProperty == "LINE_RENDERER_POSITION"){
									int positionIndex = int.Parse(splitLine[i+2]);
									float posX = float.Parse(splitLine[i+3]);
									float posY = float.Parse(splitLine[i+4]);
									float posZ = float.Parse(splitLine[i+5]);

									objInScene.GetComponent<LineRenderer>().SetPosition(positionIndex, new Vector3(posX, posY, posZ));
								}

								else if (loggedProperty == "LINE_RENDERER_COLOR"){

									float startR = float.Parse(splitLine[i+2]);
									float startG = float.Parse(splitLine[i+3]);
									float startB = float.Parse(splitLine[i+4]);
									float startA = float.Parse(splitLine[i+5]);
									Color startColor = new Color(startR, startG, startB, startA);

									//string positionIndex = int.Parse(splitLine[i+7]);
									float endR = float.Parse(splitLine[i+6]);
									float endG = float.Parse(splitLine[i+7]);
									float endB = float.Parse(splitLine[i+8]);
									float endA = float.Parse(splitLine[i+9]);
									Color endColor = new Color(endR, endG, endB, endA);

									
									objInScene.GetComponent<LineRenderer>().SetColors(startColor, endColor);
								}

							}
							else{
								Debug.Log("REPLAY: No obj in scene named " + objName);
							}
							
						}
					}

				}


			}

			//read the next line at the end of the while loop
			currentLogFileLine = fileReader.ReadLine ();

			if(hasFinishedSettingFrame){ //
				yield return 0; //WHILE LOGGED ON FIXED UPDATE, REPLAY ON UPDATE TO GET A CONSTANT #RENDERED FRAMES

				hasFinishedSettingFrame = false;

			}
		}

		//take the last screenshot
		if (isRecording) {
			RecordScreenShot ();
		}
		yield return 0;
		Application.LoadLevel(0); //return to main menu
	}

}
