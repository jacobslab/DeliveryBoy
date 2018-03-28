using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundRecorder : MonoBehaviour
{
    public GameObject pleaseSpeakNow;

    private AudioClip recording;
    private int startSample;
    private float startTime;
    private bool isRecording = false;
    private string nextOutputPath;

    private const int SECONDS_IN_MEMORY = 600;

    void OnEnable()
    {
        recording = Microphone.Start("", true, SECONDS_IN_MEMORY, 44100);
    }

    void OnDisable()
    {
        Microphone.End("");
    }

    //using the system's default device
    public void StartRecording(string outputFilePath)
    {
        if (isRecording)
        {
            throw new UnityException("Already recording.  Please StopRecording first.");
        }

        nextOutputPath = outputFilePath;
        pleaseSpeakNow.SetActive(true);
        startSample = Microphone.GetPosition("");
        startTime = Time.unscaledTime;
        isRecording = true;
    }

    public void StopRecording()
    {
        if (!isRecording)
        {
            throw new UnityException("Not recording.  Please StartRecording first.");
        }

        isRecording = false;
        pleaseSpeakNow.SetActive(false);

        int recordingLength = Mathf.CeilToInt(Time.unscaledTime - startTime);

        int outputLength = 44100 * recordingLength;
        AudioClip croppedClip = AudioClip.Create("cropped recording", outputLength, 1, 44100, false);

        float[] saveData = new float[outputLength];
        if (startSample < recording.samples - outputLength)
        {
            Debug.Log("No wraparound");
            recording.GetData(saveData, startSample);
        }
        else
        {
            Debug.Log("Yes wraparound");
            float[] tailData = new float[recording.samples - startSample];
            recording.GetData(tailData, startSample);
            float[] headData = new float[outputLength - tailData.Length];
            recording.GetData(headData, 0);
            for (int i = 0; i < tailData.Length; i++)
                saveData[i] = tailData[i];
            for (int i = 0; i < headData.Length; i++)
                saveData[tailData.Length + i] = headData[i];
        }

        croppedClip.SetData(saveData, 0);
        SavWav.Save(nextOutputPath, croppedClip);
    }

    public AudioClip AudioClipFromDatapath(string datapath)
    {
        string url = "file:///" + datapath;
        WWW audioFile = new WWW(url);
        while(!audioFile.isDone)
        {
            
        }
        return audioFile.GetAudioClip();
    }

	void OnApplicationQuit()
	{
        if (isRecording)
            StopRecording();
	}
}