using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (AudioSource))]
public class Store : MonoBehaviour {

	Experiment exp { get { return Experiment.Instance; } }

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
		string folder = "StoreAudio/" + gameObject.name;
		AudioClip[] storeAudioClips = Resources.LoadAll<AudioClip>(folder);
		for(int i = 0; i < storeAudioClips.Length; i++){
			audioLeftToUse.Add(storeAudioClips[i]);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public IEnumerator PlayDeliveryAudio(int deliverySerialPosition){
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

			exp.eventLogger.LogItemDelivery(myAudioPlayer.clip.name, this, deliverySerialPosition, true, true); //TODO: move into trial controller if possible.

			while(myAudioPlayer.isPlaying){
				yield return 0;
			}

			exp.eventLogger.LogItemDelivery(myAudioPlayer.clip.name, this, deliverySerialPosition, true, false); //TODO: move into trial controller if possible.
		}
		else{
			Debug.Log("No audio for this store!" + gameObject.name);
			yield return 0;
		}
	}
	
	public void ResetStore(){
		transform.position = origPosition;
		transform.rotation = origRotation;
	}
}

