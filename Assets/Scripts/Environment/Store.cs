using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (AudioSource))]
public class Store : MonoBehaviour {

	Experiment exp { get { return Experiment.Instance; } }

	public bool hasRotationVisuals = false;
	public Vector3 presentationRotation;
	public EnableChildrenLogTrack rotationVisualsParent;
	public EnableChildrenLogTrack regularVisualsParent;

	public TextMesh myLabel;
	Vector3 origPosition;
	Quaternion origRotation;
	
	public List<AudioClip> audioLeftToUse;
	AudioSource myAudioPlayer;
	
	// Use this for initialization
	void Awake () {
		if (myLabel) {
			myLabel.text = gameObject.name;
		}
		origPosition = transform.position;
		origRotation = transform.rotation;

		myAudioPlayer = GetComponent<AudioSource> ();
		InitAudio();
	}
	
	void InitAudio(){
		if (Config.isStoreCorrelatedDelivery) {
		
			audioLeftToUse = new List<AudioClip> ();
			string folder = "StoreAudio/" + GetDisplayName(); //just happens to be organized with the display name...
			AudioClip[] storeAudioClips = Resources.LoadAll<AudioClip> (folder);
			for (int i = 0; i < storeAudioClips.Length; i++) {
				audioLeftToUse.Add (storeAudioClips [i]);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public bool GetIsAudioPlaying(){
		if (myAudioPlayer.isPlaying) {
			return true;
		} else {
			return false;
		}
	}

	public void PlayCurrentAudio(){
		if (myAudioPlayer.clip != null) {
			myAudioPlayer.Play();
		}
	}
	
	public IEnumerator PlayDeliveryAudio(int deliverySerialPosition){
		Debug.Log ("Should play delivery audio! " + gameObject.name);

		AudioClip audioDeliveryClip;
		if (!Config.isStoreCorrelatedDelivery) {
			audioDeliveryClip = ChooseDeliveryAudio (exp.storeController.allStoreAudioLeftToUse);
		} else {
			audioDeliveryClip = ChooseDeliveryAudio (audioLeftToUse);
		}

		if(audioDeliveryClip != null){
			myAudioPlayer.clip = audioDeliveryClip;
			myAudioPlayer.Play ();

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

	AudioClip ChooseDeliveryAudio(List<AudioClip> audioList){

		if (audioList.Count == 0) {
			if(audioList == audioLeftToUse){
				InitAudio ();
			}
			else if (audioList == exp.storeController.allStoreAudioLeftToUse){
				exp.storeController.InitAudio();
			}
		}

		if (audioList.Count != 0) {
			int randomAudioIndex = Random.Range (0, audioList.Count);
			AudioClip audio = audioList [ randomAudioIndex ];
			audioList.RemoveAt(randomAudioIndex);
			return audio;
		}

		return null;
	}

	public void SetVisualsForPresentation(){
		transform.rotation = Quaternion.Euler(presentationRotation);

		if (hasRotationVisuals) {
			UsefulFunctions.EnableChildren(regularVisualsParent.transform, false);
			regularVisualsParent.LogChildrenEnabled(false);

			UsefulFunctions.EnableChildren(rotationVisualsParent.transform, true);
			rotationVisualsParent.LogChildrenEnabled(true);
		}
	}
	
	public void ResetStore(){
		transform.position = origPosition;
		transform.rotation = origRotation;

		if (hasRotationVisuals) {
			UsefulFunctions.EnableChildren (regularVisualsParent.transform, true);
			regularVisualsParent.LogChildrenEnabled (true);
		
			UsefulFunctions.EnableChildren (rotationVisualsParent.transform, false);
			rotationVisualsParent.LogChildrenEnabled (false);
		}
	}

	public string GetDisplayName(){
		string displayName = gameObject.name;
		displayName = displayName.Replace ("_", " ");

		return displayName;
	}

}

