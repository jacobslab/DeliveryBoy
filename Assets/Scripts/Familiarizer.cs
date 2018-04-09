using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Familiarizer : MonoBehaviour
{

    public GameObject[] stores;
    public ScriptedEventReporter scriptedEventReporter;
    public DeliveryExperiment deliveryExperiment;

	public IEnumerator DoFamiliarization(float minISI, float maxISI, float presentationLength)
    {
        yield return new WaitForSeconds(Random.Range(minISI, maxISI));
        for (int i = 0; i < stores.Length; i++)
        {
            Dictionary<string, object> displayData = new Dictionary<string, object>();
            displayData.Add("store name", deliveryExperiment.GetStoreNameFromGameObjectName(stores[i].name));
            scriptedEventReporter.ReportScriptedEvent("familiarization store displayed", displayData);
            stores[i].SetActive(true);
            yield return new WaitForSeconds(presentationLength);
            scriptedEventReporter.ReportScriptedEvent("familiarization store cleared", displayData);
            stores[i].SetActive(false);
            yield return new WaitForSeconds(Random.Range(minISI, maxISI));
        }
    }
}
