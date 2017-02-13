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
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace iView
{

    public struct EyeData
    {
        public Vector3 positionEye { get; set; }
        public Vector2 positionGaze { get; set; }
        public float diameterPupil { get; set; }

        /// <summary>
        /// Init the EyeSample
        /// </summary>
        /// <param name="positionEye"></param>
        /// <param name="positionGaze"></param>
        /// <param name="diameterPupil"></param>
        public EyeData(Vector3 PositionEye, Vector2 PositionGaze, float DiameterPupil)
        {
            positionEye = PositionEye;
            positionGaze = PositionGaze;
            diameterPupil = DiameterPupil;
        }

    }

    // API Struct definition. See the manual for further description. 
    public struct SystemInfoStruct
    {
        public int samplerate;
        public int iV_MajorVersion;
        public int iV_MinorVersion;
        public int iV_Buildnumber;
        public int API_MajorVersion;
        public int API_MinorVersion;
        public int API_Buildnumber;
        public int iV_ETSystem;
    };

    public struct CalibrationPointStruct
    {
        public int number;
        public int positionX;
        public int positionY;
    };


    public struct SDKEyeDataStruct
    {
        public double gazeX;
        public double gazeY;
        public double diam;
        public double eyePositionX;
        public double eyePositionY;
        public double eyePositionZ;
    };


    public struct SDKSampleStruct
    {
        public Int64 timestamp;
        public SDKEyeDataStruct leftEye;
        public SDKEyeDataStruct rightEye;
        public int planeNumber;
    };


    public struct EventStruct
    {
        public char eventType;
        public char eye;
        public Int64 startTime;
        public Int64 endTime;
        public Int64 duration;
        public double positionX;
        public double positionY;
    };


    public struct AccuracyStruct
    {
        public double deviationXLeft;
        public double deviationYLeft;
        public double deviationXRight;
        public double deviationYRight;
    };


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct CalibrationStruct
    {
        public int method;
        public int visualization;
        public int displayDevice;
        public int speed;
        public int autoAccept;
        public int foregroundColor;
        public int backgroundColor;
        public int targetShape;
        public int targetSize;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string targetFilename;
    };


    public struct REDGeometryStruct
    {
        public REDGeometryEnum redGeometry;
        public int monitorSize;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string setupName;
        public int stimX;
        public int stimY;
        public int stimHeightOverFloor;
        public int redHeightOverFloor;
        public int redStimDist;
        public int redInclAngle;
        public int redStimDistHeight;
        public int redStimDistDepth;
    };

    public enum REDGeometryEnum
    {
        //! use monitor integrated mode
        monitorIntegrated = 0,

        //! use standalone mode
        standalone = 1
    };


    /// <summary>
    /// Select the Data from a Eye / both Eyes
    /// </summary>
    public enum Eye
    {
        MonocularLeft,
        MonocularRightEye,
        Biocular
    }

    public class EyeSample
    {
        private Vector2 gazePositon = Vector2.zero;
        public Vector3 eyePosition = Vector3.zero;
        public double pupilDiameter =0f; 
        
        public EyeSample(Vector2 gazePositon,Vector3 eyePosition,double pupilDiameter)
        {
            this.gazePositon = gazePositon;
            this.eyePosition = eyePosition;
            this.pupilDiameter = pupilDiameter;
        }

        public static EyeSample operator +(EyeSample e1,EyeSample e2)
        {
            Vector3 eyePosition = (e1.eyePosition + e2.eyePosition) / 2;
            Vector2 gazePosition = (e1.gazePositon + e2.gazePositon) / 2;
            double pupilDiameter = (e1.pupilDiameter + e2.pupilDiameter) / 2;

            return new EyeSample(gazePosition, eyePosition, pupilDiameter);
        }

        public Vector2 gazePosInUnityScreenCoords()
        {
            Vector2 gazePos = gazePositon;
            gazePos.y = Screen.height-gazePos.y;

            return gazePos;
        }

        public Vector2 gazePosInScreenCoords()
        {
            return gazePositon;
        }

        public Vector2 gazePosInViewPortCoords()
        {
            Vector2 result = gazePositon;
            result.x = result.x / Screen.width;
            result.y = result.y / Screen.height;
            return result ;
        }
    }

    public class SampleData
    {
        public EyeSample leftEye;
        public EyeSample rightEye;
        public EyeSample averagedEye;
        public double timeStamp;

        public SampleData(EyeSample leftEye,EyeSample rightEye,double timeStamp)
        {
            averagedEye = leftEye + rightEye;
            this.leftEye = leftEye;
            this.rightEye = rightEye;
            this.timeStamp = timeStamp; 
        }

        public SampleData()
        {
            averagedEye = new EyeSample(Vector2.zero,Vector3.zero,0.0);
            leftEye = new EyeSample(Vector2.zero, Vector3.zero, 0.0);
            rightEye = new EyeSample(Vector2.zero, Vector3.zero, 0.0);
            timeStamp = 0;
        }
    }
    /// <summary>
    /// Various Spaces of Unity
    /// </summary>
    public enum CoordinateSpace
    {
        UnityScreenSpace,
        ScreenSpace, 
        ViewPort
    }

    /// <summary>
    /// The Selectable Focus
    /// </summary>
    public enum FocusFilter
    {
        WorldSpaceObjects,
        SpriteObjects, 
        GUIObjects,
        NoFilter
    }

    public class IDContainerIviewNG {

        
        #region ErrorID
        public const int STATE_CONNECT = 0;
        public const int STATE_DISCONNET = 1;
        public const int STATE_SETUPCALIBRATION = 2;
        public const int STATE_CALIBRATE = 3;
        public const int STATE_SETUPVALIDATE = 4;
        public const int STATE_VALIDATE = 5;
        public const int STATE_PAUSE = 6;
        public const int STATE_CONTINUE = 7;
        public const int STATE_GAZEFILTER = 8;
        public const int STATE_GETGEOMETRYDATA = 9;
        public const int STATE_SYSTEMDATA = 10;

        public const int ACTION_COMPLETE = 1;
        public const int ACTION_CRITICALERROR= 99;
        #endregion

        #region ErrorMessages
        private static Dictionary<int, string> ErrorId = new Dictionary<int, string>()
            {
                {0, ""},
                {1, "Intended functionality has been fulfilled"},
                {2, "No new data available"},
                {3, "Calibration was aborted"},
                {100, "Failed to establish connection"},
                {101, "No connection established"},
                {102, "System is not calibrated"},
                {103, "System is not validated"},
                {104, "No SMI eye tracking application running"},
                {105, "Wrong port settings"},
                {111, "Eye tracking device required for this function is not connected"},
                {112, "Parameter out of range"},
                {113, "Eye tracking device required for this calibration method is not connected"},
                {121, "Failed to create sockets"},
                {122, "Failed to connect sockets"},
                {123, "Failed to bind sockets"},
                {124, "Failed to delete sockets"},
                {131, "No response from iViewX; check iViewX connection settings (IP addresses, ports) or last command"},
                {132, "iViewX version could not be resolved"},
                {133, "Wrong version of iViewX"},
                {171, "Failed to access log file"},
                {181, "Socket error during data transfer"},
                {191, "Recording buffer is empty"},
                {192, "Recording is activated"},
                {193, "Data buffer is full"},
                {194, "iViewX is not ready"},
                {201, "No installed iViewX detected"},
                {220, "Could not open port for TTL output"},
                {221, "Could not close port for TTL output"},
                {250,"Feature not licensed"},
                {300,"Function is deprecated"},
                {400,"Error at initialization"},
                {401,"Function not loaded"}
            };

        private static Dictionary<int, string> ErrorState = new Dictionary<int, string>()
            {
                {0, "Connecting"},
                {1, "Disconnecting"},
                {2, "Setup Calibration"},
                {3, "Running the Calibration"},
                {4, "Setup Validation"},
                {5, "Runnint the Validation"},
                {6, "Pause the System"},
                {7, "Continue"},
                {8, "Setup the internal GazeFilter"}
            };
        #endregion

        /// <summary>
        /// Print the Message depends on the ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string getErrorMessage(int id)
        {
            return ErrorId[id];
        }

        /// <summary>
        /// returns the name of the State in the Communication with the Server (e.G. Calibration, Initialisiation)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string getState(int id)
        {
            return ErrorState[id];
        }
    }
}