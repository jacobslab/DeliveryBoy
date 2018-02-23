using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using MonoLibUsb;

public class SyncBox : MonoBehaviour
{

    //Function from Corey's Syncbox plugin (called "ASimplePlugin")
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

    private const float PULSE_START_DELAY = 1f;
    private const float TIME_BETWEEN_PULSES_MIN = 0.8f;
    private const float TIME_BETWEEN_PULSES_MAX = 1.2f;
    private const int SECONDS_TO_MILLISECONDS = 1000;

    private const short FREIBURG_SYNCBOX_VENDOR_ID  = 0x0403;
    private const short FREIBURG_SYNCBOX_PRODUCT_ID = 0x6001;
    private const int FREIBURG_SYNCBOX_TIMEOUT_MS = 500;
    private const int FREIBURG_SYNCBOX_PIN_COUNT = 8;


    private Thread syncpulseThread;

    public ScriptedEventReporter scriptedEventReporter;

    // Use this for initialization
    void Start()
    {
        FreiburgPulse();

        //open usb, log the result string returned
        Debug.Log(Marshal.PtrToStringAuto(OpenUSB()));

        //start a thread which will send the pulses
        syncpulseThread = new Thread(DoPulses);
        syncpulseThread.Start();
    }

    private void FreiburgPulse()
    {
        MonoLibUsb.MonoUsbSessionHandle sessionHandle = new MonoUsbSessionHandle();
        MonoLibUsb.Profile.MonoUsbProfileList profileList = null;

        if (sessionHandle.IsInvalid)
            throw new ExternalException("Failed to initialize context.");

        MonoUsbApi.SetDebug(sessionHandle, 0);

        profileList = new MonoLibUsb.Profile.MonoUsbProfileList();

        // The list is initially empty.
        // Each time refresh is called the list contents are updated. 
        int profileListRefreshResult;
        profileListRefreshResult = profileList.Refresh(sessionHandle);
        if (profileListRefreshResult < 0) throw new ExternalException("Failed to retrieve device list.");
        Debug.Log(profileListRefreshResult.ToString() + " device(s) found.");

        // Iterate through the profile list.
        // If we find the device, write 00000000 to its endpoint 2.
        foreach (MonoLibUsb.Profile.MonoUsbProfile profile in profileList)
        {
            if (profile.DeviceDescriptor.ProductID == FREIBURG_SYNCBOX_PRODUCT_ID && profile.DeviceDescriptor.VendorID == FREIBURG_SYNCBOX_VENDOR_ID)
            {
                int actual_length;
                MonoLibUsb.MonoUsbDeviceHandle deviceHandle = null;
                deviceHandle = profile.OpenDeviceHandle();
                if (deviceHandle == null)
                    throw new ExternalException("The ftd USB device was found but couldn't be opened");
                MonoUsbApi.BulkTransfer(deviceHandle, 2, byte.MinValue, FREIBURG_SYNCBOX_PIN_COUNT, out actual_length, FREIBURG_SYNCBOX_TIMEOUT_MS);
                Debug.Log(actual_length.ToString() + " bits written.");
            }
        }
        
        profileList.Close();
        sessionHandle.Close();
    }

    private void PennPulse()
    {
        SyncPulse();
    }

    private void DoPulses ()
    {
        System.Random random = new System.Random();

        //delay before starting pulses
        Thread.Sleep((int)(PULSE_START_DELAY*SECONDS_TO_MILLISECONDS));

		while (true)
        {
            //pulse
            PennPulse();
            //FreiburgPulse();
            //log the pulse
            LogPulse();

            //wait a random time between min and max
            float timeBetweenPulses = (float)(TIME_BETWEEN_PULSES_MIN + (random.NextDouble() * (TIME_BETWEEN_PULSES_MAX - TIME_BETWEEN_PULSES_MIN)));
            Thread.Sleep((int)(timeBetweenPulses * SECONDS_TO_MILLISECONDS));
		}
	}

    private void LogPulse()
    {
        scriptedEventReporter.ReportScriptedEvent("Sync pulse begin", new System.Collections.Generic.Dictionary<string, object>());
    }

    private void OnApplicationQuit()
    {
        //close usb, log the result string returned
		Debug.Log(Marshal.PtrToStringAuto (CloseUSB()));
        //stop the pulsing thread
        syncpulseThread.Abort();
	}
}