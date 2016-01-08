/*using UnityEngine;
using System.Collections;

public class AudioRecorder : MonoBehaviour {

	void Start() {

		foreach (string device in Microphone.devices) {
			Debug.Log("Name: " + device);
		}

	}

	public void Record(string filePath, string fileName, float duration){
		if (CheckForRecordingDevice ()) {
			Debug.Log("There is a recording device!");
			AudioSource aud = GetComponent<AudioSource> ();
			aud.clip = Microphone.Start ("Built-in Microphone", true, duration, 44100);
			aud.Play ();

			SavWav.Save (filePath, fileName, aud.clip);
		} 
		else {
			Debug.Log ("No recording device.");
		}
	}

	bool CheckForRecordingDevice(){
		if (Microphone.devices.Length > 0) {
			return true;
		}
		return false;
	}
}*/

//SCRIPT MODIFIED FROM: http://wiki.unity3d.com/index.php/Mic_Input

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class AudioRecorder : MonoBehaviour {
	
	public enum micActivation {
		HoldToSpeak,
		PushToSpeak,
		ConstantSpeak
	}
	
	public float sensitivity = 100;
	public float ramFlushSpeed = 5;//The smaller the number the faster it flush's the ram, but there might be performance issues...
	[Range(0,100)]
	public float sourceVolume = 100;//Between 0 and 100
	public bool GuiSelectDevice = true;
	public micActivation micControl;
	//
	public string selectedDevice { get; private set; }	
	public float loudness { get; private set; } //dont touch
	//
	private bool micSelected = false;
	private float ramFlushTimer;
	private int amountSamples = 256; //increase to get better average, but will decrease performance. Best to leave it
	private int minFreq, maxFreq; 

	AudioSource audio;

	void Start() {
		audio = GetComponent<AudioSource> ();

		if (CheckForRecordingDevice ()) {
			Debug.Log(Microphone.devices.Length);
			audio.loop = true; // Set the AudioClip to loop
			audio.mute = false; // Mute the sound, we don't want the player to hear it
			selectedDevice = Microphone.devices [0].ToString ();
			micSelected = true;
			GetMicCaps ();

		}
	}

	bool CheckForRecordingDevice(){
		if (Microphone.devices.Length > 0) {
			return true;
		}
		return false;
	}
	
	public void GetMicCaps () {
		Microphone.GetDeviceCaps(selectedDevice, out minFreq, out maxFreq);//Gets the frequency of the device
		if ((minFreq + maxFreq) == 0)//These 2 lines of code are mainly for windows computers
			maxFreq = 44100;
	}

	/* //FOR DEBUGGING / TESTING
	int numRecordings = 0;
	void GetInput(){
		if (Input.GetKeyDown (KeyCode.A)) {
			StartCoroutine(Record("/Users/coreynovich/Desktop/Unity/DeliveryBoy/TextFiles", "testRecord" + numRecordings, 4));
			numRecordings++;
		}
	}

	void Update() {
		GetInput ();
	}*/

	public Text recordText;
	public IEnumerator Record(string filePath, string fileName, int duration){
		Color origTextColor = recordText.color;
		recordText.color = Color.red;

		StartMicrophone (duration);
		yield return new WaitForSeconds (duration);

		StopMicrophone ();
		recordText.color = Color.white;

		SavWav.Save (filePath, fileName, audio.clip);
	}

	public void StartMicrophone (int duration) {
		audio.clip = Microphone.Start(selectedDevice, true, duration, maxFreq);//Starts recording
		while (!(Microphone.GetPosition(selectedDevice) > 0)){} // Wait until the recording has started
		//audio.Play(); // Play the audio source!
	}
	
	public void StopMicrophone () {
		audio.Stop();//Stops the audio
		Microphone.End(selectedDevice);//Stops the recording of the device	
	}		
	
	private void RamFlush () {
		if (ramFlushTimer >= ramFlushSpeed && Microphone.IsRecording(selectedDevice)) {
			StopMicrophone();
			StartMicrophone(10);
			ramFlushTimer = 0;
		}
	}
	
	float GetAveragedVolume() {
		float[] data = new float[amountSamples];
		float a = 0;
		audio.GetOutputData(data,0);
		foreach(float s in data) {
			a += Mathf.Abs(s);
		}
		return a/amountSamples;
	}
}