// -----------------------------------------------------------------------
//
// (c) Copyright 1997-2015, SensoMotoric Instruments GmbH
// 
// Permission  is  hereby granted,  free  of  charge,  to any  person  or
// organization  obtaining  a  copy  of  the  software  and  accompanying
// documentation  covered  by  this  license  (the  "Software")  to  use,
// reproduce,  display, distribute, execute,  and transmit  the Software,
// and  to  prepare derivative  works  of  the  Software, and  to  permit
// third-parties to whom the Software  is furnished to do so, all subject
// to the following:
// 
// The  copyright notices  in  the Software  and  this entire  statement,
// including the above license  grant, this restriction and the following
// disclaimer, must be  included in all copies of  the Software, in whole
// or  in part, and  all derivative  works of  the Software,  unless such
// copies   or   derivative   works   are   solely   in   the   form   of
// machine-executable  object   code  generated  by   a  source  language
// processor.
// 
// THE  SOFTWARE IS  PROVIDED  "AS  IS", WITHOUT  WARRANTY  OF ANY  KIND,
// EXPRESS OR  IMPLIED, INCLUDING  BUT NOT LIMITED  TO THE  WARRANTIES OF
// MERCHANTABILITY,   FITNESS  FOR  A   PARTICULAR  PURPOSE,   TITLE  AND
// NON-INFRINGEMENT. IN  NO EVENT SHALL  THE COPYRIGHT HOLDERS  OR ANYONE
// DISTRIBUTING  THE  SOFTWARE  BE   LIABLE  FOR  ANY  DAMAGES  OR  OTHER
// LIABILITY, WHETHER  IN CONTRACT, TORT OR OTHERWISE,  ARISING FROM, OUT
// OF OR IN CONNECTION WITH THE  SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// -----------------------------------------------------------------------

using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using iView;
namespace iView
{
    public class SMIGazeController : MonoBehaviour
    {
        // enable the internal Gazefilter
        public delegate void CalibrationEvent();
        public static event CalibrationEvent CalibrationBegan;
        public static event CalibrationEvent CalibrationStopped;

        public static event CalibrationEvent EyetrackerSetupFinished;
        public static event CalibrationEvent EyetrackerSetupFailed;
        public bool useGazeFilter = true;
        public bool runningCalibration = false;
        // maximal Distance for the Rays to detect focused Objects
        public float maxDistanceForRaycasts = 100f;

        //KeyCodes to Start a Calibration
        public KeyCode startOnePointCalibration = KeyCode.Alpha1;

        public KeyCode startFivePointCalibration = KeyCode.Alpha2;

        public KeyCode startValidation = KeyCode.Alpha3;

        public bool isDoingCalibration = true;

        private bool firstTime = true;
        public bool waitingInput = false;
        private bool canStartValidation = false;

        //Thread for the initialisation of the GazeController
        private static Thread eyeThread;
        //gazeController: Use this class for the communication with the server
        private EyeTrackingController ET_Device;

        //Instance of the MonoBehaviour
        private static SMIGazeController instance;

        // Input for 2PC setup

        private string sendIP;
        private int sendPort;

        private string receiveIP;
        private int receivePort;

        public double leftXDeviation = 0f;
        public double leftYDeviation = 0f;
        public double rightXDeviation = 0f;
        public double rightYDeviation = 0f;

        #region UnityInternFunction

        void Awake()
        {

			#if EYETRACKER
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (!instance)
            {
                //Create an instance 
                instance = this;
                DontDestroyOnLoad(gameObject);
                ET_Device = new EyeTrackingController();

                //Connect to the Device
                //Single PC Setup:
                instance.InitEyeThread();

                //Two PC Setup:
                //instance.InitEyeThread(sendIP, sendPort, receiveIP, receivePort);

                instance.StartEyeThread();

            }

            else
            {
                Destroy(gameObject);
            }
#else
            Debug.LogError("You need Windows as operating system.");
#endif
			#else
			gameObject.SetActive(false);
			#endif
        }

        /// <summary>
        /// Mono Behaviour Update Function:
        /// Start the Coroutine if, the System must start a Validation/Calibration
        /// </summary>
        void Update()
        {


#if UNITY_EDITOR || UNITY_STANDALONE_WIN


            if (GetIsStartingOver())
            {

                StartCalibrationRoutine(ET_Device.getAcessToGazeModel().calibrationMethod);
                StartValidationRoutine();
                ManagePlayerInput();
                if (firstTime)
                {
                    DoCalibrationProcedure();
                    firstTime = false;
                }
               
            }
            if(waitingInput && Input.GetKeyDown(KeyCode.Tab))
            {
                DoCalibrationProcedure();
            }
            else if(waitingInput && Input.GetKeyDown(KeyCode.X))
            {
                waitingInput = false;
                isDoingCalibration = false;
            }
#else
            Debug.LogError("You need Windows as operating system.");
#endif
        }

