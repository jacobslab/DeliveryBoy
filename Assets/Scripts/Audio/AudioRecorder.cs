using UnityEngine;
using System.Collections;

public class AudioRecorder : MonoBehaviour {

	void Start() {

		foreach (string device in Microphone.devices) {
			Debug.Log("Name: " + device);
		}

	}

	public void Record(string filePath, string fileName){
		if (CheckForRecordingDevice ()) {
			Debug.Log("There is a recording device!");
			AudioSource aud = GetComponent<AudioSource> ();
			aud.clip = Microphone.Start ("Built-in Microphone", true, 10, 44100);
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
}