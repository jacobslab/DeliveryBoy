using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Threading;

public class SyncboxInput : MonoBehaviour
{

	//DYNLIB FUNCTIONS
	//[DllImport ("liblabjackusb")]
	//private static extern float LJUSB_GetLibraryVersion( );


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

    private const float PULSE_START_DELAY = 1f;
    private const float TIME_BETWEEN_PULSES_MIN = 0.8f;
    private const float TIME_BETWEEN_PULSES_MAX = 1.2f;
    private const int SECONDS_TO_MILLISECONDS = 1000;

    private Thread syncpulseThread;

    public ScriptedEventReporter scriptedEventReporter;

	// Use this for initialization
	void Start ()
    {
		Debug.Log(Marshal.PtrToStringAuto (OpenUSB()));

        syncpulseThread = new Thread(Pulse);
        syncpulseThread.Start();
	}

	void SetSyncPulse()
    {
		Debug.Log (SyncPulse ());
	}

	void Pulse ()
    {
        System.Random random = new System.Random();

        Thread.Sleep((int)(PULSE_START_DELAY*SECONDS_TO_MILLISECONDS));
		while (true)
        {
            SyncPulse();
            scriptedEventReporter.ReportScriptedEvent("Sync pulse begin", new System.Collections.Generic.Dictionary<string, object>());
            float timeBetweenPulses = (float)(TIME_BETWEEN_PULSES_MIN + (random.NextDouble() * (TIME_BETWEEN_PULSES_MAX - TIME_BETWEEN_PULSES_MIN)));
            Thread.Sleep((int)(timeBetweenPulses * SECONDS_TO_MILLISECONDS));
		}
	}

	void OnApplicationQuit()
    {
		Debug.Log(Marshal.PtrToStringAuto (CloseUSB()));
        syncpulseThread.Abort();
	}

}
