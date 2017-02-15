using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SceneController : MonoBehaviour { //there can be a separate scene controller in each scene

    public Image calibrationInstructions;
	//SINGLETON
	private static SceneController _instance;
	
	public static SceneController Instance{
		get{
			return _instance;
		}
	}
	
	void Awake(){
		if (_instance != null) {
			Debug.Log("Instance already exists!");
			Destroy(transform.gameObject);
			return;
		}
		_instance = this;
	}


	// Use this for initialization
	void Start () {
        if (calibrationInstructions!=null)
        calibrationInstructions.enabled = false;

	}


	// Update is called once per frame
	void Update () {

	}

    IEnumerator ShowCalibrationInstructions()
    {
        calibrationInstructions.enabled = true;
        yield return StartCoroutine(UsefulFunctions.WaitForActionButton());
        yield return null;
    }


    public void LoadMainMenu(){
		if(Experiment.Instance != null){
			Experiment.Instance.OnExit();
		}

		Debug.Log("loading main menu!");
		SubjectReaderWriter.Instance.RecordSubjects();
		Application.LoadLevel(0);
	}

    IEnumerator LoadExperimentTask()
    {
		#if EYETRACKER
        yield return StartCoroutine(ShowCalibrationInstructions());
		#endif
        Application.LoadLevel(1);
       // calibrationInstructions.enabled = false;
        yield return null;
    }

	public void LoadExperiment(){

        //should be no new data to record for the subject
		if(Experiment.Instance != null){
			Experiment.Instance.OnExit();
		}

		if (ExperimentSettings.currentSubject != null) {
			LoadExperimentLevel();
		} 
		else if (ExperimentSettings.isReplay) {
			Debug.Log ("loading experiment!");
			Application.LoadLevel (1);
		}
		else if (ExperimentSettings.Instance.isRelease){ //no subject, not replay, is pilot
			if(ExperimentSettings.currentSubject == null){
				ExperimentSettings.Instance.subjectSelectionController.SendMessage("AddNewSubject");
				if(ExperimentSettings.currentSubject != null){
					LoadExperimentLevel();
				}
			}
		}
	}

	void LoadExperimentLevel(){
		if (ExperimentSettings.currentSubject.trials < ExperimentSettings.numDelivDays) {
			Debug.Log ("loading experiment!");
            StartCoroutine("LoadExperimentTask");
		} else {
			Debug.Log ("Subject has already finished all blocks! Loading end menu.");
			Application.LoadLevel (2);
		}
	}

	public void LoadEndMenu(){
		if(Experiment.Instance != null){
			Experiment.Instance.OnExit();
		}

		SubjectReaderWriter.Instance.RecordSubjects();
		Debug.Log("loading end menu!");
		Application.LoadLevel(2);
	}

	public void Quit(){
		SubjectReaderWriter.Instance.RecordSubjects();
		Application.Quit();
	}

	void OnApplicationQuit(){
		Debug.Log("On Application Quit!");
		SubjectReaderWriter.Instance.RecordSubjects();
	}
}
