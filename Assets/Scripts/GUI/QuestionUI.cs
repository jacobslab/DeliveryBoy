using UnityEngine;
using System.Collections;

public class QuestionUI : MonoBehaviour {

	Experiment exp { get { return Experiment.Instance; } } 

	public TextMesh ObjectNameTextMesh;
	public GameObject Answers;
	public ParticleSystem ObjectParticles;
	public AudioSource ObjectSound;
	public Transform ObjectPositionTransform;

	public bool isPlaying = false;

	public AnswerSelector myAnswerSelector;

	GameObject selectedObject = null;

	float objectScaleMult = 5.0f; //what the appropriate object scale is when in this UI

	// Use this for initialization
	void Start () {
		Enable (false);
	}

	//no object
	public IEnumerator Play(){
		isPlaying = true;

		Enable (true);
		Answers.gameObject.SetActive (false);
		myAnswerSelector.SetShouldCheckForInput(false);

		Answers.gameObject.SetActive (true);
		myAnswerSelector.SetShouldCheckForInput (true);

		yield return 0;
	}

	//show an object
	public IEnumerator Play(GameObject objectToSelect){
		isPlaying = true;

		Enable (true);
		Answers.gameObject.SetActive (false);
		myAnswerSelector.SetShouldCheckForInput (false);

		PlayObjectJuice ();

		selectedObject = objectToSelect;
		selectedObject.transform.position = ObjectPositionTransform.position;
		SpawnableObject selectedObjectSpawnable = selectedObject.GetComponent<SpawnableObject> ();
		selectedObjectSpawnable.TurnVisible (true);
		selectedObjectSpawnable.SetShadowCasting (false); //turn off shadows, they look weird in this case.
		selectedObjectSpawnable.Scale (objectScaleMult);

		ObjectNameTextMesh.text = selectedObjectSpawnable.GetName ();

		UsefulFunctions.FaceObject (objectToSelect, exp.player.gameObject, false); //make UI copy face the player

		Answers.gameObject.SetActive (true);
		myAnswerSelector.SetShouldCheckForInput (true);

		yield return 0;
	}

	void PlayObjectJuice(){
		JuiceController.PlayParticles (ObjectParticles);
		AudioController.PlayAudio (ObjectSound);
	}

	public void Stop(){
		isPlaying = false;

		if (selectedObject != null) {
			Destroy(selectedObject);
		}
		Enable (false);
	}

	void Enable(bool shouldEnable){
		GetComponent<EnableChildrenLogTrack>().LogChildrenEnabled(shouldEnable);

		UsefulFunctions.EnableChildren( transform, shouldEnable );
	}

}
