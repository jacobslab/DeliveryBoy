using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

public class Syncbox : MonoBehaviour
{
    public enum SynboxType { FREIBURG, JEFF };
    public static SynboxType currentSyncboxType;


    IEnumerator RunSyncPulseManual()
    {
        float syncPulseDuration = 0.05f;
        float syncPulseInterval = 1.0f;

        float jitterMin = 0.1f;
        float jitterMax = syncPulseInterval - syncPulseDuration;

        while (true)
        {
            UnityEngine.Debug.Log("pulse running");

            float jitter = UnityEngine.Random.Range(jitterMin, jitterMax);//syncPulseInterval - syncPulseDuration);
            yield return StartCoroutine(WaitForShortTime(jitter));

            //ToggleLEDOn();
            yield return StartCoroutine(WaitForShortTime(syncPulseDuration));
            //ToggleLEDOff();

            float timeToWait = (syncPulseInterval - syncPulseDuration) - jitter;
            if (timeToWait < 0)
            {
                timeToWait = 0;
            }

            yield return StartCoroutine(WaitForShortTime(timeToWait));
        }
    }


    long GetMicroseconds(long ticks)
    {
        long microseconds = ticks / (TimeSpan.TicksPerMillisecond / 1000);
        return microseconds;
    }

    IEnumerator WaitForShortTime(float jitter)
    {
        float currentTime = 0.0f;
        while (currentTime < jitter)
        {
            currentTime += Time.deltaTime;
            yield return 0;
        }

    }


    void OnApplicationQuit()
    {
        //UnityEngine.Debug.Log(Marshal.PtrToStringAuto(CloseUSB()));
    }

}