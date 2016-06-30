using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class DBStorePoolWriter {

	string FormatStringForWriting(string currString){
		string formattedString = currString.ToLower ();
		formattedString = formattedString.Replace (" ", "_");

		return formattedString;
	}

	public void WriteStores(Store[] stores){
		if (ExperimentSettings.isLogging) {
			string path = GetPath (Config.StoreFileName);
			StreamWriter sr = new StreamWriter (path);

			for (int i = 0; i < stores.Length; i++) {

#if GERMAN
			string storeName = stores[i].FullGermanName;
#else
				string storeName = stores [i].GetDisplayName ();
#endif
				storeName = FormatStringForWriting (storeName);
				sr.WriteLine (storeName);
			}

			sr.Flush ();
			sr.Close ();
		}
	}

	public void WriteItemPool(Store[] stores){
		if (ExperimentSettings.isLogging) {
			string path = GetPath (Config.PoolFileName);
			StreamWriter sr = new StreamWriter (path);

			for (int i = 0; i < stores.Length; i++) {
				for (int j = 0; j < stores[i].audioLeftToUse.Count; j++) {
					string itemName = stores [i].audioLeftToUse [j].name;
					itemName = FormatStringForWriting (itemName);
					sr.WriteLine (itemName);
				}
			}

			sr.Flush ();
			sr.Close ();
		}
	}

	public void WriteItemPool(List<AudioClip> audioItemList){
		if (ExperimentSettings.isLogging) {
			string path = GetPath (Config.PoolFileName);
			StreamWriter sr = new StreamWriter (path);
		
			for (int i = 0; i < audioItemList.Count; i++) {
				string itemName = audioItemList [i].name;
				itemName = FormatStringForWriting (itemName);
				sr.WriteLine (itemName);
			}
		
			sr.Flush ();
			sr.Close ();
		}
	}

	string GetPath(string fileName){
		string path = Experiment.Instance.SessionDirectory + fileName;
		return path;
	}
}
