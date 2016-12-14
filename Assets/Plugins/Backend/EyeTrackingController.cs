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

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace iView
{

    public class EyeTrackingController
    {

        #region gazeController

        //Sample Data
        private AccuracyStruct m_AccuracyData;
        private SDKSampleStruct m_SampleData;
        private SystemInfoStruct m_SystemData;
        private CalibrationStruct m_CalibrationData;
        private REDGeometryStruct m_GeometryData;

        //Sample callBack 
        private GetSampleCallBack m_samplecallBack;
        private delegate void GetSampleCallBack(SDKSampleStruct sampleData);

        //Instance of the GazeModel
        private GazeModel gazeModel;


        //EXPERIMENT IS A SINGLETON
        private static EyeTrackingController _instance;

        public static EyeTrackingController Instance
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



        }
        /// <summary>
         /// Init the Controller
         /// </summary>
        public EyeTrackingController()
        {
            m_samplecallBack = GetSampleCallbackFunction;
            gazeModel = new GazeModel(Screen.width, Screen.height);
        }

        /// <summary>
        /// Returns the instance of the GazeModel instance
        /// </summary>
        /// <returns></returns>
        public GazeModel getAcessToGazeModel()
        {
            return gazeModel;
        }

        /// <summary>
        /// Connect Local
        /// </summary>
        /// <returns></returns>
        public void ConnectToDevice()
        {
            int resultID = iV_ConnectLocal();
            iV_SetSampleCallback(m_samplecallBack);
            GetLogData(resultID, IDContainerIviewNG.STATE_CONNECT);
            UnityEngine.Debug.Log("resultID" + resultID.ToString());
            gazeModel.isStartingProcessOver = true;
        }

        /// <summary>
        /// Connect to the Remote Device 
        /// </summary>
        /// <param name="sendIP"></param>
        /// <param name="sendPort"></param>
        /// <param name="receiveIP"></param>
        /// <param name="receivePort"></param>
        /// <returns></returns>
        public void ConnectToDevice(string sendIP, int sendPort, string receiveIP, int receivePort)
        {
            int resultID = iV_Connect(new StringBuilder(sendIP), sendPort, new StringBuilder(receiveIP), receivePort);
            GetLogData(resultID, IDContainerIviewNG.STATE_CONNECT);

            gazeModel.isStartingProcessOver = true;
        }

        /// <summary>
        /// Disconnect from the Server
        /// </summary>
        /// <returns></returns>
        public void DisconnectFromDevice()
        {
            int resultID = iV_Disconnect();
            GetLogData(resultID, IDContainerIviewNG.STATE_DISCONNET);
        }

        /// <summary>
        /// Pause the EyeTracker
        /// </summary>
        /// <returns></returns>
        public void PauseEyeTracker()
        {
            int resultID = iV_PauseEyetracking();
            GetLogData(resultID, IDContainerIviewNG.STATE_PAUSE);

            if (resultID == IDContainerIviewNG.ACTION_COMPLETE)
            {
                gazeModel.isStartingProcessOver = false;
            }
        }

        /// <summary>
        /// Continue Eye Tracking
        /// </summary>
        public void ContinueEyeTracking()
        {
            int resultID = iV_ContinueEyetracking();
            GetLogData(resultID, IDContainerIviewNG.STATE_CONTINUE);

            if (resultID == IDContainerIviewNG.ACTION_COMPLETE)
            {
                gazeModel.isStartingProcessOver = true;
            }
        }

        /// <summary>
        /// Setup the internal GazeFilter
        /// </summary>
        /// <param name="isActive"></param>
        public void SetGazeFilterActive(bool isActive)
        {
            if (gazeModel.isStartingProcessOver)
            {
                int resultID;

                if (isActive)
                {
                    resultID = iV_EnableGazeDataFilter();
                }

                else
                {
                    resultID = iV_DisableGazeDataFilter();
                }

                GetLogData(resultID, IDContainerIviewNG.STATE_GAZEFILTER);
            }
        }

        public void LoadCalibration(string profileName)
        {
            int resultID = iV_LoadCalibration(new StringBuilder(profileName));
            GetLogData(resultID, IDContainerIviewNG.STATE_CALIBRATE);
        }

        /// <summary>
        /// Start a Calibration from the Server
        /// Use this function in your Scripts to start a Validation: Set True, if the System must start a Validation
        /// </summary>
        public void StartCalibration()
        {
            int resultID = 0;

            //Update the Geometry and the Systeminformation
            updateGeometryInformation();
            updateSystemInformation();

            // Example of a Calibration
            int displayDevice = 0;
            int calibrationPoints = SMIGazeController.Instance.GetCalibrationPoints();
            int targetSize = 50;

            m_CalibrationData.displayDevice = displayDevice;
            m_CalibrationData.autoAccept = 1;
            m_CalibrationData.method = calibrationPoints;
            m_CalibrationData.visualization = 1;
            m_CalibrationData.speed = 1;
            m_CalibrationData.targetShape = 2;
            m_CalibrationData.backgroundColor = 0;
            m_CalibrationData.foregroundColor = 250;
            m_CalibrationData.targetSize = targetSize;
            m_CalibrationData.targetFilename = "";

            resultID = iV_SetupCalibration(ref m_CalibrationData);

            //Start the calibration
            resultID = iV_Calibrate();
            
            //ErrorMessage
            GetLogData(resultID, IDContainerIviewNG.STATE_CALIBRATE);
        }
        
        /// <summary>
        /// Start a Validation
        /// Use this function in your Scripts to start a Validation: Set True, if the System must start a Validation
        /// </summary>
        public void StartValidation()
        {
            gazeModel.isValidationRunning = true;
        }

        /// <summary>
        /// Write the SampleData into the gazeModel
        /// </summary>
        /// <param name="sampleData"></param>
        private void GetSampleCallbackFunction(SDKSampleStruct sampleData)
        {
            //Update the Samples in the model
            gazeModel.WriteEyeSamplesInGazeModel(sampleData);
        }

        /// <summary>
        /// Get the ErrorMessage from the ErrorID
        /// </summary>
        /// <param name="errorID"></param>
        /// <param name="state"></param>
        private void GetLogData(int errorID, int state)
        {
            gazeModel.statusID = errorID;
            if (errorID > IDContainerIviewNG.ACTION_COMPLETE && errorID <402)
            {
                Debug.LogError("Error by " + IDContainerIviewNG.getState(state) + ": " + IDContainerIviewNG.getErrorMessage(errorID));
            }
        }

        /// <summary>
        /// Get the Geometryinfos of the Screen and the Systeminformations from the Client
        /// </summary>
        private void updateGeometryInformation()
        {
            //Get The Resolution of the Application
            Vector2 screenRes = getAcessToGazeModel().getScreenResolution();
            iV_SetResolution((int)screenRes.x, (int)screenRes.y);

            //GetGeometryinformation
            int id = (iV_GetCurrentREDGeometry(ref m_GeometryData));
            GetLogData(id, IDContainerIviewNG.STATE_GETGEOMETRYDATA);
        }

        /// <summary>
        /// Get Systeminformations from the Client
        /// </summary>
        private void updateSystemInformation()
        {
            //GetSysteminformation
            int id = iV_GetSystemInfo(ref m_SystemData);
            GetLogData(id, IDContainerIviewNG.STATE_SYSTEMDATA);
        }

        #endregion

        #region API
        // the dll to load
        //use for 32 bit
        const string dllName = "iViewXAPI.dll";

        //use for 64 bit
        //const string dllName = "iViewXAPI64.dll";

        // Kernel Function definition 

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);


        // API Function definition. See the manual for further description. 

        [DllImport(dllName, EntryPoint = "iV_AbortCalibration")]
        private static extern int Unmanaged_AbortCalibration();

        [DllImport(dllName, EntryPoint = "iV_AcceptCalibrationPoint")]
        private static extern int Unmanaged_AcceptCalibrationPoint();

        [DllImport(dllName, EntryPoint = "iV_Calibrate")]
        private static extern int Unmanaged_Calibrate();

        [DllImport(dllName, EntryPoint = "iV_ChangeCalibrationPoint")]
        private static extern int Unmanaged_ChangeCalibrationPoint(int number, int positionX, int positionY);

        [DllImport(dllName, EntryPoint = "iV_ClearRecordingBuffer")]
        private static extern int Unmanaged_ClearRecordingBuffer();

        [DllImport(dllName, EntryPoint = "iV_Connect")]
        private static extern int Unmanaged_Connect(StringBuilder sendIPAddress, int sendPort, StringBuilder recvIPAddress, int receivePort);

        [DllImport(dllName, EntryPoint = "iV_ConnectLocal")]
        private static extern int Unmanaged_ConnectLocal();

        [DllImport(dllName, EntryPoint = "iV_ContinueEyetracking")]
        private static extern int Unmanaged_ContinueEyetracking();

        [DllImport(dllName, EntryPoint = "iV_ContinueRecording")]
        private static extern int Unmanaged_ContinueRecording(StringBuilder etMessage);

        [DllImport(dllName, EntryPoint = "iV_DeleteREDGeometry")]
        private static extern int Unmanaged_DeleteREDGeometry(StringBuilder name);

        [DllImport(dllName, EntryPoint = "iV_DisableGazeDataFilter")]
        private static extern int Unmanaged_DisableGazeDataFilter();

        [DllImport(dllName, EntryPoint = "iV_DisableProcessorHighPerformanceMode")]
        private static extern int Unmanaged_DisableProcessorHighPerformanceMode();

        [DllImport(dllName, EntryPoint = "iV_Disconnect")]
        private static extern int Unmanaged_Disconnect();

        [DllImport(dllName, EntryPoint = "iV_EnableGazeDataFilter")]
        private static extern int Unmanaged_EnableGazeDataFilter();

        [DllImport(dllName, EntryPoint = "iV_EnableProcessorHighPerformanceMode")]
        private static extern int Unmanaged_EnableProcessorHighPerformanceMode();

        [DllImport(dllName, EntryPoint = "iV_GetAccuracy")]
        private static extern int Unmanaged_GetAccuracy(ref AccuracyStruct accuracyData, int visualization);

        [DllImport(dllName, EntryPoint = "iV_GetCalibrationParameter")]
        private static extern int Unmanaged_GetCalibrationParameter(ref CalibrationStruct calibrationData);

        [DllImport(dllName, EntryPoint = "iV_GetCalibrationPoint")]
        private static extern int Unmanaged_GetCalibrationPoint(int calibrationPointNumber, ref CalibrationPointStruct calibrationPoint);

        [DllImport(dllName, EntryPoint = "iV_GetCurrentCalibrationPoint")]
        private static extern int Unmanaged_GetCurrentCalibrationPoint(ref CalibrationPointStruct actualCalibrationPoint);

        [DllImport(dllName, EntryPoint = "iV_GetCurrentREDGeometry")]
        private static extern int Unmanaged_GetCurrentREDGeometry(ref REDGeometryStruct geometry);

        [DllImport(dllName, EntryPoint = "iV_GetCurrentTimestamp")]
        private static extern int Unmanaged_GetCurrentTimestamp(ref Int64 currentTimestamp);

        [DllImport(dllName, EntryPoint = "iV_GetEvent")]
        private static extern int Unmanaged_GetEvent(ref EventStruct eventDataSample);

        [DllImport(dllName, EntryPoint = "iV_GetFeatureKey")]
        private static extern int Unmanaged_GetFeatureKey(ref Int64 featureKey);

        [DllImport(dllName, EntryPoint = "iV_GetGeometryProfiles")]
        private static extern int Unmanaged_GetGeometryProfiles(int maxSize, ref StringBuilder profileNames);

        [DllImport(dllName, EntryPoint = "iV_GetREDGeometry")]
        private static extern int Unmanaged_GetREDGeometry(StringBuilder profileName, ref REDGeometryStruct geometry);

        [DllImport(dllName, EntryPoint = "iV_GetSample")]
        private static extern int Unmanaged_GetSample(ref SDKSampleStruct rawDataSample);

        [DllImport(dllName, EntryPoint = "iV_GetSerialNumber")]
        private static extern int Unmanaged_GetSerialNumber(ref StringBuilder serialNumber);

        [DllImport(dllName, EntryPoint = "iV_GetDeviceName")]
        private static extern int Unmanaged_GetDeviceName(ref StringBuilder serialNumber);

        [DllImport(dllName, EntryPoint = "iV_GetSystemInfo")]
        private static extern int Unmanaged_GetSystemInfo(ref SystemInfoStruct systemInfoData);

        [DllImport(dllName, EntryPoint = "iV_IsConnected")]
        private static extern int Unmanaged_IsConnected();

        [DllImport(dllName, EntryPoint = "iV_LoadCalibration")]
        private static extern int Unmanaged_LoadCalibration(StringBuilder name);

        [DllImport(dllName, EntryPoint = "iV_Log")]
        private static extern int Unmanaged_Log(StringBuilder logMessage);

        [DllImport(dllName, EntryPoint = "iV_PauseEyetracking")]
        private static extern int Unmanaged_PauseEyetracking();

        [DllImport(dllName, EntryPoint = "iV_PauseRecording")]
        private static extern int Unmanaged_PauseRecording();

        [DllImport(dllName, EntryPoint = "iV_Quit")]
        private static extern int Unmanaged_Quit();

        [DllImport(dllName, EntryPoint = "iV_ResetCalibrationPoints")]
        private static extern int Unmanaged_ResetCalibrationPoints();

        [DllImport(dllName, EntryPoint = "iV_SaveCalibration")]
        private static extern int Unmanaged_SaveCalibration(StringBuilder aoiName);

        [DllImport(dllName, EntryPoint = "iV_SaveData")]
        private static extern int Unmanaged_SaveData(StringBuilder filename, StringBuilder description, StringBuilder user, int overwrite);

        [DllImport(dllName, EntryPoint = "iV_SendCommand")]
        private static extern int Unmanaged_SendCommand(StringBuilder etMessage);

        [DllImport(dllName, EntryPoint = "iV_SendImageMessage")]
        private static extern int Unmanaged_SendImageMessage(StringBuilder etMessage);

        [DllImport(dllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "iV_SetCalibrationCallback")]
        private static extern void Unmanaged_SetCalibrationCallback(MulticastDelegate calibrationCallbackFunction);

        [DllImport(dllName, EntryPoint = "iV_SetConnectionTimeout")]
        private static extern int Unmanaged_SetConnectionTimeout(int time);

        [DllImport(dllName, EntryPoint = "iV_SetGeometryProfile")]
        private static extern int Unmanaged_SetGeometryProfile(StringBuilder profileName);

        [DllImport(dllName, EntryPoint = "iV_SetResolution")]
        private static extern int Unmanaged_SetResolution(int stimulusWidth, int stimulusHeight);

        [DllImport(dllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "iV_SetEventCallback")]
        private static extern void Unmanaged_SetEventCallback(MulticastDelegate eventCallbackFunction);

        [DllImport(dllName, EntryPoint = "iV_SetEventDetectionParameter")]
        private static extern int Unmanaged_SetEventDetectionParameter(int minDuration, int maxDispersion);

        [DllImport(dllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "iV_SetEyeImageCallback")]
        private static extern void Unmanaged_SetEyeImageCallback(MulticastDelegate eyeImageCallbackFunction);

        [DllImport(dllName, EntryPoint = "iV_SetLicense")]
        private static extern int Unmanaged_SetLicense(StringBuilder licenseKey);

        [DllImport(dllName, EntryPoint = "iV_SetLogger")]
        private static extern int Unmanaged_SetLogger(int logLevel, StringBuilder filename);

        [DllImport(dllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "iV_SetSampleCallback")]
        private static extern void Unmanaged_SetSampleCallback(MulticastDelegate sampleCallbackFunction);

        [DllImport(dllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "iV_SetSceneVideoCallback")]
        private static extern void Unmanaged_SetSceneVideoCallback(MulticastDelegate sceneVideoCallbackFunction);

        [DllImport(dllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "iV_SetTrackingMonitorCallback")]
        private static extern void Unmanaged_SetTrackingMonitorCallback(MulticastDelegate trackingMonitorCallbackFunction);

        [DllImport(dllName, EntryPoint = "iV_SetTrackingParameter")]
        private static extern int Unmanaged_SetTrackingParameter(int ET_PARAM_EYE, int ET_PARAM, int value);

        [DllImport(dllName, EntryPoint = "iV_SetupCalibration")]
        private static extern int Unmanaged_SetupCalibration(ref CalibrationStruct calibrationData);

        [DllImport(dllName, EntryPoint = "iV_SetREDGeometry")]
        private static extern int Unmanaged_SetREDGeometry(ref REDGeometryStruct redGeometry);

        [DllImport(dllName, EntryPoint = "iV_ShowAccuracyMonitor")]
        private static extern int Unmanaged_ShowAccuracyMonitor();

        [DllImport(dllName, EntryPoint = "iV_ShowEyeImageMonitor")]
        private static extern int Unmanaged_ShowEyeImageMonitor();

        [DllImport(dllName, EntryPoint = "iV_HideEyeImageMonitor")]
        private static extern int Unmanaged_HideEyeImageMonitor();

        [DllImport(dllName, EntryPoint = "iV_ShowSceneVideoMonitor")]
        private static extern int Unmanaged_ShowSceneVideoMonitor();

        [DllImport(dllName, EntryPoint = "iV_HideSceneVideoMonitor")]
        private static extern int Unmanaged_HideSceneVideoMonitor();

        [DllImport(dllName, EntryPoint = "iV_HideAccuracyMonitor")]
        private static extern int Unmanaged_HideAccuracyMonitor();

        [DllImport(dllName, EntryPoint = "iV_ShowTrackingMonitor")]
        private static extern int Unmanaged_ShowTrackingMonitor();

        [DllImport(dllName, EntryPoint = "iV_HideTrackingMonitor")]
        private static extern int Unmanaged_HideTrackingMonitor();

        [DllImport(dllName, EntryPoint = "iV_Start")]
        private static extern int Unmanaged_Start(int etApplication);

        [DllImport(dllName, EntryPoint = "iV_StartRecording")]
        private static extern int Unmanaged_StartRecording();

        [DllImport(dllName, EntryPoint = "iV_StopRecording")]
        private static extern int Unmanaged_StopRecording();

        [DllImport(dllName, EntryPoint = "iV_TestTTL")]
        private static extern int Unmanaged_TestTTL(long value);

        [DllImport(dllName, EntryPoint = "iV_Validate")]
        private static extern int Unmanaged_Validate();

        private int iV_AbortCalibration()
        {
            return Unmanaged_AbortCalibration();
        }

        private int iV_AcceptCalibrationPoint()
        {
            return Unmanaged_AcceptCalibrationPoint();
        }

        private int iV_Calibrate()
        {
            return Unmanaged_Calibrate();
        }

        private int iV_ChangeCalibrationPoint(int number, int positionX, int positionY)
        {
            return Unmanaged_ChangeCalibrationPoint(number, positionX, positionY);
        }

        private int iV_ClearRecordingBuffer()
        {
            return Unmanaged_ClearRecordingBuffer();
        }

        private int iV_Connect(StringBuilder sendIP, int sendPort, StringBuilder receiveIP, int receivePort)
        {
            return Unmanaged_Connect(sendIP, sendPort, receiveIP, receivePort);
        }

        private int iV_ConnectLocal()
        {
            return Unmanaged_ConnectLocal();
        }

        private int iV_ContinueEyetracking()
        {
            return Unmanaged_ContinueEyetracking();
        }

        private int iV_ContinueRecording(StringBuilder trialname)
        {
            return Unmanaged_ContinueRecording(trialname);
        }

        private int iV_DeleteREDGeometry(StringBuilder name)
        {
            return Unmanaged_DeleteREDGeometry(name);
        }

        private int iV_DisableGazeDataFilter()
        {
            return Unmanaged_DisableGazeDataFilter();
        }

        private int iV_DisableProcessorHighPerformanceMode()
        {
            return Unmanaged_DisableProcessorHighPerformanceMode();
        }

        private int iV_Disconnect()
        {
            return Unmanaged_Disconnect();
        }

        private int iV_EnableGazeDataFilter()
        {
            return Unmanaged_EnableGazeDataFilter();
        }

        private int iV_EnableProcessorHighPerformanceMode()
        {
            return Unmanaged_EnableProcessorHighPerformanceMode();
        }

        private int iV_GetAccuracy(ref AccuracyStruct accuracyData, int visualization)
        {
            return Unmanaged_GetAccuracy(ref accuracyData, visualization);
        }

        private int iV_GetCalibrationParameter(ref CalibrationStruct calibrationData)
        {
            return Unmanaged_GetCalibrationParameter(ref calibrationData);
        }

        private int iV_GetCalibrationPoint(int calibrationPointNumber, ref CalibrationPointStruct calibrationPoint)
        {
            return Unmanaged_GetCalibrationPoint(calibrationPointNumber, ref calibrationPoint);
        }

        private int iV_GetCurrentCalibrationPoint(ref CalibrationPointStruct currentCalibrationPoint)
        {
            return Unmanaged_GetCurrentCalibrationPoint(ref currentCalibrationPoint);
        }

        private int iV_GetCurrentREDGeometry(ref REDGeometryStruct geometry)
        {
            return Unmanaged_GetCurrentREDGeometry(ref geometry);
        }

        private int iV_GetCurrentTimestamp(ref Int64 currentTimestamp)
        {
            return Unmanaged_GetCurrentTimestamp(ref currentTimestamp);
        }

        private int iV_GetEvent(ref EventStruct eventDataSample)
        {
            return Unmanaged_GetEvent(ref eventDataSample);
        }

        private int iV_GetFeatureKey(ref Int64 featureKey)
        {
            return Unmanaged_GetFeatureKey(ref featureKey);
        }

        private int iV_GetGeometryProfiles(int maxSize, ref StringBuilder profileNames)
        {
            return Unmanaged_GetGeometryProfiles(maxSize, ref profileNames);
        }

        private int iV_GetREDGeometry(StringBuilder profileName, ref REDGeometryStruct geometry)
        {
            return Unmanaged_GetREDGeometry(profileName, ref geometry);
        }

        private int iV_GetSample(ref SDKSampleStruct rawDataSample)
        {
            return Unmanaged_GetSample(ref rawDataSample);
        }

        private int iV_GetSerialNumber(ref StringBuilder serialNumber)
        {
            return Unmanaged_GetSerialNumber(ref serialNumber);
        }

        private int iV_GetDeviceName(ref StringBuilder serialNumber)
        {
            return Unmanaged_GetDeviceName(ref serialNumber);
        }

        private int iV_GetSystemInfo(ref SystemInfoStruct systemInfo)
        {
            return Unmanaged_GetSystemInfo(ref systemInfo);
        }

        private int iV_IsConnected()
        {
            return Unmanaged_IsConnected();
        }

        private int iV_LoadCalibration(StringBuilder name)
        {
            return Unmanaged_LoadCalibration(name);
        }

        private int iV_Log(StringBuilder message)
        {
            return Unmanaged_Log(message);
        }

        private int iV_PauseEyetracking()
        {
            return Unmanaged_PauseEyetracking();
        }

        private int iV_PauseRecording()
        {
            return Unmanaged_PauseRecording();
        }

        private int iV_Quit()
        {
            return Unmanaged_Quit();
        }

        private int iV_ResetCalibrationPoints()
        {
            return Unmanaged_ResetCalibrationPoints();
        }

        private int iV_SaveCalibration(StringBuilder name)
        {
            return Unmanaged_SaveCalibration(name);
        }

        private int iV_SaveData(StringBuilder filename, StringBuilder description, StringBuilder user, int overwrite)
        {
            return Unmanaged_SaveData(filename, description, user, overwrite);
        }

        private int iV_SendCommand(StringBuilder etMessage)
        {
            return Unmanaged_SendCommand(etMessage);
        }

        private int iV_SendImageMessage(StringBuilder message)
        {
            return Unmanaged_SendImageMessage(message);
        }

        private void iV_SetCalibrationCallback(MulticastDelegate calibrationCallback)
        {
            Unmanaged_SetCalibrationCallback(calibrationCallback);
        }

        private void iV_SetConnectionTimeout(int time)
        {
            Unmanaged_SetConnectionTimeout(time);
        }

        private int iV_SetGeometryProfile(StringBuilder profileName)
        {
            return Unmanaged_SetGeometryProfile(profileName);
        }

        private void iV_SetResolution(int stimulusWidth, int stimulusHeight)
        {
            Unmanaged_SetResolution(stimulusWidth, stimulusHeight);
        }

        private void iV_SetEventCallback(MulticastDelegate eventCallback)
        {
            Unmanaged_SetEventCallback(eventCallback);
        }

        private int iV_SetEventDetectionParameter(int minDuration, int maxDispersion)
        {
            return Unmanaged_SetEventDetectionParameter(minDuration, maxDispersion);
        }

        private void iV_SetEyeImageCallback(MulticastDelegate eyeImageCallback)
        {
            Unmanaged_SetEyeImageCallback(eyeImageCallback);
        }

        private int iV_SetLicense(StringBuilder key)
        {
            return Unmanaged_SetLicense(key);
        }

        private int iV_SetLogger(int logLevel, StringBuilder filename)
        {
            return Unmanaged_SetLogger(logLevel, filename);
        }

        private void iV_SetSampleCallback(MulticastDelegate sampleCallback)
        {
            Unmanaged_SetSampleCallback(sampleCallback);
        }

        private void iV_SetSceneVideoCallback(MulticastDelegate sceneVideoCallback)
        {
            Unmanaged_SetSceneVideoCallback(sceneVideoCallback);
        }

        private void iV_SetTrackingMonitorCallback(MulticastDelegate trackingMonitorCallback)
        {
            Unmanaged_SetTrackingMonitorCallback(trackingMonitorCallback);
        }

        private void iV_SetTrackingParameter(int ET_PARAM_EYE, int ET_PARAM, int value)
        {
            Unmanaged_SetTrackingParameter(ET_PARAM_EYE, ET_PARAM, value);
        }

        private int iV_SetupCalibration(ref CalibrationStruct calibrationData)
        {
            return Unmanaged_SetupCalibration(ref calibrationData);
        }

        private int iV_SetREDGeometry(ref REDGeometryStruct redGeometry)
        {
            return Unmanaged_SetREDGeometry(ref redGeometry);
        }

        private int iV_ShowAccuracyMonitor()
        {
            return Unmanaged_ShowAccuracyMonitor();
        }

        private int iV_ShowEyeImageMonitor()
        {
            return Unmanaged_ShowEyeImageMonitor();
        }

        private int iV_HideEyeImageMonitor()
        {
            return Unmanaged_HideEyeImageMonitor();
        }

        private int iV_ShowSceneVideoMonitor()
        {
            return Unmanaged_ShowSceneVideoMonitor();
        }

        private int iV_HideSceneVideoMonitor()
        {
            return Unmanaged_HideSceneVideoMonitor();
        }

        private int iV_HideAccuracyMonitor()
        {
            return Unmanaged_HideAccuracyMonitor();
        }

        private int iV_ShowTrackingMonitor()
        {
            return Unmanaged_ShowTrackingMonitor();
        }

        private int iV_HideTrackingMonitor()
        {
            return Unmanaged_HideTrackingMonitor();
        }

        private int iV_Start(int etApplication)
        {
            return Unmanaged_Start(etApplication);
        }

        private int iV_StartRecording()
        {
            return Unmanaged_StartRecording();
        }

        private int iV_StopRecording()
        {
            return Unmanaged_StopRecording();
        }

        private int iV_TestTTL(int value)
        {
            return Unmanaged_TestTTL(value);
        }

        private int iV_Validate()
        {
            return Unmanaged_Validate();
        }

        #endregion
    }

}
