using UnityEngine;
using System.Collections;

public class EyetrackerLogTrack : LogTrack
{

    //currently just logs one point at a time.
    public void LogScreenGazePoint(Vector2 position, bool lowConfidence)
    {
        if (ExperimentSettings.isLogging)
        {
            subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "SCREEN_GAZE_POSITION" + separator + position.x + separator + position.y + separator + "LOW_CONFIDENCE" + separator + lowConfidence.ToString());
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
    public void LogWorldGazePoint(Vector3 position, bool lowConfidence)
    {
        if (ExperimentSettings.isLogging)
        {
            subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "WORLD_GAZE_POSITION" + separator + position.x + separator + position.y + separator + position.z + separator + "LOW_CONFIDENCE" + separator + lowConfidence.ToString());
        }
    }

    public void LogGazeObject(GameObject gazeObject)
    {
        if (ExperimentSettings.isLogging)
        {
            subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "GAZE_OBJECT" + separator + gazeObject.name);
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
