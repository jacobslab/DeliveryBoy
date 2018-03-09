using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pauser : MonoBehaviour
{

    public GameObject pauseScreen;
    public PlayerMovement playerMovement;
    public ScriptedEventReporter scriptedEventReporter;

    private bool paused = false;
    private bool allowPausing = false;
	
	void Update ()
    {
        if (allowPausing && Input.GetKeyDown(KeyCode.P) && !paused)
        {
            pauseScreen.SetActive(true);
            playerMovement.Freeze();
            paused = true;
            scriptedEventReporter.ReportScriptedEvent("pause", new Dictionary<string, object>());
        }
        else if (allowPausing && Input.GetKeyDown(KeyCode.P) && paused)
        {
            pauseScreen.SetActive(false);
            playerMovement.Unfreeze();
            paused = false;
            scriptedEventReporter.ReportScriptedEvent("unpause", new Dictionary<string, object>());
        }
	}

    public void AllowPausing()
    {
        allowPausing = true;
    }

    public void ForbidPausing()
    {
        allowPausing = false;
    }
}
