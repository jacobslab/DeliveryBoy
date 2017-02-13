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

using UnityEngine;
using System.Collections;
using iView;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;
    
public class GazeModel
{
        /// <summary>
        /// Container for the EyeData
        /// </summary>
        public SampleData dataSample;


        #region systemData

        /// <summary>
        /// geometryinformation from the Client
        /// </summary>
        public REDGeometryStruct geometryInformation
        {
            get;
            private set;
        }

        /// <summary>
        /// Systeminformations from the Client
        /// </summary>
        public SystemInfoStruct systemInfo
        {
            get;
            private set;
        }

        public bool isStartingProcessOver { get; set; }
        public bool isCalibrationRunning { get; set; }
        public bool isValidationRunning { get; set; }

        public int statusID = -1; 

        public int calibrationMethod { get; set; }
        public long timeStamp { get; set; }
        public Vector2 gameScreenPosition { get; set; }

        /// <summary>
        /// ScreenSetup
        /// </summary>
        private int currentScreenHeight;
        private int currentScreenWidth;

        #endregion

        /// <summary>
        /// Setup the GazeModel and save the current ScreenResoultion
        /// </summary>
        /// <param name="currentScreenHeight"></param>
        /// <param name="currentScreenWidth"></param>
        public GazeModel(int currentScreenWidth, int currentScreenHeight)
        {
            this.currentScreenHeight = currentScreenHeight;
            this.currentScreenWidth = currentScreenWidth;

            //init a emptySample
            dataSample = new SampleData();
        }

        /// <summary>
        /// Get the Screenresolution
        /// </summary>
        /// <returns></returns>
        public Vector2 getScreenResolution()
        {
            return new Vector2(currentScreenWidth, currentScreenHeight);

        }

        /// <summary>
        /// Write the Data into the gazeModel
        /// </summary>
        /// <param name="leftEye"></param>
        /// <param name="rightEye"></param>
        /// <returns></returns>
        public bool WriteEyeSamplesInGazeModel(SDKSampleStruct dataSample)
        {
            try
            {
                Vector2 leftEyeGaze = new Vector2((float)dataSample.leftEye.gazeX, (float)dataSample.leftEye.gazeY);
                Vector3 leftEyePosition = new Vector3((float)dataSample.leftEye.eyePositionX, (float)dataSample.leftEye.eyePositionY, (float)dataSample.leftEye.eyePositionZ);

                Vector2 rightEyeGaze = new Vector2((float)dataSample.rightEye.gazeX, (float)dataSample.rightEye.gazeY);
                Vector3 rightEyePosition = new Vector3((float)dataSample.rightEye.eyePositionX, (float)dataSample.rightEye.eyePositionY, (float)dataSample.rightEye.eyePositionZ);

#if UNITY_EDITOR

                //Remove the Offset from the Topbar of the Editor
                leftEyeGaze.y -= 110f;
                rightEyeGaze.y -= 110f;                
#endif
                EyeSample leftEye = new EyeSample(leftEyeGaze, leftEyePosition, dataSample.leftEye.diam);
                EyeSample rightEye = new EyeSample(rightEyeGaze, rightEyePosition, dataSample.rightEye.diam);

                this.dataSample = new SampleData(leftEye, rightEye, dataSample.timestamp);
                
                return true;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogException(e);
                return false;
            }
        }

        /// <summary>
        /// Update the Systeminformations in the Model
        /// </summary>
        /// <param name="sysInfo"></param>
        /// <param name="geometryStruct"></param>
        /// <returns></returns>
        public bool WriteSystemInformationInGazeModel(SystemInfoStruct sysInfo, REDGeometryStruct geometryStruct)
        {
            try
            {
                systemInfo = sysInfo;
                geometryInformation = geometryStruct;
                return true;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogException(e);
                return false;
            }
        }
}