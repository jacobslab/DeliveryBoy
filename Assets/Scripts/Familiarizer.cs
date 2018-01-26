using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Familiarizer : MonoBehaviour
{

    public GameObject[] stores;

	public IEnumerator DoFamiliarization(float minISI, float maxISI, float presentationLength)
    {
        yield return new WaitForSeconds(Random.Range(minISI, maxISI));
        for (int i = 0; i < stores.Length; i++)
        {
            stores[i].SetActive(true);
            yield return new WaitForSeconds(presentationLength);
            stores[i].SetActive(false);
            yield return new WaitForSeconds(Random.Range(minISI, maxISI));
        }
    }
}
