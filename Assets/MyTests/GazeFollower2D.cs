using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using iView;
public class GazeFollower2D : MonoBehaviour
{

    public Canvas myCanvas;
    private Ray ray;
    private RaycastHit hit;
    public Vector3 gazeFollower { get { return gameObject.transform.position; } set { gameObject.transform.position = value; } }
    public LayerMask mask;
    public EyetrackerLogTrack eyetrackerLogTrack;
    private float timer = 0f;
    private Vector2 screenGazePos;
    public bool edgeConfidence = false;
    private bool blinkConfidence = false;
    public Image leftEye;
    public Image rightEye;
    public float factor = 3f;
    int widthLimit = 10;
    int heightLimit = 7;
    bool allowOnce = true;
    public bool ETStatus = true;
    public Text reconnectionInstructionText;
    bool reconnectionActive = false;
    public Text reconnectionTitleText;
    public GameObject reconnectionPanel;


    public CanvasGroup calibrationResults;

    public Text calibrationComplete;
    public Text validationComplete;
    public Text validationResults;
        
    //EXPERIMENT IS A SINGLETON
    private static GazeFollower2D _instance;

    public static GazeFollower2D Instance
    {
        get
        {
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance != null)
        {
            Debug.Log("Instance already exists!");
            return;
        }
        _instance = this;
        //  gazeFollower = GetComponent<Transform>();

    }

    // Use this for initialization
    void Start()
    {

        SMIGazeController.CalibrationBegan += CalibrationStarted;
        SMIGazeController.CalibrationStopped += CalibrationEnded;
        SMIGazeController.EyetrackerSetupFailed += SetupFailed;
        widthLimit = Mathf.CeilToInt(Screen.width / 192);
        heightLimit = Mathf.CeilToInt(Screen.height / 192);
        reconnectionInstructionText.enabled = false;
        reconnectionTitleText.enabled = false;
        reconnectionPanel.SetActive(false);
        calibrationResults.alpha = 0f;
        EnableCalibrationUI(false);
        EnableValidationUI(false);
    }

    void CalibrationStarted()
    {
		
        ETStatus = true;
        eyetrackerLogTrack.LogCalibrationStarted(5);
        Debug.Log("CALIBRATION HAS STARTED");
    }

    void CalibrationEnded()
    {
        EnableValidationUI(true);
        eyetrackerLogTrack.LogCalibrationEnded(5);
    }

    void SetupFailed()
    {

        UnityEngine.Debug.Log("FAILED");
        ETStatus = false;
    }

    
    public IEnumerator ShowEyeReconnectionScreen()
    {
        reconnectionActive = true;
        reconnectionPanel.SetActive(true);
        yield return StartCoroutine(ReconnectionProcedure());
        //please hold position

        Debug.Log("done with procedure");
        reconnectionInstructionText.text = "Please maintain this posture during the session";

        Debug.Log("waited for two seconds...TURNING things off");
        leftEye.enabled = false;
        rightEye.enabled = false;
        Experiment.Instance.trialController.TogglePause();  //unpause now
        yield return new WaitForSeconds(2f);
        
        reconnectionInstructionText.enabled= false;
        reconnectionTitleText.enabled = false;
        reconnectionActive = false;
        reconnectionPanel.SetActive(false);
        yield return null;
    }

    IEnumerator ReconnectionProcedure()
    {
        Debug.Log("starting reconnection procedure");
        float leftDepth = 0f;
        float rightDepth = 0f;
		float leftXPos = 0f;
		float rightXPos = 0f;
        Vector2 leftPos, rightPos;
        reconnectionTitleText.enabled = true;
        reconnectionInstructionText.enabled = true;
        Experiment.Instance.trialController.TogglePause(); //pause game
                                                           // GetComponent<Image>().enabled = true; //enable averaged eye indicator
        leftEye.enabled = true;
        float minTimer = 0f;
        rightEye.enabled = true;
        while ((leftDepth > 70f && leftDepth < 60f && rightDepth < 60f && rightDepth > 70f) || (leftDepth == 0 || rightDepth == 0))
        {
			if (leftDepth < 60f || rightDepth < 60f) {
				reconnectionInstructionText.text = "Please move closer to the screen";
			} else if (leftDepth > 70f || rightDepth > 70f) {
				reconnectionInstructionText.text = "Please move further from the screen";
			} else if (leftXPos < 100f || rightXPos < 100f) {
				reconnectionInstructionText.text = "Please move a bit to the right";
			} else if (leftXPos > Screen.width - 100f || rightXPos > Screen.width - 100f) {
				reconnectionInstructionText.text = " Please move a bit to the left";
			}
            minTimer += Time.deltaTime;
            leftPos = SMIGazeController.Instance.GetSample().leftEye.gazePosInUnityScreenCoords();
            rightPos = SMIGazeController.Instance.GetSample().rightEye.gazePosInUnityScreenCoords();
            Vector2 left, right;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(myCanvas.transform as RectTransform, leftPos, myCanvas.worldCamera, out left);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(myCanvas.transform as RectTransform, rightPos, myCanvas.worldCamera, out right);
            leftEye.transform.position = myCanvas.transform.TransformPoint(left);
            rightEye.transform.position = myCanvas.transform.TransformPoint(right);
			leftXPos = SMIGazeController.Instance.GetSample ().leftEye.eyePosition.x;
			rightXPos = SMIGazeController.Instance.GetSample ().rightEye.eyePosition.x;
            leftDepth = SMIGazeController.Instance.GetSample().leftEye.eyePosition.z / 10f;
            rightDepth = SMIGazeController.Instance.GetSample().rightEye.eyePosition.z / 10f;

            /*Debug.Log(minTimer);
            Debug.Log("LEFT: " + leftDepth);
            Debug.Log("RIGHT: " + rightDepth);
            */
            yield return 0;
        }
        yield return null;
    }

