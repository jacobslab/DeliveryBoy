using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (AudioSource))]
public class Building : MonoBehaviour {
	
	public TextMesh myLabel;
	Vector3 origPosition;
	Quaternion origRotation;
	
	List<AudioClip> audioLeftToUse;
	AudioSource myAudioPlayer;
	
	// Use this for initialization
	void Awake () {
		if (myLabel) {
			myLabel.text = gameObject.name;
		}
		origPosition = transform.position;
		origRotation = transform.rotation;
		
		InitAudio();
	}
	
	void InitAudio(){
		myAudioPlayer = GetComponent<AudioSource>();
		
		audioLeftToUse = new List<AudioClip>();
		string folder = "BuildingAudio/" + gameObject.name;
		AudioClip[] buildingAudioClips = Resources.LoadAll<AudioClip>(folder);
		for(int i = 0; i < buildingAudioClips.Length; i++){
			audioLeftToUse.Add(buildingAudioClips[i]);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public IEnumerator PlayDeliveryAudio(){
		Debug.Log ("Should play delivery audio! " + gameObject.name);
		if(audioLeftToUse.Count == 0){
			InitAudio();
		}
		if(audioLeftToUse.Count != 0){
			int randomAudioIndex = Random.Range(0, audioLeftToUse.Count);
			AudioClip audioToPlay = audioLeftToUse[randomAudioIndex];
			myAudioPlayer.clip = audioToPlay;
			myAudioPlayer.Play ();
			audioLeftToUse.RemoveAt(randomAudioIndex);
			while(myAudioPlayer.isPlaying){
				yield return 0;
			}
		}
		else{
			Debug.Log("No audio for this building!" + gameObject.name);
			yield return 0;
		}
	}
	
	public void ResetBuilding(){
		transform.position = origPosition;
		transform.rotation = origRotation;
	}
}

