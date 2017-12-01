using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryExperiment : MonoBehaviour
{
    public RamulatorInterface ramulatorInterface;

    private static bool useRamulator;
    private static int sessionNumber;

    public static void ConfigureExperiment(bool newUseRamulator, int newSessionNumber)
    {
        useRamulator = newUseRamulator;
        sessionNumber = newSessionNumber;
    }

	void Start ()
    {
		
	}
	
	private IEnumerator ExperimentCoroutine()
    {
        if (useRamulator)
            yield return ramulatorInterface.BeginNewSession(sessionNumber);
    }
}