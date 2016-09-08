using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
public class ConfigReader : MonoBehaviour {

	public string[] germanNames;
	private string[] englishKeyNames;
	private string[] actualNames;

	private string pattern=":";
	// Use this for initialization
	void Start () {
#if GERMAN
		try{
		germanNames = File.ReadAllLines (System.IO.Directory.GetCurrentDirectory() +"/germanNames.txt");
		englishKeyNames = new string[germanNames.Length];
		actualNames=new string[germanNames.Length];
		for (int i=0; i<germanNames.Length; i++) {
			string[] temp= germanNames [i].Split (':');
			//Debug.Log (temp[0]);

			englishKeyNames[i]=temp[0];
			actualNames[i]=temp[1];
		}
		for (int j=0; j<englishKeyNames.Length; j++) {
			//Debug.Log(englishKeyNames[j]);
			if(gameObject.name==englishKeyNames[j])
			{
				GetComponent<Store>().FullGermanName=actualNames[j];
				GetComponent<Store>().ChangeToGerman();
				//Debug.Log(englishKeyNames[j]);
			}
		}
		}
		catch(FileNotFoundException e)
		{
			Debug.Log("file not found");
			//displaying editor version of German store names
			GetComponent<Store>().ChangeToGerman();
		}
#endif
	}
	
	// Update is called once per frame
	void Update () {
	//	Debug.Log (actualNames.Count);
		//for (int i=0; i<actualNames.Count; i++) {
	//		Debug.Log (actualNames[i]);
	//	}
	
	}
}
