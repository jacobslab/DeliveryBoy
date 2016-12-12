using UnityEngine;
using System.Collections;

public class SceneController : MonoBehaviour { //there can be a separate scene controller in each scene

    public GameObject calibrationInstructionPanel;
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

    void OnEnable()
    {
        calibrationInstructionPanel.SetActive(false);
    }

	// Use this for initialization
	void Start () {

	}


	// Update is called once per frame
	void Update () {

	}

	public void LoadMainMenu(){
		if(Experiment.Instance != null){
			Experiment.Instance.OnExit();
		}

		Debug.Log("loading main menu!");
		SubjectReaderWriter.Instance.RecordSubjects();
		Application.LoadLevel(0);
	}

	public void LoadExperiment(){
		//should be no new data to record for the subject
		if(Experiment.Instance != null){
			Experiment.Instance.OnExit();
		}

		if (ExperimentSettings.currentSubject != null) {
            
		} 
		else if (ExperimentSettings.isReplay) {
			Debug.Log ("loading experiment!");
			Application.LoadLevel (1);
		}
		else if (ExperimentSettings.Instance.isRelease){ //no subject, not replay, is pilot
			if(ExperimentSettings.currentSubject == null){
				ExperimentSettings.Instance.subjectSelectionController.SendMessage("AddNewSubject");
				if(ExperimentSettings.currentSubject != null){
                    //DON'T forget to check whether eyetracker is connected to the computer or not
                    Debug.Log("should be showing calibration instructions");
                    StartCoroutine("ShowCalibrationInstructions");
                }
			}
		}
	}


    //calibration instruction related functions
     IEnumerator ShowCalibrationInstructions()
    {
        calibrationInstructionPanel.SetActive(true);
        yield return StartCoroutine(UsefulFunctions.WaitForActionButton());
        ProceedWithCalibration();
        yield return null;
    }

    void ProceedWithCalibration()
    {
        LoadExperimentLevel();
    }

	void LoadExperimentLevel(){
		if (ExperimentSettings.currentSubject.trials < ExperimentSettings.numDelivDays) {
			Debug.Log ("loading experiment!");
            Application.LoadLevel (1);
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
