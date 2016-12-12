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
    private bool lowConfidence = false;
    public float factor = 3f;
    int widthLimit = 10;
    int heightLimit = 7;
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
        widthLimit = Mathf.CeilToInt(Screen.width / 192);
        heightLimit = Mathf.CeilToInt(Screen.height / 192);
    }

    void CalibrationStarted()
    {
        eyetrackerLogTrack.LogCalibrationStarted(5);
        Debug.Log("CALIBRATION HAS STARTED");
    }

    void CalibrationEnded()
    {
        eyetrackerLogTrack.LogCalibrationEnded(5);
    }

    // Update is called once per frame
    void Update()
    {

        if (gazeFollower != null)
        {

            if (Input.GetKeyDown(KeyCode.P))
            {
                GetComponent<Image>().enabled = !(GetComponent<Image>().enabled);
            }
            Vector2 temp = SMIGazeController.Instance.GetSample().averagedEye.gazePosInUnityScreenCoords();

            if (temp.x <= widthLimit || temp.y <= heightLimit)
            {
                lowConfidence = true;
                Debug.Log("LOW CONFIDENCE ON THIS");
            }
            else
            {
                lowConfidence = false;
            }

            screenGazePos = temp;
            //Debug.Log("SCREEN POS: " + screenGazePos);
            eyetrackerLogTrack.LogScreenGazePoint(screenGazePos, lowConfidence);
            double leftPupilDiameter = SMIGazeController.Instance.GetSample().leftEye.pupilDiameter;
            double rightPupilDiameter = SMIGazeController.Instance.GetSample().rightEye.pupilDiameter;
            double averagedPupilDiameter = SMIGazeController.Instance.GetSample().averagedEye.pupilDiameter;
            eyetrackerLogTrack.LogPupilDiameter(leftPupilDiameter, rightPupilDiameter, averagedPupilDiameter);
            Vector3 worldGazePos = Camera.main.ScreenToWorldPoint(new Vector3(screenGazePos.x, screenGazePos.y, gazeFollower.z));
            eyetrackerLogTrack.LogWorldGazePoint(worldGazePos, lowConfidence);

            //Debug.Log("WORLD POS: " + worldGazePos);
            ray = Camera.main.ScreenPointToRay(screenGazePos);
            //Vector3 gazedMovePos = ray.origin + (ray.direction * factor);
            // GazeMove.Instance.MoveSphere(gazedMovePos);

            Debug.DrawRay(new Vector3(screenGazePos.x, screenGazePos.y, 0f), worldGazePos, Color.red);
            if (Physics.SphereCast(ray, 0.8f, out hit, 100f))
            {
                if(GazeMove.Instance!=null)
                    GazeMove.Instance.MoveSphere(hit.point);
            }

            //masks and looks only for buildings
            if (Physics.SphereCast(ray, 0.8f, out hit, 100f, mask.value))
            {
               // Debug.Log(hit.collider.transform.position);
                if (GazeMove.Instance != null)
                    GazeMove.Instance.MoveSphere(hit.point);
                // Debug.Log(hit.collider.gameObject.name);
                eyetrackerLogTrack.LogGazeObject(hit.collider.gameObject);
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
