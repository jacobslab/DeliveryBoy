using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

public class SyncboxControl : MonoBehaviour {
	Experiment exp { get { return Experiment.Instance; } }

	[DllImport ("ASimplePlugin")]
	private static extern IntPtr OpenUSB();
	[DllImport ("ASimplePlugin")]
	private static extern IntPtr CloseUSB();
	[DllImport ("ASimplePlugin")]
	private static extern IntPtr TurnLEDOn();
	[DllImport ("ASimplePlugin")]
	private static extern IntPtr TurnLEDOff();
	[DllImport ("ASimplePlugin")]
	private static extern long SyncPulse();
	[DllImport ("ASimplePlugin")]
	private static extern IntPtr StimPulse(float durationSeconds, float freqHz, bool doRelay);
	
	public bool ShouldSyncPulse = true;
	public float PulseOnSeconds;
	public float PulseOffSeconds;

	public bool isUSBOpen = false;


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
		if(Config.isSyncbox){
			StartCoroutine(ConnectSyncbox());
		}
	}

	IEnumerator ConnectSyncbox(){
		while(!isUSBOpen){
			string usbOpenFeedback = Marshal.PtrToStringAuto (OpenUSB());
			UnityEngine.Debug.Log(usbOpenFeedback);
			if(usbOpenFeedback != "didn't open USB..."){
				isUSBOpen = true;
			}

			yield return 0;
		}

		StartCoroutine (RunSyncPulseManual ());
	}
	
	// Update is called once per frame
	void Update () {
		GetInput ();
	}

	void GetInput(){
		//use this for debugging if you'd like
	}

	float syncPulseDuration = 0.05f;
	float syncPulseInterval = 1.0f;
	IEnumerator RunSyncPulse(){
		Stopwatch executionStopwatch = new Stopwatch ();

		while (ShouldSyncPulse) {
			executionStopwatch.Reset();

			SyncPulse(); //executes pulse, then waits for the rest of the 1 second interval

			executionStopwatch.Start();
			long syncPulseOnTime = SyncPulse();
			LogSYNCOn(syncPulseOnTime);
			while(executionStopwatch.ElapsedMilliseconds < 1500){
				yield return 0;
			}

			executionStopwatch.Stop();

		}
	}

	IEnumerator RunSyncPulseManual(){
		float jitterMin = 0.1f;
		float jitterMax = syncPulseInterval - syncPulseDuration;

		Stopwatch executionStopwatch = new Stopwatch ();
		
		while (ShouldSyncPulse) {
			executionStopwatch.Reset();


			float jitter = UnityEngine.Random.Range(jitterMin, jitterMax);//syncPulseInterval - syncPulseDuration);
			yield return StartCoroutine(WaitForShortTime(jitter));

			ToggleLEDOn();
			yield return StartCoroutine(WaitForShortTime(syncPulseDuration));
			ToggleLEDOff();

			float timeToWait = (syncPulseInterval - syncPulseDuration) - jitter;
			if(timeToWait < 0){
				timeToWait = 0;
			}

			yield return StartCoroutine(WaitForShortTime(timeToWait));
			
			executionStopwatch.Stop();
		}
	}

	//return microseconds it took to turn on LED
	void ToggleLEDOn(){

		TurnLEDOn ();
		LogSYNCOn (GameClock.SystemTime_Milliseconds);
	}

	void ToggleLEDOff(){

		TurnLEDOff();
		LogSYNCOff (GameClock.SystemTime_Milliseconds);

	}

	long GetMicroseconds(long ticks){
		long microseconds = ticks / (TimeSpan.TicksPerMillisecond / 1000);
		return microseconds;
	}
	
	IEnumerator WaitForShortTime(float jitter){
		float currentTime = 0.0f;
		while (currentTime < jitter) {
			currentTime += Time.deltaTime;
			yield return 0;
		}
		
	}

	void LogSYNCOn(long time){
		if (ExperimentSettings.isLogging) {
			exp.eegLog.Log (time, exp.eegLog.GetFrameCount(), "ON"); //NOTE: NOT USING FRAME IN THE FRAME SLOT
		}
	}

	void LogSYNCOff(long time){
		if (ExperimentSettings.isLogging) {
			exp.eegLog.Log (time, exp.eegLog.GetFrameCount(), "OFF"); //NOTE: NOT USING FRAME IN THE FRAME SLOT
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

	void OnDestroy(){
		UnityEngine.Debug.Log(Marshal.PtrToStringAuto (CloseUSB()));
	}

	void OnApplicationQuit(){
		UnityEngine.Debug.Log(Marshal.PtrToStringAuto (CloseUSB()));
	}

}