        void DoCalibrationProcedure()
        {

            isDoingCalibration = true;
            UnityEngine.Debug.Log("doing calibration & validation");
            //ET_Device.getAcessToGazeModel().isValidationRunning = true;
            StartCalibration(9);
        }
        /// <summary>
        /// Mono Behaviour OnApplicationQuit Function:
        /// Stops the EyeTrackingController and Joins the Thread
        /// </summary>
        void OnApplicationQuit()
        {
            instance.JoinEyeThread();
        }

        #endregion

        # region PublicFunction

        /// <summary>
        /// Get the instance of the GazeController; Use this to get acess to the Gaze Data from the EyeTracker
        /// </summary>
        public static SMIGazeController Instance
        {
            get
            {
                if (!instance)
                {
                    instance = (SMIGazeController)FindObjectOfType(typeof(SMIGazeController));
                    {
                        if (!instance)
                        {

                            GameObject prefabReference = Resources.Load("EyeTrackerController") as GameObject;
                            GameObject obj = GameObject.Instantiate(prefabReference, Vector3.zero, Quaternion.identity) as GameObject;
                            obj.name = "EyeTrackingController";
                            instance = obj.GetComponent(typeof(SMIGazeController)) as SMIGazeController;
                        }
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// Pause the EyeTracker
        /// </summary>
        public void PauseEyeTracker()
        {
            try
            {
                ET_Device.PauseEyeTracker();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void EyetrackerSetupSuccess()
        {
            EyetrackerSetupFinished();
        }

        public void EyetrackerSetupFailure()
        {
            UnityEngine.Debug.Log("SHOULD FAIL");
            EyetrackerSetupFailed();
        }
        /// <summary>
        /// Continue the EyeTracker
        /// </summary>
        public void ContinueEyeTracker()
        {
            try
            {
                ET_Device.ContinueEyeTracking();
            }

            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Start A Calibration
        /// </summary>
        /// <param name="calibrationPoints"> Select the Calibration Method (e.g. 5 = Fivepoint Calibration)</param>
        public void StartCalibration(int calibrationPoints)
        {
            Debug.Log("running CALIBRATION NOW");
            CalibrationBegan();
            //runningCalibration = true;
            if (!instance.enabled)
                instance.enabled = true;

            ET_Device.getAcessToGazeModel().isCalibrationRunning = true;
            ET_Device.getAcessToGazeModel().calibrationMethod = calibrationPoints;
        }

        /// <summary>
        /// Start a Validation
        /// Use this function in your Scripts to start a Validation: Set True, if the System must start a Validation
        /// </summary>
        public void StartValidation()
        {
            if (!instance.enabled)
                instance.enabled = true;
            ET_Device.getAcessToGazeModel().isValidationRunning = true;
        }

        /// <summary>
        /// Disconnect from the Eye Tracking Server
        /// </summary>
        public void Disconnect()
        {
            ET_Device.DisconnectFromDevice();
        }

        /// <summary>
        /// Load a CalibrationProfile from the Server
        /// </summary>
        /// <param name="nameOfProfile"></param>
        public void LoadCalibrationDataFromDriver(string nameOfProfile)
        {
            ET_Device.LoadCalibration(nameOfProfile);
        }

        /// <summary>
        /// Get the CalibrationMethod (The Count Of the Calibrationpoints)
        /// </summary>
        /// <returns></returns>
        public int GetCalibrationPoints()
        {
            return ET_Device.getAcessToGazeModel().calibrationMethod;
        }

        public bool GetIsStartingOver()
        {
            return ET_Device.getAcessToGazeModel().isStartingProcessOver;
        }

        public SampleData GetSample()
        {
            return ET_Device.getAcessToGazeModel().dataSample;
        }

        public GameObject GetObjectInFocus(FocusFilter selection)
        {
            try
            {
                switch (selection)
                {
                    //3D-WorldspaceSetup
                    case FocusFilter.WorldSpaceObjects:
                        RaycastHit hit = GetRaycastHitFromGaze();
                        return hit.collider.gameObject;

                    //GUI Objects
                    case FocusFilter.GUIObjects:
                        return GetGUIInFocus();

                    //Sprites
                    case FocusFilter.SpriteObjects:

                        return GetSpriteInFocus();
                    //No Selection
                    default:
                        return null;
                }
            }

            catch (System.Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        public RaycastHit GetRaycastHitFromGaze()
        {
            RaycastHit hit;
            Ray rayGaze = Camera.main.ScreenPointToRay(SMIGazeController.Instance.GetSample().averagedEye.gazePosInUnityScreenCoords());

            if (Physics.Raycast(rayGaze, out hit, maxDistanceForRaycasts))
            {
                return hit;
            }

            return hit;
        }

        public int GetErrorID()
        {
            return ET_Device.getAcessToGazeModel().statusID;
        }


        #endregion

        #region PrivateFunction

        private void ManagePlayerInput()
        {
            if (Input.GetKeyDown(startOnePointCalibration))
            {
                StartCalibration(1);
            }
            else if (Input.GetKeyDown(startFivePointCalibration))
            {
                StartCalibration(5);
            }
            else if (Input.GetKeyDown(startValidation))
            {
                StartValidation();
            }
        }
        private GameObject GetSpriteInFocus()
        {
            Vector3 positionGaze = Camera.main.ScreenToWorldPoint(SMIGazeController.Instance.GetSample().averagedEye.gazePosInUnityScreenCoords());
            var hit = Physics2D.Raycast(positionGaze, Vector2.zero);

            if (hit.collider != null)
            {
                return hit.collider.gameObject;
            }

            return null;
        }

        private GameObject GetGUIInFocus()
        {
            //Create A PointerEvent for a Screenspace Canvas
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = ET_Device.getAcessToGazeModel().dataSample.averagedEye.gazePosInUnityScreenCoords();

            //Safe the Raycast
            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycastResults);

            if (raycastResults.Count > 0)
            {
                return raycastResults[0].gameObject;
            }
            return null;
        }

        /// <summary>
        /// Init a new Thread for the Eye Tracking Input
        /// Use this Function for a single PC setup
        /// </summary>
        private void InitEyeThread()
        {
            //SinglePC Setup
            eyeThread = new Thread(ET_Device.ConnectToDevice);
        }

        /// <summary>
        /// Init a new Thread for the Eye Tracking Input
        /// Use this Function for a two PC setup
        /// </summary>
        private void InitEyeThread(string sendIP, int sendPort, string receiveIP, int receivePort)
        {
            eyeThread = new Thread(() => ET_Device.ConnectToDevice(sendIP, sendPort, receiveIP, receivePort));
        }

        /// <summary>
        /// Start the EyeTrackingThread
        /// </summary>
        private void StartEyeThread()
        {
            eyeThread.Start();
            SetUseGazeFilter();
        }

        /// <summary>
        /// Stop the Datastream from the Eye Tracker and join the Thread.
        /// </summary>
        private void JoinEyeThread()
        {
            ET_Device.DisconnectFromDevice();
            eyeThread.Join();
        }

        /// <summary>
        /// Enable / Disable the GazeFilter
        /// </summary>
        private void SetUseGazeFilter()
        {
            ET_Device.SetGazeFilterActive(useGazeFilter);
        }

        /// <summary>
        /// Start a Coroutine to hide the gameView and open a CalibrationScreen from the Server
        /// </summary>
        private void StartCalibrationRoutine(int calibrationPoints)
        {
            if (ET_Device.getAcessToGazeModel().isCalibrationRunning)
            {
                ET_Device.getAcessToGazeModel().isCalibrationRunning = false;
                StartCoroutine(IEnumerator_StartCalibration(calibrationPoints));
            }
        }

        /// <summary>
        /// Start a Coroutine to hide the gameView and open a ValidationScreen from the Server
        /// </summary>
        private void StartValidationRoutine()
        {
            if (ET_Device.getAcessToGazeModel().isValidationRunning)
            {
                ET_Device.getAcessToGazeModel().isValidationRunning = false;
                StartCoroutine(IEnuerator_Start_Validation());
            }
        }
        #endregion


        /// <summary>
        /// Disable Fullscreen
        /// Wait one Frame
        /// Start the Calibration from the Server
        /// </summary>
        IEnumerator IEnumerator_StartCalibration(int calibrationPoints)
        {
            if (Screen.fullScreen == true)
                Screen.fullScreen = false;

            yield return new WaitForFixedUpdate();
            ET_Device.StartCalibration();
            Screen.fullScreen = true;
            // EyeTrackingController.Instance.
            

            //run validation now

            StartValidation();
            StartValidationRoutine();
            
            yield return null;
        }

        IEnumerator DisplayCalibrationResults()
        {
            ET_Device.ShowAccuracyImage(true);
            yield return new WaitForSeconds(5f);
            ET_Device.ShowAccuracyImage(false);
            yield return null;
        }

        /// <summary>
        /// Disable Fullscreen
        /// Wait one Frame
        /// Start the Validation from the Server
        /// </summary>
        IEnumerator IEnuerator_Start_Validation()
        {
            if (Screen.fullScreen == true)
                Screen.fullScreen = false;

            yield return new WaitForFixedUpdate();
            UnityEngine.Debug.Log("in validation");
            ET_Device.StartValidation();
            Screen.fullScreen = true;

            Debug.Log("end of validation");
            yield return null;
        }

        public void ShowAccuracyResults()
        {
            ET_Device.ShowAccuracyImage(true);
           
        }

        public void PrintAccuracyResults(double leftXDev,double leftYDev,double rightXDev,double rightYDev)
        {
            leftXDeviation = leftXDev;
            leftYDeviation = leftYDev;
            rightXDeviation = rightXDev;
            rightYDeviation = rightYDev;
            CalibrationStopped();
            waitingInput = true;
            Debug.Log("end of calibration+validation procedure");

            UnityEngine.Debug.Log("LEFT X DEVIATION: " + leftXDev.ToString());
            UnityEngine.Debug.Log("LEFT Y DEVIATION: " + leftYDev.ToString());
            UnityEngine.Debug.Log("RIGHT X DEVIATION: " + rightXDev.ToString());
            UnityEngine.Debug.Log("RIGHT X DEVIATION: " + rightYDev.ToString());
        }
    }
}