using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

public class SyncboxControl : MonoBehaviour {
	Experiment exp { get { return Experiment.Instance; } }

	//DYNLIB FUNCTIONS
	[DllImport ("liblabjackusb")]
	private static extern float LJUSB_GetLibraryVersion( );


	[DllImport ("ASimplePlugin")]
	private static extern int PrintANumber();
	[DllImport ("ASimplePlugin")]
	private static extern float AddTwoFloats(float f1,float f2);
	[DllImport ("ASimplePlugin")]
	private static extern IntPtr OpenUSB();
	[DllImport ("ASimplePlugin")]
	private static extern IntPtr CloseUSB();
	[DllImport ("ASimplePlugin")]
	private static extern IntPtr TurnLEDOn();
	[DllImport ("ASimplePlugin")]
	private static extern IntPtr TurnLEDOff();
	[DllImport ("ASimplePlugin")]
	private static extern float SyncPulse();
	[DllImport ("ASimplePlugin")]
	private static extern IntPtr StimPulse(float durationSeconds, float freqHz, bool doRelay);
	
	public bool ShouldPulse = false;
	public float PulseOnSeconds;
	public float PulseOffSeconds;
	public TextMesh DownCircle;
	public Color DownColor;
	public Color UpColor;

	public bool isUSBOpen = false; //TODO: set to true.

	bool isToggledOn = false;


	//SINGLETON
	private static SyncboxControl _instance;
	
	public static SyncboxControl Instance{
		get{
			return _instance;
		}
	}
	
	void Awake(){
		
		if (_instance != null) {
			UnityEngine.Debug.Log("Instance already exists!");
			Destroy(transform.gameObject);
			return;
		}
		_instance = this;
		
	}

	// Use this for initialization
	void Start () {
		if(ExperimentSettings.isSyncbox){
			//Debug.Log(AddTwoFloats(2.5F,4F));
			//Debug.Log ("OH HAYYYY");
			//Debug.Log(PrintANumber());
			//Debug.Log (LJUSB_GetLibraryVersion ());
			StartCoroutine(ConnectSyncbox());
		}
	}

	IEnumerator ConnectSyncbox(){
		while(!isUSBOpen){
			string usbOpenFeedback = Marshal.PtrToStringAuto (OpenUSB());
			Debug.Log(usbOpenFeedback);
			if(usbOpenFeedback != "didn't open USB..."){
				isUSBOpen = true;
			}

			yield return 0;
		}

		//Debug.Log(Marshal.PtrToStringAuto (CloseUSB()));
		StartCoroutine (TestPulse ());
	}
	
	// Update is called once per frame
	void Update () {
		if(ExperimentSettings.isSyncbox){
			if (!ShouldPulse) {
				GetInput ();
			}
		}
	}

	void GetInput(){
		if (Input.GetKey (KeyCode.DownArrow)) {
			ToggleOn();
		}
		else{
			ToggleOff ();
		}
		if(Input.GetKeyDown(KeyCode.S)){
			//DoSyncPulse();
			DoStimPulse();
		}
	}


	//LOGGING
	void LogTEST(long time, bool isOn){
		if(ExperimentSettings.isLogging){
			if(isOn){
				exp.eegLog.Log(time, exp.eegLog.GetFrameCount(), "LED ON");
			}
			else{
				exp.eegLog.Log(time, exp.eegLog.GetFrameCount(), "LED OFF");
			}
		}
	}

	void LogSYNCStarted(long time, float duration){
		if (ExperimentSettings.isLogging) {
			exp.eegLog.Log (time, exp.eegLog.GetFrameCount (), "SYNC PULSE STARTED" + Logger_Threading.LogTextSeparator + duration);
		}
	}

	void LogSYNCPulseInfo(long time, float timeBeforePulseSeconds){
		if (ExperimentSettings.isLogging) {
			exp.eegLog.Log (time, exp.eegLog.GetFrameCount (), "SYNC PULSE INFO" + Logger_Threading.LogTextSeparator + timeBeforePulseSeconds*1000); //log milliseconds
		}
	}

	void LogSTIM(long time, float duration){
		if (ExperimentSettings.isLogging) {
			exp.eegLog.Log (time, exp.eegLog.GetFrameCount (), "STIM PULSE" + Logger_Threading.LogTextSeparator + duration);
		}
	}


	//TOGGLING

	void ToggleOn(){
		if (!isToggledOn) {
			DownCircle.color = DownColor;
			Debug.Log(Marshal.PtrToStringAuto (TurnLEDOn()));
		}
		isToggledOn = true;
	}

	void ToggleOff(){
		if (isToggledOn) {
			DownCircle.color = UpColor;
			Debug.Log(Marshal.PtrToStringAuto (TurnLEDOff()));
		}
		isToggledOn = false;
	}


	//ex: a 10 ms pulse every second — until the duration is over...
	void DoSyncPulse(){
		float duration = 1.0f; //TODO: implement a duration or something for a series of sync pulses
		LogSYNCStarted (GameClock.SystemTime_Milliseconds, duration);
		float timeBeforePulse = SyncPulse ();
		LogSYNCPulseInfo (GameClock.SystemTime_Milliseconds, timeBeforePulse);
		Debug.Log (timeBeforePulse);
	}

	void DoStimPulse(){
		//TODO: move these to a config file or something.
		float durationSeconds = 1.0f;
		float freqHz = 10;
		LogSTIM (GameClock.SystemTime_Milliseconds, durationSeconds);
		Debug.Log(Marshal.PtrToStringAuto (StimPulse (durationSeconds, freqHz, false)));
	}

	IEnumerator TestPulse (){
		yield return new WaitForSeconds(TCP_Config.numSecondsBeforeAlignment);
		while (true) {
			if(ShouldPulse){
				ToggleOn();
				LogTEST(GameClock.SystemTime_Milliseconds, true);
				yield return new WaitForSeconds(PulseOnSeconds);
				ToggleOff();
				LogTEST(GameClock.SystemTime_Milliseconds, false);
				yield return new WaitForSeconds(PulseOffSeconds);
			}
			else{
				yield return 0;
			}
		}
	}

	void OnApplicationQuit(){
		if(isUSBOpen){
			Debug.Log(Marshal.PtrToStringAuto (CloseUSB()));
		}
	}

}
