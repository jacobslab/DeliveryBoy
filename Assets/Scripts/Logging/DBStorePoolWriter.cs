using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class DBStorePoolWriter {
	
	public void WriteStores(Store[] stores){
		string path = GetPath (Config.StoreFileName);
		StreamWriter sr = new StreamWriter (path);

		for (int i = 0; i < stores.Length; i++) {
#if GERMAN
			sr.WriteLine(stores[i].FullGermanName);
#else
			sr.WriteLine(stores[i].GetDisplayName());
#endif
		}

		sr.Flush ();
		sr.Close ();
	}

	public void WriteItemPool(Store[] stores){
		string path = GetPath (Config.PoolFileName);
		StreamWriter sr = new StreamWriter (path);

		for (int i = 0; i < stores.Length; i++) {
			for(int j = 0; j < stores[i].audioLeftToUse.Count; j++){
				sr.WriteLine(stores[i].audioLeftToUse[j].name);
			}
		}

		sr.Flush ();
		sr.Close ();
	}

	public void WriteItemPool(List<AudioClip> audioItemList){
		string path = GetPath (Config.PoolFileName);
		StreamWriter sr = new StreamWriter (path);
		
		for (int i = 0; i < audioItemList.Count; i++) {
			sr.WriteLine(audioItemList[i].name);
		}
		
		sr.Flush ();
		sr.Close ();
	}

	string GetPath(string fileName){
		string path = Experiment.Instance.SessionDirectory + fileName;
		return path;
	}
}