    IEnumerator CheckEyeDetection()
    {
        float timer = 0f;
       // Debug.Log("starting eye detection check;");
        while (edgeConfidence && !reconnectionActive)
        {
            //Debug.Log("eye wait: " + timer);
            timer += Time.deltaTime;
            if (timer > Config.eyeDetectionToleranceTime)
            {
                StartCoroutine("ShowEyeReconnectionScreen");
                timer = 0f;
            }
            yield return 0;
        }
        yield return null;
    }
    public void EnableCalibrationUI(bool shouldEnable)
    {
        if (shouldEnable)
        {
            calibrationResults.alpha = 1f;
        }
        else
            calibrationResults.alpha = 0f;
        calibrationComplete.gameObject.SetActive(shouldEnable);
    }
    public void EnableValidationUI(bool shouldEnable)
    {
        validationComplete.gameObject.SetActive(shouldEnable);
        double leftXDev = SMIGazeController.Instance.leftXDeviation;
        double leftYDev = SMIGazeController.Instance.leftYDeviation;
        double rightXDev = SMIGazeController.Instance.rightXDeviation;
        double rightYDev = SMIGazeController.Instance.rightYDeviation;
        validationResults.text = " Left X/Y:  " + leftXDev.ToString("F2") + " / " + leftYDev.ToString("F2") + "\n Right X/Y:  " + rightXDev.ToString("F2") + " / " + rightYDev.ToString("F2");
    }

    // Update is called once per frame
    void Update()
    {
        if (gazeFollower != null && ETStatus)
        {

          /*  if (Input.GetKeyDown(KeyCode.P))
            {
                StartCoroutine("ShowEyeReconnectionScreen");
            
            // GetComponent<Image>().enabled = !(GetComponent<Image>().enabled);
        }
        */
            Vector2 temp = SMIGazeController.Instance.GetSample().averagedEye.gazePosInUnityScreenCoords();
           
            if (temp.x <= 10 || temp.y <= 6)
            {
                edgeConfidence = true;
            //    Debug.Log("EDGE CONFIDENCE ON THIS");
            }
            else
            {
                edgeConfidence = false;
            }
            /*
            if (edgeConfidence)
            {
                    StartCoroutine("ShowEyeReconnectionScreen");
                

            }
    */
            screenGazePos = temp;
            //Debug.Log("SCREEN POS: " + screenGazePos);
            eyetrackerLogTrack.LogScreenGazePoint(screenGazePos, edgeConfidence, blinkConfidence);
            double leftPupilDiameter = SMIGazeController.Instance.GetSample().leftEye.pupilDiameter;
            double rightPupilDiameter = SMIGazeController.Instance.GetSample().rightEye.pupilDiameter;
            double averagedPupilDiameter = SMIGazeController.Instance.GetSample().averagedEye.pupilDiameter;
            if (leftPupilDiameter == 0f || rightPupilDiameter == 0f)
            {
                blinkConfidence = true;
            }
            else
            {
                blinkConfidence = false;
            }
            eyetrackerLogTrack.LogPupilDiameter(leftPupilDiameter, rightPupilDiameter, averagedPupilDiameter);
            Vector3 worldGazePos = Camera.main.ScreenToWorldPoint(new Vector3(screenGazePos.x, screenGazePos.y, gazeFollower.z));
            eyetrackerLogTrack.LogWorldGazePoint(worldGazePos, edgeConfidence, blinkConfidence);

            //Debug.Log("WORLD POS: " + worldGazePos);
            ray = Camera.main.ScreenPointToRay(screenGazePos);
            //Vector3 gazedMovePos = ray.origin + (ray.direction * factor);
            // GazeMove.Instance.MoveSphere(gazedMovePos);

            Debug.DrawRay(new Vector3(screenGazePos.x, screenGazePos.y, 0f), worldGazePos, Color.red);
            if (Physics.SphereCast(ray, 0.8f, out hit, 100f))
            {
                if (GazeMove.Instance != null)
                    GazeMove.Instance.MoveSphere(hit.point);
            }

            //masks and looks only for buildings
            if (Physics.SphereCast(ray, 0.8f, out hit, 100f, mask.value))
            {
                // Debug.Log(hit.collider.transform.position);
                if (GazeMove.Instance != null)
                    GazeMove.Instance.MoveSphere(hit.point);
                // Debug.Log(hit.collider.gameObject.name);
                eyetrackerLogTrack.LogGazeObject(hit.collider.gameObject, hit.distance);
                // hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.green;
            }
            //gazeFollower.position = new Vector3(screenGazePos.x, screenGazePos.y, gazeFollower.position.z);
            //gazeFollower.position = worldGazePos;

            //     if (!ExperimentSettings_CoinTask.isReplay)
            //   {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(myCanvas.transform as RectTransform, screenGazePos, myCanvas.worldCamera, out pos);

            gazeFollower = myCanvas.transform.TransformPoint(pos);
            // }

        }
    }
}
