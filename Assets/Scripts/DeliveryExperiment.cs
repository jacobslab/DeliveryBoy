using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryExperiment : MonoBehaviour
{
    public RamulatorInterface ramulatorInterface;

    private static int sessionNumber = -1;
    private static bool useRamulator;

    public static void ConfigureExperiment(bool newUseRamulator, int newSessionNumber)
    {
        useRamulator = newUseRamulator;
        sessionNumber = newSessionNumber;
    }

	void Start ()
    {
        StartCoroutine(ExperimentCoroutine());
	}
	
	private IEnumerator ExperimentCoroutine()
    {
        if (sessionNumber == -1)
        {
            throw new UnityException("Please call ConfigureExperiment before beginning the experiment.");
        }

        if (useRamulator)
            yield return ramulatorInterface.BeginNewSession(sessionNumber);
    }
}