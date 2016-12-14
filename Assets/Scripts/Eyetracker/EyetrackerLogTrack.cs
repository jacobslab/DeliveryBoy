using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class EyetrackerLogTrack : LogTrack
{

    Dictionary<string, float> buildingGazeTime = new Dictionary<string, float>();
    
    //currently just logs one point at a time.
    public void LogScreenGazePoint(Vector2 position, bool edgeConfidence, bool blinkConfidence)
    {
        if (ExperimentSettings.isLogging)
        {
            subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "SCREEN_GAZE_POSITION" + separator + position.x + separator + position.y + separator + "LOW_CONFIDENCE" + separator + edgeConfidence.ToString() + separator + "BLINK_UNCERTAINTY" + separator + blinkConfidence.ToString());
        }
    }

    public void LogCalibrationStarted(int calibrationPoints)
    {
        if (ExperimentSettings.isLogging)
        {
            subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "EYETRACKER_CALIBRATION_EVENT" + separator + calibrationPoints.ToString() + separator + "STARTED");
        }
    }

    public void LogCalibrationEnded(int calibrationPoints)
    {
        if (ExperimentSettings.isLogging)
        {
            subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "EYETRACKER_CALIBRATION_EVENT" + separator + calibrationPoints.ToString() + separator + "ENDED");
        }
    }
    public void LogWorldGazePoint(Vector3 position, bool edgeConfidence, bool blinkConfidence)
    {
        if (ExperimentSettings.isLogging)
        {
            subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "WORLD_GAZE_POSITION" + separator + position.x + separator + position.y + separator + position.z + separator + "EDGE_UNCERTAINTY" + separator + edgeConfidence.ToString() + separator + "BLINK_UNCERTAINTY" + separator + blinkConfidence.ToString());
        }
    }

    //looks only for designated buildings at the moment
    //logs object name, total gaze time and distance from current point
    public void LogGazeObject(GameObject gazeObject, float distance)
    {
        string objName = gazeObject.name;
        float val = 0f;
        if(!buildingGazeTime.ContainsKey(objName))
        {
            buildingGazeTime.Add(objName, 0f); 
        }
        else
        {
            val=buildingGazeTime[objName];
            val += Time.deltaTime;
            buildingGazeTime[objName] = val;
        }
        if (ExperimentSettings.isLogging)
        {
            subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "GAZE_OBJECT" + separator + gazeObject.name + separator+  "TOTAL_GAZE_TIME" + separator + val.ToString("F2") + separator + "GAZE_DISTANCE" + separator + distance.ToString("F2"));
        }
    }
    public void LogPupilDiameter(double leftPupilDiameter, double rightPupilDiameter, double averagedPupilDiameter)
    {
        if (ExperimentSettings.isLogging)
        {
            subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "PUPIL_DIAMETER" + separator + leftPupilDiameter.ToString("F3") + separator + rightPupilDiameter.ToString("F3") + separator + averagedPupilDiameter.ToString("F3"));
        }
    }
}
