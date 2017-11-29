using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PrintLists : MonoBehaviour {


	private Object[] germanAudio;
	// Use this for initialization
	void Start () {



		germanAudio = Resources.LoadAll ("StoreAudioGerman");

		string contents = "";
		for (int i = 0; i < germanAudio.Length; i++) {
			contents += germanAudio[i].name + "\n";
		}
		System.IO.File.WriteAllText ("/Users/anshpatel/Desktop/germanAudioDeliverables.txt", contents);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
