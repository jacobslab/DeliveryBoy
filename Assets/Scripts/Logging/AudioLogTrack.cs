using UnityEngine;
using System.Collections;

public class AudioLogTrack : LogTrack {
	//public bool useParentsName = false;

	public AudioSource audioSource;
	SpawnableObject spawnableObject;

	bool isAudioPlaying = false;
	// Use this for initialization
	void Start () {
		spawnableObject = GetComponent<SpawnableObject> ();
	}
	
	//log on late update so that everything for that frame gets set first
	void LateUpdate () {
		if (ExperimentSettings.isLogging) {
			LogAudio ();
		}
	}

	void LogAudio(){
		//will only log when it changes from playing to not playing, or vice versa

		if (!isAudioPlaying && audioSource.isPlaying) {
			isAudioPlaying = true;
			LogAudioPlaying (audioSource.clip, audioSource.transform.position);
		} 
		else if (isAudioPlaying && !audioSource.isPlaying) {
			isAudioPlaying = false;
			LogAudioOver (audioSource.clip, audioSource.transform.position);
		}
	}

	void LogAudioPlaying(AudioClip audioClip, Vector3 audioLocation){
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), GetNameToLog() + separator + "AUDIO_PLAYING" + separator + audioSource.name + separator + audioClip.name + separator + "IS_LOOPING" + separator + audioSource.loop + separator + audioLocation.x + separator + audioLocation.y + separator + audioLocation.z);
	}

	void LogAudioOver(AudioClip audioClip, Vector3 audioLocation){
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), GetNameToLog() + separator + "AUDIO_STOPPED" + separator + audioSource.name + separator + audioClip.name + separator + "IS_LOOPING" + separator + audioSource.loop + separator + audioLocation.x + separator + audioLocation.y + separator + audioLocation.z);
	}

	string GetNameToLog(){
		string name = gameObject.name;
		/*if (useParentsName) {
			SpawnableObject parentSpawnable = transform.parent.GetComponent<SpawnableObject>();
			if( parentSpawnable != null ){
				name = parentSpawnable.GetName();
			}
			else{
				name = transform.parent.name;
			}
		}
		else */if (spawnableObject) {
			name = spawnableObject.GetName();
		}
		return name;
	}

	void OnDestroy(){
		if( isAudioPlaying ) {
			isAudioPlaying = false;
			LogAudioOver (audioSource.clip, audioSource.transform.position);
		}
	}

	void OnDisable(){
		if( isAudioPlaying ) {
			isAudioPlaying = false;
			audioSource.Stop();
			LogAudioOver (audioSource.clip, audioSource.transform.position);
		}
	}
}
